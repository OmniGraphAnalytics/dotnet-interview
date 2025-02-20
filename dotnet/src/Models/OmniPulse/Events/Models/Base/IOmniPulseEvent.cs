namespace OmniGraphInterview.Models.OmniPulse.Events.Models.Base;

/// <summary>
///
/// </summary>
public interface IOmniPulseEvent
{
    /// <summary>
    /// General identifier for the event type (constant)
    /// </summary>
    public string EventName { get; }

    /// <summary>
    /// Identifier for the specific event for subsequent triggering (e.g., account/created, tier/changed/{newTier})
    /// </summary>
    public string EventId { get; }
}
