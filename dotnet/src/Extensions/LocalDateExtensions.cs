using Cassandra;

namespace OmniGraphInterview.Extensions;

/// <summary>
/// Extensions for LocalDate (Cassandra type)
/// </summary>
public static class LocalDateExtensions
{
    /// <summary>
    /// Converts a DateTimeOffset to a LocalDate (LocalDate is always set in UTC)
    /// </summary>
    /// <param name="dateTimeOffset"></param>
    /// <returns></returns>
    public static LocalDate ToLocalDate(this DateTimeOffset dateTimeOffset)
    {
        return new LocalDate(year: dateTimeOffset.UtcDateTime.Year, month: dateTimeOffset.UtcDateTime.Month,
            day: dateTimeOffset.UtcDateTime.Day);
    }

    /// <summary>
    /// Converts a LocalDate to a DateTimeOffset (LocalDate is always assumed UTC)
    /// </summary>
    /// <param name="localDate"></param>
    /// <returns></returns>
    public static DateTimeOffset ToDateTimeOffset(this LocalDate localDate)
    {
        return new DateTimeOffset(year: localDate.Year, month: localDate.Month, day: localDate.Day, hour: 0, minute: 0,
            second: 0, offset: TimeSpan.Zero);
    }
}
