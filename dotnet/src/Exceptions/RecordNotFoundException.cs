using Microsoft.AspNetCore.Mvc;


namespace OmniGraphInterview.Exceptions;

/// <summary>
/// Exception thrown when a record is not found.
/// </summary>
/// <param name="recordType"></param>
public class RecordNotFoundException(string recordType)
    : AppErrorException($"No {recordType} found with the provided identifier(s).")
{
    /// <inheritdoc />
    public override AppError GetClientError()
    {
        return new AppError
        {
            Title = "Record Not Found",
            DisplayMessage = $"No {recordType} found with the provided identifier(s).",
        };
    }

    /// <inheritdoc />
    public override ObjectResult ToObjectResult()
    {
        return new NotFoundObjectResult(GetClientError());
    }

    /// <summary>
    /// Throws a RecordNotFoundException if the provided record is null.
    /// </summary>
    /// <param name="record"></param>
    /// <param name="recordType"></param>
    /// <typeparam name="T"></typeparam>
    /// <exception cref="RecordNotFoundException"></exception>
    public static void ThrowIfNull<T>(T? record, string? recordType = null)
        where T : class
    {
        if (record is null) throw new RecordNotFoundException(recordType ?? typeof(T).Name);
    }
}
