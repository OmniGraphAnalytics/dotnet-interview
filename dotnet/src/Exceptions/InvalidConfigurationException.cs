using Microsoft.AspNetCore.Mvc;


namespace OmniGraphInterview.Exceptions;

/// <summary>
/// Thrown when a configuration is invalid.
/// </summary>
/// <param name="message"></param>
public class InvalidConfigurationException(string message) : AppErrorException(message)
{
    /// <inheritdoc />
    public override AppError GetClientError()
    {
        return new AppError
        {
            Title = "Invalid Configuration",
            DisplayMessage = "There was an issue with the configuration. Please verify your settings and try again.",
        };
    }

    /// <inheritdoc />
    public override ObjectResult ToObjectResult()
    {
        return new BadRequestObjectResult(GetClientError());
    }
}
