using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;
using Xunit;

namespace RaptorSheets.Core.Tests.Unit.Helpers;

public class SheetGenerationHelperTests
{
    private static SheetModel BuildSheetModel(string name, bool protectSheet = false)
    {
        return new SheetModel
        {
            Name = name,
            ProtectSheet = protectSheet,
            Headers =
            [
                new SheetCellModel { Name = "Date", Format = Format.DATE },
                new SheetCellModel { Name = "Total", Formula = "=SUM(A:A)", Format = Format.ACCOUNTING },
                new SheetCellModel { Name = "Category", Validation = "SomeRange!A2:A" },
                new SheetCellModel { Name = "Notes" }
            ]
        };
    }

    [Fact]
    public void Generate_WithEmptySheetList_ReturnsEmptyRequest()
    {
        var result = SheetGenerationHelper.Generate([], _ => new SheetModel(), _ => null);

        Assert.NotNull(result);
        Assert.Empty(result.Requests);
    }

    [Fact]
    public void Generate_ForOneSheet_IncludesPropertiesAppendCellsBandingAndProtection()
    {
        var result = SheetGenerationHelper.Generate(
            ["Trips"],
            name => BuildSheetModel(name),
            _ => null);

        Assert.Contains(result.Requests, r => r.AddSheet != null && r.AddSheet.Properties.Title == "Trips");
        Assert.Contains(result.Requests, r => r.AppendCells != null);
        Assert.Contains(result.Requests, r => r.AddBanding != null);
        Assert.Contains(result.Requests, r => r.AddProtectedRange != null);
    }

    [Fact]
    public void Generate_AssignsARandomNonZeroSheetId()
    {
        var result = SheetGenerationHelper.Generate(
            ["Trips"],
            name => BuildSheetModel(name),
            _ => null);

        var addSheet = result.Requests.First(r => r.AddSheet != null).AddSheet;
        Assert.NotEqual(0, addSheet.Properties.SheetId ?? 0);
    }

    [Fact]
    public void Generate_ForUnprotectedSheetWithFormulaColumn_ProtectsThatColumn()
    {
        var result = SheetGenerationHelper.Generate(
            ["Trips"],
            name => BuildSheetModel(name, protectSheet: false),
            _ => null);

        Assert.Contains(result.Requests, r => r.AddProtectedRange?.ProtectedRange?.Range?.StartColumnIndex == 1);
    }

    [Fact]
    public void Generate_ForFullyProtectedSheet_DoesNotAddPerColumnProtectionForFormulaColumn()
    {
        var withoutSheetProtection = SheetGenerationHelper.Generate(
            ["Trips"],
            name => BuildSheetModel(name, protectSheet: false),
            _ => null);
        var withSheetProtection = SheetGenerationHelper.Generate(
            ["Trips"],
            name => BuildSheetModel(name, protectSheet: true),
            _ => null);

        var perColumnProtectionCount = withoutSheetProtection.Requests
            .Count(r => r.AddProtectedRange?.ProtectedRange?.Range?.StartColumnIndex == 1);
        var perColumnProtectionCountWhenProtected = withSheetProtection.Requests
            .Count(r => r.AddProtectedRange?.ProtectedRange?.Range?.StartColumnIndex == 1);

        Assert.True(perColumnProtectionCount > 0);
        Assert.Equal(0, perColumnProtectionCountWhenProtected);
    }

    [Fact]
    public void Generate_ForColumnWithValidation_InvokesGetDataValidationForThatHeaderOnly()
    {
        var validatedHeaders = new List<string>();

        SheetGenerationHelper.Generate(
            ["Trips"],
            name => BuildSheetModel(name),
            header =>
            {
                validatedHeaders.Add(header.Name);
                return new DataValidationRule();
            });

        Assert.Equal(["Category"], validatedHeaders);
    }

    [Fact]
    public void Generate_ForMultipleSheets_CallsGetSheetModelForEachRequestedName()
    {
        var requestedNames = new List<string>();

        SheetGenerationHelper.Generate(
            ["Trips", "Shifts", "Expenses"],
            name =>
            {
                requestedNames.Add(name);
                return BuildSheetModel(name);
            },
            _ => null);

        Assert.Equal(["Trips", "Shifts", "Expenses"], requestedNames);
    }

    [Fact]
    public void Generate_HeadersGetSequentialColumnAssignments()
    {
        var sheetModel = BuildSheetModel("Trips");

        SheetGenerationHelper.Generate(["Trips"], _ => sheetModel, _ => null);

        Assert.Equal(["A", "B", "C", "D"], sheetModel.Headers.Select(h => h.Column));
    }

    [Fact]
    public void Generate_ForColumnFlaggedWithNamedRange_AddsNamedRangeRequest()
    {
        // Regression guard: a NamedRange-only header (no Format/Validation/FormatPattern) must
        // not be silently skipped by the "nothing to format" early-exit that governs whether a
        // RepeatCell request gets built - named ranges are a separate request entirely.
        var sheetModel = new SheetModel
        {
            Name = "Trips",
            Headers = [new SheetCellModel { Name = "Key", NamedRange = true }]
        };

        var result = SheetGenerationHelper.Generate(["Trips"], _ => sheetModel, _ => null);

        var namedRange = result.Requests.Single(r => r.AddNamedRange != null).AddNamedRange.NamedRange;
        Assert.Equal("Trips_Key", namedRange.Name);
    }

    [Fact]
    public void Generate_ForColumnWithoutNamedRangeFlag_DoesNotAddNamedRangeRequest()
    {
        var sheetModel = BuildSheetModel("Trips");

        var result = SheetGenerationHelper.Generate(["Trips"], _ => sheetModel, _ => null);

        Assert.DoesNotContain(result.Requests, r => r.AddNamedRange != null);
    }

    [Fact]
    public void Generate_ForColumnFlaggedWithConditionalFormat_InvokesResolverAndAddsRequest()
    {
        var sheetModel = new SheetModel
        {
            Name = "Trips",
            Headers = [new SheetCellModel { Name = "Total", ConditionalFormat = "NEGATIVE_BALANCE" }]
        };
        var rule = new BooleanRule { Condition = new BooleanCondition { Type = "NUMBER_LESS" } };

        var result = SheetGenerationHelper.Generate(["Trips"], _ => sheetModel, _ => null, _ => rule);

        var addedRule = result.Requests.Single(r => r.AddConditionalFormatRule != null).AddConditionalFormatRule;
        Assert.Same(rule, addedRule.Rule.BooleanRule);
        Assert.Equal(0, addedRule.Index);
    }

    [Fact]
    public void Generate_ForMultipleColumnsFlaggedWithConditionalFormat_AssignsSequentialIndexes()
    {
        var sheetModel = new SheetModel
        {
            Name = "Trips",
            Headers =
            [
                new SheetCellModel { Name = "Pay", ConditionalFormat = "NEGATIVE_BALANCE" },
                new SheetCellModel { Name = "Tips", ConditionalFormat = "NEGATIVE_BALANCE" }
            ]
        };

        var result = SheetGenerationHelper.Generate(
            ["Trips"], _ => sheetModel, _ => null, _ => new BooleanRule());

        var indexes = result.Requests
            .Where(r => r.AddConditionalFormatRule != null)
            .Select(r => r.AddConditionalFormatRule.Index)
            .ToList();
        Assert.Equal([0, 1], indexes);
    }

    [Fact]
    public void Generate_ForColumnFlaggedWithConditionalFormat_WithoutResolver_AddsNothing()
    {
        // Backward-compat guard: getConditionalFormat is optional, so existing callers that don't
        // pass one (all 4 domains, as of this feature) must be unaffected even if a header happens
        // to have ConditionalFormat set.
        var sheetModel = new SheetModel
        {
            Name = "Trips",
            Headers = [new SheetCellModel { Name = "Total", ConditionalFormat = "NEGATIVE_BALANCE" }]
        };

        var result = SheetGenerationHelper.Generate(["Trips"], _ => sheetModel, _ => null);

        Assert.DoesNotContain(result.Requests, r => r.AddConditionalFormatRule != null);
    }

    [Fact]
    public void Generate_ForColumnFlaggedWithConditionalFormat_WhenResolverReturnsNull_AddsNothing()
    {
        var sheetModel = new SheetModel
        {
            Name = "Trips",
            Headers = [new SheetCellModel { Name = "Total", ConditionalFormat = "NEGATIVE_BALANCE" }]
        };

        var result = SheetGenerationHelper.Generate(["Trips"], _ => sheetModel, _ => null, _ => null);

        Assert.DoesNotContain(result.Requests, r => r.AddConditionalFormatRule != null);
    }

    [Fact]
    public void Generate_ForColumnWithoutConditionalFormat_DoesNotInvokeResolver()
    {
        var sheetModel = BuildSheetModel("Trips");
        var invoked = false;

        SheetGenerationHelper.Generate(["Trips"], _ => sheetModel, _ => null, _ => { invoked = true; return null; });

        Assert.False(invoked);
    }
}
