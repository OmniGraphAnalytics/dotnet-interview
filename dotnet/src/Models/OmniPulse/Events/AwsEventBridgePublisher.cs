using System.Text.Json;
using Akka;
using Amazon.EventBridge;
using Amazon.EventBridge.Model;
using Microsoft.Extensions.Logging;
using OmniGraphInterview.Constants;
using OmniGraphInterview.Constants.JsonSerializerSettings;
using OmniGraphInterview.Extensions;
using OmniGraphInterview.Models.OmniPulse.Events.Models;
using OmniGraphInterview.Models.OmniPulse.Events.Models.Base;
using OmniGraphInterview.Services.Base;
using SnD.Sdk.Extensions.Environment.Hosting;
using Snd.Sdk.Metrics.Base;

namespace OmniGraphInterview.Models.OmniPulse.Events;

/// <summary>
/// Publisher for publishing events to AWS EventBridge (not primary event bus)
/// </summary>
/// <param name="eventBridge"></param>
/// <param name="metricsService"></param>
/// <param name="logger"></param>
public class AwsEventBridgePublisher(
    AmazonEventBridgeClient eventBridge,
    MetricsService metricsService,
    ILogger<AwsEventBridgePublisher> logger) : IEventPublisher
{
    private readonly string eventBusArn = EnvironmentExtensions.GetDomainEnvironmentVariable("AWS_EVENTBUS_ARN")
                                          ?? throw new Exception("AWS_EVENTBUS_ARN is not set");

    /// <inheritdoc />
    public Task<Done> PublishEvent<T>(T @event) where T : IOmniPulseEvent
    {
        return PublishEvent(name: @event.EventName, @event: @event);
    }

    /// <inheritdoc />
    public async Task<Done> PublishEvent<T>(string name, T @event)
    {
        metricsService.Increment(metricName: "event_triggered", tags: new SortedDictionary<string, string>
        {
            { "event_name", name },
            { "source", AppDomain.CurrentDomain.FriendlyName },
        });
        var res = await eventBridge.PutEventsAsync(new PutEventsRequest
        {
            Entries =
            [
                new PutEventsRequestEntry
                {
                    EventBusName = eventBusArn,
                    Detail = JsonSerializer.Serialize(value: new OmniPulseEventDetail<T>
                    {
                        Id = Guid.NewGuid().ToString(),
                        Metadata = new OmniPulseEventMetadata
                        {
                            EventName = name,
                            Source = AppDomain.CurrentDomain.FriendlyName,
                            Version = OmniPulseEnvironmentExtensions.AppVersion,
                            Timestamp = DateTimeOffset.UtcNow,
                            Host = Environment.MachineName,
                            Environment = OmniPulseEnvironmentExtensions.AppEnvironment,
                        },
                        Payload = @event,
                    }, options: Serializers.CqlSerializers),
                    DetailType = EventConstants.OmniPulseEventDetailType,
                    Source = AppDomain.CurrentDomain.FriendlyName,
                    Time = DateTime.Now,
                },
            ],
        });

        if (res.HttpStatusCode != System.Net.HttpStatusCode.OK)
            logger.LogCritical(message: "Failed to publish event {EventName} to EventBridge", name);

        return Done.Instance;
    }
}
