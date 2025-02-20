using Microsoft.AspNetCore.Mvc;


namespace OmniGraphInterview.Exceptions;

/// <summary>
/// Thrown when an action is not accepted. (e.g. a user tries to delete a record that is not deletable)
/// </summary>
/// <param name="title"></param>
/// <param name="message"></param>
public class UnacceptedActionException(string title, string message) : AppErrorException(message)
{
    /// <inheritdoc />
    public override AppError GetClientError()
    {
        return new AppError
        {
            Title = title,
            DisplayMessage = Message,
        };
    }

    /// <inheritdoc />
    public override ObjectResult ToObjectResult()
    {
        return new BadRequestObjectResult(GetClientError());
    }
}
