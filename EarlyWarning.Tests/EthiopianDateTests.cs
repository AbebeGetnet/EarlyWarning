using EarlyWarning.Extensions;

namespace EarlyWarning.Tests;

/// <summary>The wizard picks dates in the Ethiopian calendar and stores Gregorian; both directions must agree.</summary>
public class EthiopianDateTests
{
    [Theory]
    [InlineData(2018, 1, 1, "2025-09-11")]
    [InlineData(2018, 8, 17, "2026-04-25")]
    [InlineData(2015, 13, 6, "2023-09-11")]   // ጳጉሜ 6 in a leap year
    [InlineData(2016, 1, 1, "2023-09-12")]
    [InlineData(2016, 13, 5, "2024-09-10")]
    public void ToGregorian_KnownDates(int year, int month, int day, string expected) =>
        Assert.Equal(DateTime.Parse(expected, System.Globalization.CultureInfo.InvariantCulture), new EthiopianDate(year, month, day).ToGregorian());

    [Fact]
    public void RoundTrip_EveryDayFrom1990To2050()
    {
        for (var date = new DateTime(1990, 1, 1); date <= new DateTime(2050, 12, 31); date = date.AddDays(1))
        {
            var ethiopian = EthiopianDate.FromGregorian(date);
            Assert.Equal(date, ethiopian.ToGregorian());
            Assert.InRange(ethiopian.Day, 1, ethiopian.Month == 13 ? (EthiopianDate.IsLeapYear(ethiopian.Year) ? 6 : 5) : 30);
        }
    }

    [Fact]
    public void LongForm_UsesMonthName() =>
        Assert.Equal("17 ሚያዝያ 2018", EthiopianDate.FromGregorian(new DateTime(2026, 4, 25)).ToLongString());
}
