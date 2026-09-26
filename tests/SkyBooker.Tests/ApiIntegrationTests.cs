using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using SkyBooker.AuthService.Contracts;
using SkyBooker.BookingService.Contracts;
using SkyBooker.FlightService.Contracts;

namespace SkyBooker.Tests;

public sealed class ApiIntegrationTests(ApiHost host) : IClassFixture<ApiHost>
{
    private async Task<TokenResponse> RegisterAsync()
    {
        string username="user"+Guid.NewGuid().ToString("N")[..12];
        var response=await host.Client.PostAsJsonAsync("/api/register",new RegisterRequest(username,username+"@example.com","SkyBooker!2026"));
        Assert.Equal(HttpStatusCode.Created,response.StatusCode);
        Assert.DoesNotContain("password",await response.Content.ReadAsStringAsync(),StringComparison.OrdinalIgnoreCase);
        var login=await host.Client.PostAsJsonAsync("/api/login",new LoginRequest(username,"SkyBooker!2026"));
        Assert.Equal(HttpStatusCode.OK,login.StatusCode);
        return (await login.Content.ReadFromJsonAsync<TokenResponse>())!;
    }
    private async Task<HttpResponseMessage> SendAsync(HttpMethod method,string path,string? token=null,object? body=null)
    {
        using var request=new HttpRequestMessage(method,path);
        if(token is not null)request.Headers.Authorization=new("Bearer",token);
        if(body is not null)request.Content=JsonContent.Create(body);
        return await host.Client.SendAsync(request);
    }
    private async Task<FlightResponse> FlightAsync(string token)
    {
        string number="LX"+Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        var response=await SendAsync(HttpMethod.Post,"/api/flight",token,new CreateFlightRequest(number,"SWISS","Basel","London",DateTimeOffset.UtcNow.AddDays(3),DateTimeOffset.UtcNow.AddDays(3).AddHours(2),100));
        Assert.Equal(HttpStatusCode.Created,response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        return (await response.Content.ReadFromJsonAsync<FlightResponse>())!;
    }
    [Theory]
    [InlineData("/api/flight")]
    [InlineData("/api/booking")]
    public async Task MissingTokenIsUnauthorized(string path) => Assert.Equal(HttpStatusCode.Unauthorized,(await host.Client.GetAsync(path)).StatusCode);
    [Fact]
    public async Task BasicGetLoginWorksWithoutQueryCredentials()
    {
        var account=await RegisterAsync();
        using var request=new HttpRequestMessage(HttpMethod.Get,"/api/login");
        request.Headers.Authorization=new("Basic",Convert.ToBase64String(Encoding.UTF8.GetBytes(account.Username+":SkyBooker!2026")));
        var response=await host.Client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK,response.StatusCode);
        Assert.True(response.Headers.CacheControl!.NoStore);
    }
    [Theory]
    [InlineData("Basic invalid!")]
    [InlineData("Bearer nope")]
    public async Task InvalidBasicHeaderIsUnauthorized(string authorization)
    {
        using var request=new HttpRequestMessage(HttpMethod.Get,"/api/login");request.Headers.TryAddWithoutValidation("Authorization",authorization);
        Assert.Equal(HttpStatusCode.Unauthorized,(await host.Client.SendAsync(request)).StatusCode);
    }
    [Theory]
    [InlineData("expired")]
    [InlineData("issuer")]
    [InlineData("audience")]
    [InlineData("signature")]
    [InlineData("algorithm")]
    public async Task InvalidJwtIsUnauthorized(string reason)
    {
        var now=DateTime.UtcNow;
        var jwt=new JwtSecurityToken(issuer:reason=="issuer"?"wrong":"SkyBooker.Auth",audience:reason=="audience"?"wrong":"SkyBooker.Services",
            notBefore:now.AddMinutes(-10),expires:reason=="expired"?now.AddMinutes(-1):now.AddMinutes(10),
            signingCredentials:new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(reason=="signature"?new string('x',88):host.Key)),reason=="algorithm"?SecurityAlgorithms.HmacSha256:SecurityAlgorithms.HmacSha512));
        var response=await SendAsync(HttpMethod.Get,"/api/flight",new JwtSecurityTokenHandler().WriteToken(jwt));
        Assert.Equal(HttpStatusCode.Unauthorized,response.StatusCode);
    }
    [Fact]
    public async Task BookingUsesHttpFlightLookupAndRestrictsOwnership()
    {
        var owner=await RegisterAsync();var stranger=await RegisterAsync();var flight=await FlightAsync(owner.Token);
        var body=new CreateBookingRequest(flight.FlightId,owner.PassengerId,"Felipe","Carvalho",2);
        var created=await SendAsync(HttpMethod.Post,"/api/booking",owner.Token,body);
        Assert.Equal(HttpStatusCode.Created,created.StatusCode);
        int id=JsonDocument.Parse(await created.Content.ReadAsStringAsync()).RootElement.GetProperty("id").GetInt32();
        Assert.Equal(HttpStatusCode.OK,(await SendAsync(HttpMethod.Get,"/api/booking/"+id,owner.Token)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,(await SendAsync(HttpMethod.Get,"/api/booking/"+id,stranger.Token)).StatusCode);
        var others=await SendAsync(HttpMethod.Get,"/api/booking",stranger.Token);
        Assert.Equal("[]",await others.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.Forbidden,(await SendAsync(HttpMethod.Post,"/api/booking",stranger.Token,body)).StatusCode);
    }
    [Fact]
    public async Task MissingFlightDoesNotCreateBooking()
    {
        var owner=await RegisterAsync();
        var result=await SendAsync(HttpMethod.Post,"/api/booking",owner.Token,new CreateBookingRequest("ZZ404",owner.PassengerId,"Felipe","Carvalho",1));
        Assert.Equal(HttpStatusCode.UnprocessableEntity,result.StatusCode);
        Assert.Equal("[]",await (await SendAsync(HttpMethod.Get,"/api/booking",owner.Token)).Content.ReadAsStringAsync());
    }
    [Fact]
    public async Task InvalidRequestReturnsFieldErrors()
    {
        var owner=await RegisterAsync();var result=await SendAsync(HttpMethod.Post,"/api/booking",owner.Token,new CreateBookingRequest("LX123",owner.PassengerId,"Felipe","Carvalho",0));
        Assert.Equal(HttpStatusCode.BadRequest,result.StatusCode);
        Assert.Contains("TicketCount",await result.Content.ReadAsStringAsync());
    }
    [Fact]
    public async Task FlightHasIdCanBeReadAndDuplicateIsConflict()
    {
        var owner=await RegisterAsync();var flight=await FlightAsync(owner.Token);
        Assert.Equal(24,flight.Id.Length);
        Assert.Equal(HttpStatusCode.OK,(await SendAsync(HttpMethod.Get,"/api/flight/"+flight.Id,owner.Token)).StatusCode);
        var repeat=new CreateFlightRequest(flight.FlightId,"SWISS","Basel","London",DateTimeOffset.UtcNow.AddDays(3),DateTimeOffset.UtcNow.AddDays(3).AddHours(2),100);
        Assert.Equal(HttpStatusCode.Conflict,(await SendAsync(HttpMethod.Post,"/api/flight",owner.Token,repeat)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,(await SendAsync(HttpMethod.Get,"/api/flight/not-an-id",owner.Token)).StatusCode);
    }
    [Fact]
    public async Task OpenApiContainsAllRequiredRoutes()
    {
        var response=await host.Client.GetAsync("/swagger/v1/swagger.json");Assert.Equal(HttpStatusCode.OK,response.StatusCode);
        var spec=JsonDocument.Parse(await response.Content.ReadAsStringAsync());var paths=spec.RootElement.GetProperty("paths");
        foreach(var path in new[]{"/api/register","/api/login","/api/flight","/api/flight/{id}","/api/booking","/api/booking/{id}"})Assert.True(paths.TryGetProperty(path,out _));
    }
}
