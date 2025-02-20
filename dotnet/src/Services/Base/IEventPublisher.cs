using Akka;
using OmniGraphInterview.Models.OmniPulse.Events.Models.Base;

namespace OmniGraphInterview.Services.Base;

/// <summary>
/// Interface for publishing events to the event bus
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// Publishes an event to the event bus for the given name
    /// </summary>
    /// <param name="name"></param>
    /// <param name="event"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    Task<Done> PublishEvent<T>(string name, T @event);

    /// <summary>
    /// Publishes an event to the event bus
    /// </summary>
    /// <param name="event"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    Task<Done> PublishEvent<T>(T @event) where T : IOmniPulseEvent;
}
