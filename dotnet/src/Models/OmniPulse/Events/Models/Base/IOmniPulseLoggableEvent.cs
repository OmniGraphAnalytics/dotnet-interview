using Microsoft.Extensions.Logging;
using Snd.Sdk.Metrics.Base;

namespace OmniGraphInterview.Models.OmniPulse.Events.Models.Base;

/// <summary>
/// Interface for events that can be logged
/// </summary>
public interface IOmniPulseLoggableEvent : IOmniPulseEvent
{
    /// <summary>
    /// Log the event
    /// </summary>
    /// <param name="logger"></param>
    public void Log(ILogger logger);

    /// <summary>
    /// Send metrics for the event
    /// </summary>
    /// <param name="metricsService"></param>
    public void SendMetrics(MetricsService metricsService);
}
