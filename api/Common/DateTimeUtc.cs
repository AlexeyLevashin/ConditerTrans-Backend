namespace Common;

public static class DateTimeUtc
{
    /// <summary>Календарная дата в UTC (полночь) для PostgreSQL timestamptz.</summary>
    public static DateTime FromDate(DateTime value) =>
        DateTime.SpecifyKind(value.Date, DateTimeKind.Utc);

    public static DateTime? FromDate(DateTime? value) =>
        value.HasValue ? FromDate(value.Value) : null;
}
