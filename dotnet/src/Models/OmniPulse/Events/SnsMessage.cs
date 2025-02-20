using System.Text.Json;
using OmniGraphInterview.Constants.JsonSerializerSettings;
using OmniGraphInterview.Models.OmniPulse.Events.Models;

namespace OmniGraphInterview.Models.OmniPulse.Events;

/// <summary>
/// SNS Message
/// </summary>
/// <typeparam name="T"></typeparam>
public class SnsMessage<T> where T : new()
{
    /// <summary>
    /// Message Type
    /// </summary>
    public required string Type { get; set; }

    /// <summary>
    /// Message Id
    /// </summary>
    public required string MessageId { get; set; }

    /// <summary>
    /// Arn of the SNS topic
    /// </summary>
    public required string TopicArn { get; set; }

    /// <summary>
    /// Message body
    /// </summary>
    public required string Message { get; set; }

    /// <summary>
    /// Timestamp of the message
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>
    /// Unsubscribe URL (for email notifications -- not used)
    /// </summary>
    public string? UnsubscribeURL { get; set; }

    /// <summary>
    /// Parse the message body as an OmniPulseEventDetail
    /// </summary>
    public OmniPulseEventDetail<T>? Detail =>
        JsonSerializer.Deserialize<OmniPulseEventDetail<T>>(json: Message, options: Serializers.CqlSerializers);
}
