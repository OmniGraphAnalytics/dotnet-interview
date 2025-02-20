using System.Text.Json.Serialization;

namespace OmniGraphInterview.Models.OmniPulse.Events.Models;

/// <summary>
/// Shopify Event Detail
/// </summary>
/// <typeparam name="TPayload"></typeparam>
public record OmniPulseEventDetail<TPayload>
{
    /// <summary>
    /// Event Payload
    /// </summary>
    [JsonPropertyName("payload")]
    public TPayload? Payload { get; set; }

    /// <summary>
    /// Event metadata
    /// </summary>
    [JsonPropertyName("metadata")]
    public required OmniPulseEventMetadata Metadata { get; set; }

    /// <summary>
    /// ID of the event
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();
}
