using System.Text.Json.Serialization;

namespace OmniGraphInterview.Models.OmniPulse.Events.Models;

/// <summary>
/// Event Metadata for OmniPulse Events in event bus
/// </summary>
public record OmniPulseEventMetadata
{
    /// <summary>
    /// Event Name (type of event)
    /// </summary>
    [JsonPropertyName("EventName")]
    public required string EventName { get; set; }

    /// <summary>
    /// Source of the event
    /// </summary>
    [JsonPropertyName("Source")]
    public required string Source { get; set; }

    /// <summary>
    /// Version of the source system
    /// </summary>
    [JsonPropertyName("Version")]
    public required string Version { get; set; }

    /// <summary>
    /// Timestamp of the event
    /// </summary>
    [JsonPropertyName("Timestamp")]
    public required DateTimeOffset Timestamp { get; set; }

    /// <summary>
    /// Host machine of the event
    /// </summary>
    [JsonPropertyName("Host")]
    public required string Host { get; set; }

    /// <summary>
    /// Host environment of the event
    /// </summary>
    [JsonPropertyName("Environment")]
    public required string Environment { get; set; }
}
