using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AiLogAnalyzer.API.Models;

public class LogEntry
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("logText")]
    public string LogText { get; set; } = string.Empty;

    [BsonElement("issue")]
    public string Issue { get; set; } = string.Empty;

    [BsonElement("severity")]
    public string Severity { get; set; } = string.Empty;

    [BsonElement("explanation")]
    public string Explanation { get; set; } = string.Empty;

    [BsonElement("solution")]
    public string Solution { get; set; } = string.Empty;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
