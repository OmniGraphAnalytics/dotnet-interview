using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc;


namespace OmniGraphInterview.Exceptions;

/// <summary>
/// Thrown when a required field is missing.
/// </summary>
/// <param name="field"></param>
public class FieldRequiredException(string field) : AppErrorException($"The field '{field}' is required.")
{
    /// <inheritdoc />
    public override AppError GetClientError()
    {
        return new AppError
        {
            Title = "Field Required",
            DisplayMessage = $"The field '{field}' is required.",
        };
    }

    /// <inheritdoc />
    public override ObjectResult ToObjectResult()
    {
        return new BadRequestObjectResult(GetClientError());
    }

    /// <summary>
    /// Throws if argument is null
    /// </summary>
    /// <param name="argument"></param>
    /// <param name="paramName"></param>
    /// <exception cref="FieldRequiredException"></exception>
    public static void ThrowIfNull([NotNull] object? argument,
        [CallerArgumentExpression("argument")] string? paramName = null)
    {
        if (argument == null) throw new FieldRequiredException(paramName ?? "");
    }


    /// <summary>
    /// Throws if argument is null or empty string
    /// </summary>
    /// <param name="argument"></param>
    /// <param name="paramName"></param>
    /// <exception cref="FieldRequiredException"></exception>
    public static void ThrowIfNullOrEmptyString([NotNull] string? argument,
        [CallerArgumentExpression("argument")] string? paramName = null)
    {
        if (string.IsNullOrEmpty(argument)) throw new FieldRequiredException(paramName ?? "");
    }
}
