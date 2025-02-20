using Akka;

using Snd.Sdk.Tasks;

namespace OmniGraphInterview.Exceptions;

/// <summary>
/// Thrown when a CQL update operation fails.
/// </summary>
/// <param name="message"></param>
public class CqlUpdateFailedException(string? message) : AppErrorException(message ?? "Failed to update record")
{
    /// <inheritdoc />
    public override AppError GetClientError()
    {
        return new AppError
        {
            Title = "Failed to update record",
            DisplayMessage = "Failed to update record in the database",
            Cause = Message,
        };
    }
}

/// <summary>
/// Extension methods for CQL update tasks.
/// </summary>
public static class CqlUpdateTaskExtension
{
    /// <summary>
    /// Throws a <see cref="CqlUpdateFailedException"/> if the task result is false.
    /// </summary>
    /// <param name="task"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    /// <exception cref="CqlUpdateFailedException"></exception>
    public static Task<Done> ThrowCqlErrorIfFalse(this Task<bool> task, string? message = null)
    {
        return task.Map(result =>
            result
                ? Done.Instance
                : throw new CqlUpdateFailedException(message));
    }

    /// <summary>
    /// Throws a <see cref="CqlUpdateFailedException"/> if the task result is false.
    /// </summary>
    /// <param name="task"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="RecordNotFoundException"></exception>
    public static Task<T> ThrowIfNotFound<T>(this Task<T?> task)
    {
        return task.Map(result => result ?? throw new RecordNotFoundException(typeof(T).Name));
    }
}
