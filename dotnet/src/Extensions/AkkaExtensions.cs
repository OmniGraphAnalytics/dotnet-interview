using Akka.Streams.Dsl;
using Akka.Util;
using Snd.Sdk.Tasks;

namespace OmniGraphInterview.Extensions;

/// <summary>
/// Extensions for Akka.NET streams.
/// </summary>
public static class AkkaExtensions
{
    /// <summary>
    /// Filter out null values from a flow and update the nullability of the flow.
    /// </summary>
    /// <param name="flow"></param>
    /// <typeparam name="TOut"></typeparam>
    /// <typeparam name="TMat"></typeparam>
    /// <returns></returns>
    public static Source<TOut, TMat> WhereNotNull<TOut, TMat>(this Source<TOut?, TMat> flow)
    {
        return flow.Where(x => x is not null).Select(x => x!);
    }

    /// <summary>
    /// Filter out null values from a flow and update the nullability of the flow.
    /// </summary>
    /// <param name="flow"></param>
    /// <typeparam name="TIn"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    /// <typeparam name="TMat"></typeparam>
    /// <returns></returns>
    public static Flow<TIn, TOut, TMat> WhereNotNull<TIn, TOut, TMat>(this Flow<TIn, TOut?, TMat> flow)
    {
        return flow.Where(x => x is not null).Select(x => x!);
    }

    /// <summary>
    /// Extension method to map async function to the stream that filters results that are null to avoid stream errors
    /// </summary>
    /// <param name="flow"></param>
    /// <param name="parallelism"></param>
    /// <param name="asyncMapper"></param>
    /// <typeparam name="TIn"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    /// <typeparam name="TMat"></typeparam>
    /// <returns></returns>
    public static Source<TOut, TMat> SelectAsyncNullable<TIn, TOut, TMat>(this Source<TIn, TMat> flow, int parallelism,
        Func<TIn, Task<TOut?>> asyncMapper)
    {
        return flow
            .SelectAsync(parallelism: parallelism, asyncMapper: x => asyncMapper(x)
                .Map(nullableRes => nullableRes != null
                    ? Option<TOut>.Create(nullableRes)
                    : Option<TOut>.None))
            .Where(x => x.HasValue).Select(x => x.Value);
    }
}
