using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SkyBooker.FlightService.Models;

public sealed class Flight
{
    [BsonId] public ObjectId Id { get; set; }
    [BsonElement("flightId")] public string FlightId { get; set; } = "";
    [BsonElement("airlineName")] public string AirlineName { get; set; } = "";
    [BsonElement("source")] public string Source { get; set; } = "";
    [BsonElement("destination")] public string Destination { get; set; } = "";
    [BsonElement("departure_time")] public DateTime DepartureTime { get; set; }
    [BsonElement("arrival_time")] public DateTime ArrivalTime { get; set; }
    [BsonElement("available_seats")] public int AvailableSeats { get; set; }
    [BsonElement("created_at")] public DateTime CreatedAt { get; set; }
    [BsonElement("updated_at")] public DateTime UpdatedAt { get; set; }
}
