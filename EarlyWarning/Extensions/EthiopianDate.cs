namespace EarlyWarning.Extensions;

/// <summary>
/// Gregorian → Ethiopian calendar (ዓ/ም). Twelve months of 30 days plus Pagume (5 days, 6 in the year
/// before a Gregorian leap year). Uses Julian day numbers; the Ethiopian epoch is JDN 1724221.
/// </summary>
public readonly record struct EthiopianDate(int Year, int Month, int Day)
{
    private const int EpochMinusOneYear = 1723856; // JDN of 1/1/1 minus 365 days

    public static EthiopianDate FromGregorian(DateTime date)
    {
        var jdn = (int)(date.Date - new DateTime(2000, 1, 1)).TotalDays + 2451545;
        var offset = jdn - EpochMinusOneYear;
        var r = offset % 1461;
        var n = r % 365 + 365 * (r / 1460);
        var year = 4 * (offset / 1461) + r / 365 - r / 1460;
        return new EthiopianDate(year, n / 30 + 1, n % 30 + 1);
    }

    /// <summary>The Gregorian date of this Ethiopian date (inverse of <see cref="FromGregorian"/>).</summary>
    public DateTime ToGregorian()
    {
        var jdn = EpochMinusOneYear + 365 + 365 * (Year - 1) + Year / 4 + 30 * (Month - 1) + Day - 1;
        return new DateTime(2000, 1, 1).AddDays(jdn - 2451545);
    }

    /// <summary>ዓመተ ምሕረት leap year: Pagume (month 13) has 6 days.</summary>
    public static bool IsLeapYear(int year) => year % 4 == 3;

    public static readonly string[] MonthNames =
        ["መስከረም", "ጥቅምት", "ኅዳር", "ታኅሣሥ", "ጥር", "የካቲት", "መጋቢት", "ሚያዝያ", "ግንቦት", "ሰኔ", "ሐምሌ", "ነሐሴ", "ጳጉሜ"];

    /// <summary>Long form with the month name, e.g. "17 ሚያዝያ 2018".</summary>
    public string ToLongString() => $"{Day} {MonthNames[Month - 1]} {Year}";

    /// <summary>dd/MM/yyyy, as written on the paper reports (e.g. 17/08/2018).</summary>
    public override string ToString() => $"{Day:00}/{Month:00}/{Year}";
}
