namespace OmniGraphInterview.Extensions;

/// <summary>
/// Extensions for the OmniPulse environment
/// </summary>
public static class OmniPulseEnvironmentExtensions
{
    /// <summary>
    /// Get the application environment from the environment variables
    /// </summary>
    /// <exception cref="Exception"></exception>
    public static string AppEnvironment => Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                                           ?? throw new Exception("ASPNETCORE_ENVIRONMENT is not set");

    /// <summary>
    /// Get the application version from the environment variables
    /// </summary>
    /// <exception cref="Exception"></exception>
    public static string AppVersion => Environment.GetEnvironmentVariable("DD_VERSION")
                                       ?? throw new Exception("DD_VERSION is not set");

    /// <summary>
    /// Check if the current environment is production or not
    /// </summary>
    /// <returns></returns>
    public static bool IsProduction()
    {
        return string.Equals(a: Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
            b: "production", comparisonType: StringComparison.InvariantCultureIgnoreCase);
    }
}
