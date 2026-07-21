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
                new SheetCellModel { Name = "Date", Format = FormatEnum.DATE },
                new SheetCellModel { Name = "Total", Formula = "=SUM(A:A)", Format = FormatEnum.ACCOUNTING },
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
}
