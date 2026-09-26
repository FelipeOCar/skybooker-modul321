using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SkyBooker.Shared;

public sealed class SwaggerSecurityFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        string? scheme = context.ApiDescription.RelativePath == "api/login" && context.ApiDescription.HttpMethod == "GET" ? "Basic" : null;
        var attributes = context.MethodInfo.GetCustomAttributes(true).Concat(context.MethodInfo.DeclaringType?.GetCustomAttributes(true) ?? []);
        if (scheme is null && attributes.OfType<AuthorizeAttribute>().Any() && !attributes.OfType<AllowAnonymousAttribute>().Any()) scheme = "Bearer";
        if (scheme is null) return;
        operation.Security = [new OpenApiSecurityRequirement { [new OpenApiSecurityScheme
            { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = scheme } }] = Array.Empty<string>() }];
    }
}
