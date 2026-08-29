using RaptorSheets.Core.Constants;
using Xunit;

namespace RaptorSheets.Core.Tests.Unit.Constants;

/// <summary>
/// The example is what makes a format note useful - the raw pattern tells a user almost nothing,
/// least of all the accounting one, which is a 60-character run of escapes.
/// </summary>
public class CellFormatExamplesTests
{
    [Theory]
    [InlineData(CellFormatPatterns.Accounting, "$ 1,234.56")]
    [InlineData(CellFormatPatterns.Currency, "$1,234.56")]
    [InlineData(CellFormatPatterns.Date, "2026-03-09")]
    [InlineData(CellFormatPatterns.Time24Hour, "14:30")]
    [InlineData(CellFormatPatterns.Duration, "26:30")]
    [InlineData(CellFormatPatterns.Distance, "1,234.5")]
    public void For_KnownPattern_ReturnsItsExample(string pattern, string expected)
    {
        Assert.Equal(expected, CellFormatExamples.For(pattern));
    }

    [Fact]
    public void For_UnknownPattern_ReturnsNull()
    {
        // Callers append only when non-null, so an unrecognised pattern degrades to showing just
        // the pattern rather than to a confidently wrong example.
        Assert.Null(CellFormatExamples.For("##0.000###"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void For_NoPattern_ReturnsNull(string? pattern)
    {
        Assert.Null(CellFormatExamples.For(pattern));
    }

    [Fact]
    public void DurationExample_ExceedsTwentyFourHours()
    {
        // [h] accumulates rather than wrapping, which is the whole difference between a duration
        // and a clock time - an example under 24h would hide it.
        var duration = CellFormatExamples.For(CellFormatPatterns.Duration);

        Assert.NotNull(duration);
        Assert.Equal(26, int.Parse(duration!.Split(':')[0]));
    }

    [Fact]
    public void DateExamples_AllDescribeTheSameDay()
    {
        // Rendering one day every way is what lets a user tell 3/9 from 09/03 at a glance.
        Assert.Equal("2026-03-09", CellFormatExamples.For(CellFormatPatterns.Date));
        Assert.Equal("3/9/2026", CellFormatExamples.For(CellFormatPatterns.DateUS));
        Assert.Equal("09/03/2026", CellFormatExamples.For(CellFormatPatterns.DateEU));
        Assert.Equal("March 9, 2026", CellFormatExamples.For(CellFormatPatterns.DateLong));
    }
}
