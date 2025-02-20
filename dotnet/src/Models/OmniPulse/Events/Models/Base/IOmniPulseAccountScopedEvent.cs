namespace OmniGraphInterview.Models.OmniPulse.Events.Models.Base;

/// <summary>
/// Interface for events that are scoped to an account
/// </summary>
public interface IOmniPulseAccountScopedEvent : IOmniPulseEvent
{
    /// <summary>
    /// AccountId of the event
    /// </summary>
    public string AccountId { get; set; }
}
