namespace OmniGraphInterview.Constants;

/// <summary>
/// Event constants for OmniPulse events
/// </summary>
public static class EventConstants
{
    /// <summary>
    /// Event Type for OmniPulse events
    /// </summary>
    public const string OmniPulseEventDetailType = "OmniPulseEvent";

    /// <summary>
    /// Event when user is invited to account
    /// </summary>
    public const string UserInvited = "user/invited";

    /// <summary>
    /// Event when new account is registered
    /// </summary>
    public const string AccountCreated = "account/created";

    /// <summary>
    /// Event on tier change for existing account
    /// </summary>
    public const string TierChanged = "tier/changed";

    /// <summary>
    /// Event when tier quota is updated
    /// </summary>
    public const string TierQuotaUpdate = "tier/quota-update";

    /// <summary>
    /// Event when account is deleted
    /// </summary>
    public const string AccountDeleted = "account/deleted";

    /// <summary>
    /// Event when a single stream finalizes initialization (back-fill)
    /// </summary>
    public const string StreamInitializingCompleted = "stream-initializing/completed";

    /// <summary>
    /// Event when a customer's streams have all finished and the platform is ready for analytics
    /// </summary>
    public const string InitialCustomerSyncCompleted = "initial-customer-sync/completed";

    /// <summary>
    /// Event when a job is rolled out (created, updated, or disabled)
    /// </summary>
    public const string JobRollout = "job/rollout";

    /// <summary>
    /// Event when an integration is rolled out (created, updated, or disabled)
    /// </summary>
    public const string IntegrationRollout = "integration/rollout";

    /// <summary>
    /// Event when a recommendation's state is changed (accepted, rejected, etc.)
    /// </summary>
    public const string ObjectScopedRecommendationV2StateChanged =
        "recommendation/state-changed/object_scoped_recommendation_v2";

    /// <summary>
    /// Event when a job is completed (successfully). Will have format "job/completed/{JobId}"
    /// </summary>
    public const string JobCompleted = "job/completed";

    /// <summary>
    /// Event when a job fails. Will have format "job/failed/{JobId}"
    /// </summary>
    public const string JobFailed = "job/failed";

    /// <summary>
    /// Event when a job is skipped. Will have format "job/skipped/{JobId}"
    /// </summary>
    public const string JobSkipped = "job/skipped";

    /// <summary>
    /// Event when a job tracking is cancelled. Will have format "job/tracking-cancelled/{JobId}"
    /// </summary>
    public const string JobTrackingCancelled = "job/tracking-cancelled";
}
