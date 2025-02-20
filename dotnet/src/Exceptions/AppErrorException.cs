using Microsoft.AspNetCore.Mvc;


namespace OmniGraphInterview.Exceptions;

/// <summary>
/// Exception thrown that represents an error in the application that can be presented to the client (REST API).
/// </summary>
public abstract class AppErrorException(string message) : Exception(message)
{
    /// <summary>
    /// Method to extract the error that can be sent to the client.
    /// </summary>
    /// <returns></returns>
    public abstract AppError GetClientError();

    /// <summary>
    /// Method to extract the error that can be sent to the client.
    /// </summary>
    /// <returns></returns>
    public virtual ObjectResult ToObjectResult()
    {
        return new BadRequestObjectResult(GetClientError());
    }
}
