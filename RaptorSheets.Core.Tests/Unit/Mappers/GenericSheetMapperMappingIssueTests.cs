using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Mappers;
using Xunit;

namespace RaptorSheets.Core.Tests.Unit.Mappers;

/// <summary>
/// Covers the mapping-diagnostics overload of MapFromRangeData. The overarching rule under test,
/// repeated in every case: a mapping issue is a report, never control flow - the row's entity is
/// always returned, the property is left at its type default, and the read never fails.
/// </summary>
public class GenericSheetMapperMappingIssueTests
{
    public class TestEntity
    {
        public int RowId { get; set; }

        [Column("Name", isInput: true)]
        public string Name { get; set; } = "";

        [Column("Amount", isInput: true)]
        public decimal? Amount { get; set; }

        [Column("Count", isInput: true)]
        public int Count { get; set; }

        [Column("Active", isInput: true)]
        public bool Active { get; set; }

        [Column("Note", isInput: true, ignoreMappingErrors: true)]
        public decimal? Note { get; set; }
    }

    private static List<IList<object>> Rows(params object[][] rows) =>
        rows.Select(r => (IList<object>)r.ToList()).ToList();

    [Fact]
    public void UnparseableNumericCell_ShouldStillReturnTheRow_WithThePropertyAtItsDefault()
    {
        var values = Rows(
            ["Name", "Amount", "Count", "Active", "Note"],
            ["John", "not-a-number", "5", "TRUE", ""]);

        var result = GenericSheetMapper<TestEntity>.MapFromRangeData(values, "Sheet1", out var issues);

        Assert.Single(result);
        Assert.Equal("John", result[0].Name);
        Assert.Null(result[0].Amount);
        Assert.Single(issues);
    }

    [Fact]
    public void UnparseableNumericCell_ShouldReportLocationButNotRawValueByDefault()
    {
        var values = Rows(
            ["Name", "Amount", "Count", "Active", "Note"],
            ["John", "not-a-number", "5", "TRUE", ""]);

        GenericSheetMapper<TestEntity>.MapFromRangeData(values, "Sheet1", out var issues);

        var issue = Assert.Single(issues);
        Assert.Equal("Sheet1", issue.SheetName);
        Assert.Equal(2, issue.RowId);
        Assert.Equal("Amount", issue.Header);
        Assert.Equal(nameof(TestEntity.Amount), issue.PropertyName);
        Assert.Equal(MappingIssueReason.CouldNotParseValue, issue.Reason);
        Assert.Null(issue.RawValue);
    }

    [Fact]
    public void IncludeRawCellValues_ShouldSurfaceTheOffendingText()
    {
        var values = Rows(
            ["Name", "Amount", "Count", "Active", "Note"],
            ["John", "not-a-number", "5", "TRUE", ""]);

        GenericSheetMapper<TestEntity>.MapFromRangeData(values, "Sheet1", out var issues, includeRawCellValues: true);

        Assert.Equal("not-a-number", Assert.Single(issues).RawValue);
    }

    [Fact]
    public void BlankCell_ShouldNotBeReportedAsAMappingIssue()
    {
        // A blank numeric cell isn't a parse failure - it's the property staying at its natural
        // default, which is the normal, unremarkable case.
        var values = Rows(
            ["Name", "Amount", "Count", "Active", "Note"],
            ["John", "", "5", "TRUE", ""]);

        GenericSheetMapper<TestEntity>.MapFromRangeData(values, "Sheet1", out var issues);

        Assert.Empty(issues);
    }

    [Fact]
    public void FirstIssuePerRow_ShouldWin_RatherThanOneMessagePerBadColumn()
    {
        var values = Rows(
            ["Name", "Amount", "Count", "Active", "Note"],
            ["John", "not-a-number", "also-bad", "TRUE", ""]);

        GenericSheetMapper<TestEntity>.MapFromRangeData(values, "Sheet1", out var issues);

        Assert.Single(issues);
    }

    [Fact]
    public void IgnoreMappingErrors_ShouldSuppressTheIssueForThatColumn()
    {
        var values = Rows(
            ["Name", "Amount", "Count", "Active", "Note"],
            ["John", "100", "5", "TRUE", "not-a-number"]);

        GenericSheetMapper<TestEntity>.MapFromRangeData(values, "Sheet1", out var issues);

        Assert.Empty(issues);
    }

    [Fact]
    public void MultipleBadRows_ShouldEachContributeAtMostOneIssue()
    {
        var values = Rows(
            ["Name", "Amount", "Count", "Active", "Note"],
            ["John", "not-a-number", "5", "TRUE", ""],
            ["Jane", "200", "5", "TRUE", ""],
            ["Jack", "also-bad", "5", "TRUE", ""]);

        var result = GenericSheetMapper<TestEntity>.MapFromRangeData(values, "Sheet1", out var issues);

        Assert.Equal(3, result.Count);
        Assert.Equal(2, issues.Count);
        Assert.Equal([2, 4], issues.Select(i => i.RowId).ToArray());
    }

    [Fact]
    public void OriginalOverload_ShouldStillWork_WithoutRequestingDiagnostics()
    {
        var values = Rows(
            ["Name", "Amount", "Count", "Active", "Note"],
            ["John", "not-a-number", "5", "TRUE", ""]);

        var result = GenericSheetMapper<TestEntity>.MapFromRangeData(values);

        Assert.Single(result);
        Assert.Null(result[0].Amount);
    }

    [Fact]
    public void MessageHelpers_ShouldRenderAnIssueAsAWarning_NeverAnError()
    {
        var values = Rows(
            ["Name", "Amount", "Count", "Active", "Note"],
            ["John", "not-a-number", "5", "TRUE", ""]);

        GenericSheetMapper<TestEntity>.MapFromRangeData(values, "Sheet1", out var issues);
        var message = MessageHelpers.CreateMappingIssueMessage(Assert.Single(issues));

        Assert.Equal(MessageLevel.WARNING.UpperName(), message.Level);
        Assert.DoesNotContain("not-a-number", message.Message, StringComparison.Ordinal);
    }
}
