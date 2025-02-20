namespace OmniGraphInterview.Exceptions;

/// <summary>
/// Represents an error in the application that is sent to the client (REST API).
/// </summary>
public record AppError
{
    /// <summary>
    /// The title of the error
    /// </summary>
    public string? Title { get; init; } = "Error";

    /// <summary>
    /// The display message of the error
    /// </summary>
    public string? DisplayMessage { get; init; } = "An error occurred";

    /// <summary>
    /// Error cause
    /// </summary>
    public string? Cause { get; init; }
}