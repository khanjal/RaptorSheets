using RaptorSheets.Gig.Managers;
using Xunit;

namespace RaptorSheets.Gig.Tests.Unit.Helpers;

/// <summary>
/// Demo data is how a consuming app's tag UI gets exercised without hand-entering rows, so the
/// tags it generates have to look like tags a driver would actually keep - a small vocabulary,
/// reused, on some rows but not all.
/// </summary>
public class DemoTagsTests
{
    private static (List<string> tripTags, List<string> shiftTags) Generate()
    {
        var sheet = new SheetManager("token", "spreadsheet").GenerateDemoData();

        return (
            sheet.Sheets.Trips.Select(t => t.Tags).ToList(),
            sheet.Sheets.Shifts.Select(s => s.Tags).ToList());
    }

    [Fact]
    public void DemoData_TagsSomeTripsAndShifts()
    {
        var (tripTags, shiftTags) = Generate();

        Assert.Contains(tripTags, t => !string.IsNullOrEmpty(t));
        Assert.Contains(shiftTags, t => !string.IsNullOrEmpty(t));
    }

    [Fact]
    public void DemoData_LeavesSomeRowsUntagged()
    {
        // Tagging everything would make the column look mandatory, and would hide the case where a
        // row has none - which is the one a consuming UI is most likely to render badly.
        var (tripTags, _) = Generate();

        Assert.Contains(tripTags, string.IsNullOrEmpty);
    }

    [Fact]
    public void DemoData_ReusesASmallVocabularyRatherThanInventingPerRow()
    {
        // The point of reuse: a tag autocomplete built on "tags already used" is useless if every
        // row invented its own value.
        var (tripTags, shiftTags) = Generate();

        var distinct = tripTags.Concat(shiftTags)
            .SelectMany(t => t.Split(',', StringSplitOptions.RemoveEmptyEntries))
            .Select(t => t.Trim())
            .Distinct()
            .ToList();

        Assert.NotEmpty(distinct);
        Assert.True(distinct.Count <= 15, $"expected a small reusable vocabulary, got {distinct.Count}");
    }

    [Fact]
    public void DemoData_WritesTagsInTheColumnsCommaDelimitedFormat()
    {
        var (tripTags, shiftTags) = Generate();

        foreach (var value in tripTags.Concat(shiftTags).Where(t => !string.IsNullOrEmpty(t)))
        {
            // No leading, trailing or doubled separators - the value is read back by splitting on
            // commas, so a stray one becomes an empty tag.
            Assert.DoesNotContain(",,", value);
            Assert.False(value.StartsWith(','), $"'{value}' starts with a separator");
            Assert.False(value.EndsWith(','), $"'{value}' ends with a separator");

            foreach (var tag in value.Split(','))
            {
                Assert.Equal(tag.Trim(), tag.Trim(' '));
                Assert.NotEmpty(tag.Trim());
            }
        }
    }
}
