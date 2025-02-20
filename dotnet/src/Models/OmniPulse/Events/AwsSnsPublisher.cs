using System.Text.Json;
using Akka;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.Extensions.Logging;
using OmniGraphInterview.Constants.JsonSerializerSettings;
using OmniGraphInterview.Extensions;
using OmniGraphInterview.Models.OmniPulse.Events.Models;
using OmniGraphInterview.Models.OmniPulse.Events.Models.Base;
using OmniGraphInterview.Services.Base;
using SnD.Sdk.Extensions.Environment.Hosting;
using Snd.Sdk.Metrics.Base;

namespace OmniGraphInterview.Models.OmniPulse.Events;

/// <summary>
/// Publisher for publishing events to AWS SNS (primary event bus)
/// </summary>
/// <param name="client"></param>
/// <param name="metricsService"></param>
/// <param name="logger"></param>
public class AwsSnsPublisher(
    IAmazonSimpleNotificationService client,
    MetricsService metricsService,
    ILogger<AwsSnsPublisher> logger) : IEventPublisher
{
    private readonly string topicArn = EnvironmentExtensions.GetDomainEnvironmentVariable("AWS_EVENTBUS_ARN")
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
        try
        {
            var res = await client.PublishAsync(new PublishRequest
            {
                Message = JsonSerializer.Serialize(value: new OmniPulseEventDetail<T>
                {
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
                MessageDeduplicationId = Guid.NewGuid().ToString(),
                MessageGroupId = name,
                TopicArn = topicArn,
            });
            if (res.HttpStatusCode != System.Net.HttpStatusCode.OK)
                logger.LogCritical(message: "Failed to publish event {EventName} to SNS with error {StatusCode}", name,
                    res.HttpStatusCode);
        }
        catch (Exception e)
        {
            logger.LogCritical(exception: e, message: "Failed to publish event {EventName} to SNS", name);
            throw;
        }

        return Done.Instance;
    }
}
