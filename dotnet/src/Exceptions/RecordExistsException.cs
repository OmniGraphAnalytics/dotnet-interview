using System.Net;
using Microsoft.AspNetCore.Mvc;


namespace OmniGraphInterview.Exceptions;

/// <summary>
/// Exception thrown when a record already exists.
/// </summary>
/// <param name="recordId"></param>
/// <param name="type"></param>
public class RecordExistsException(string recordId, string type)
    : AppErrorException($"Record with ID '{recordId}' already exists.")
{
    /// <inheritdoc />
    public override AppError GetClientError()
    {
        return new AppError
        {
            Title = "Record Already Exists",
            DisplayMessage = $"The '{type}' you are trying to create with id '{recordId}' already exists.",
        };
    }

    /// <inheritdoc />
    public override ObjectResult ToObjectResult()
    {
        return new ObjectResult(GetClientError())
        {
            StatusCode = (int)HttpStatusCode.Conflict,
        };
    }
}
