using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Constants;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Mappers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Helpers;
using RaptorSheets.Gig.Mappers;

namespace RaptorSheets.Gig.Tests.Unit.Helpers;

public class GoogleSheetHelpersTests
{
    public static IEnumerable<object[]> Sheets =>
    new List<object[]>
    {
        new object[] { AddressMapper.GetSheet(), GenerateSheetsHelpers.Generate(new List<string> { SheetEnum.ADDRESSES.GetDescription() }) },
        new object[] { DailyMapper.GetSheet(), GenerateSheetsHelpers.Generate(new List<string> { SheetEnum.DAILY.GetDescription() }) },
        new object[] { GenericSheetMapper<ExpenseEntity>.GetSheet(SheetsConfig.ExpenseSheet), GenerateSheetsHelpers.Generate(new List<string> { SheetEnum.EXPENSES.GetDescription() }) },
        new object[] { MonthlyMapper.GetSheet(), GenerateSheetsHelpers.Generate(new List<string> { SheetEnum.MONTHLY.GetDescription() }) },
        new object[] { NameMapper.GetSheet(), GenerateSheetsHelpers.Generate(new List<string> { SheetEnum.NAMES.GetDescription() }) },
        new object[] { PlaceMapper.GetSheet(), GenerateSheetsHelpers.Generate(new List<string> { SheetEnum.PLACES.GetDescription() }) },
        new object[] { RegionMapper.GetSheet(), GenerateSheetsHelpers.Generate(new List<string> { SheetEnum.REGIONS.GetDescription() }) },
        new object[] { ServiceMapper.GetSheet(), GenerateSheetsHelpers.Generate(new List<string> { SheetEnum.SERVICES.GetDescription() }) },
        new object[] { GenericSheetMapper<SetupEntity>.GetSheet(SheetsConfig.SetupSheet), GenerateSheetsHelpers.Generate(new List<string> { SheetEnum.SETUP.GetDescription() }) },
        new object[] { ShiftMapper.GetSheet(), GenerateSheetsHelpers.Generate(new List<string> { SheetEnum.SHIFTS.GetDescription() }) },
        new object[] { TripMapper.GetSheet(), GenerateSheetsHelpers.Generate(new List<string> { SheetEnum.TRIPS.GetDescription() }) },
        new object[] { TypeMapper.GetSheet(), GenerateSheetsHelpers.Generate(new List<string> { SheetEnum.TYPES.GetDescription() }) },
        new object[] { WeekdayMapper.GetSheet(), GenerateSheetsHelpers.Generate(new List<string> { SheetEnum.WEEKDAYS.GetDescription() }) },
        new object[] { WeeklyMapper.GetSheet(), GenerateSheetsHelpers.Generate(new List<string> { SheetEnum.WEEKLY.GetDescription() }) },
        new object[] { YearlyMapper.GetSheet(), GenerateSheetsHelpers.Generate(new List<string> { SheetEnum.YEARLY.GetDescription() }) },
    };

    [Theory]
    [MemberData(nameof(Sheets))]
    public void GivenSheetConfig_ThenReturnSheetRequest(SheetModel config, BatchUpdateSpreadsheetRequest batchRequest)
    {
        var index = 0; // AddSheet should be first request

        Assert.NotNull(batchRequest.Requests[index].AddSheet);

        var sheetRequest = batchRequest.Requests[index].AddSheet;
        Assert.Equal(config.Name, sheetRequest.Properties.Title);
        Assert.Equivalent(SheetHelpers.GetColor(config.TabColor), sheetRequest.Properties.TabColor);
        Assert.Equal(config.FreezeColumnCount, sheetRequest.Properties.GridProperties.FrozenColumnCount);
        Assert.Equal(config.FreezeRowCount, sheetRequest.Properties.GridProperties.FrozenRowCount);
    }

    [Theory]
    [MemberData(nameof(Sheets))]
    public void GivenSheetHeaders_ThenReturnSheetHeaders(SheetModel config, BatchUpdateSpreadsheetRequest batchRequest)
    {
        // Get the SheetId from the batch request (which has the randomly generated ID)
        var sheetId = batchRequest.Requests.First().AddSheet.Properties.SheetId;

        // Check on if it had to expand the number of rows (headers > 26)
        if (config.Headers.Count > 26)
        {
            var appendDimension = batchRequest.Requests.FirstOrDefault(x => x.AppendDimension != null)?.AppendDimension;
            Assert.NotNull(appendDimension);
            Assert.Equal("COLUMNS", appendDimension.Dimension);
            Assert.Equal(config.Headers.Count - 26, appendDimension.Length);
            Assert.Equal(sheetId, appendDimension.SheetId);
        }

        var appendCells = batchRequest.Requests.First(x => x.AppendCells != null).AppendCells;
        Assert.Equal(sheetId, appendCells.SheetId);
        Assert.Single(appendCells.Rows);
        Assert.Equal(config.Headers.Count, appendCells.Rows[0].Values.Count);
    }

    [Theory]
    [MemberData(nameof(Sheets))]
    public void GivenSheetColors_ThenReturnSheetBanding(SheetModel config, BatchUpdateSpreadsheetRequest batchRequest)
    {
        var sheetId = batchRequest.Requests.First().AddSheet.Properties.SheetId;

        var bandedRange = batchRequest.Requests.First(x => x.AddBanding != null).AddBanding.BandedRange;
        Assert.Equal(sheetId, bandedRange.Range.SheetId);
        Assert.Equivalent(SheetHelpers.GetColor(config.TabColor), bandedRange.RowProperties.HeaderColor);
        Assert.Equivalent(SheetHelpers.GetColor(config.CellColor), bandedRange.RowProperties.SecondBandColor);
    }

    [Theory]
    [MemberData(nameof(Sheets))]
    public void GivenSheetProtected_ThenReturnProtectRequest(SheetModel config, BatchUpdateSpreadsheetRequest batchRequest)
    {
        var sheetId = batchRequest.Requests.First().AddSheet.Properties.SheetId;
        var protectRange = batchRequest.Requests.Where(x => x.AddProtectedRange != null).ToList();

        if (!config.ProtectSheet)
        {
            return;
        }

        // Protected sheets should have at least one protection request
        // With entity-driven headers, there may be multiple protection requests if headers are individually protected
        Assert.NotEmpty(protectRange);

        // Log protectRange for debugging
        protectRange.Select(protection => protection.AddProtectedRange.ProtectedRange)
            .ToList()
            .ForEach(protectedRange => Console.WriteLine($"Protected Range: SheetId={protectedRange.Range.SheetId}, Description={protectedRange.Description}"));

        // At least one should be a sheet-level protection
        var sheetProtections = protectRange.Where(p =>
            p.AddProtectedRange.ProtectedRange.Description == ProtectionWarnings.SheetWarning &&
            p.AddProtectedRange.ProtectedRange.Range.SheetId == sheetId).ToList();

        Assert.NotEmpty(sheetProtections);

        var sheetProtection = sheetProtections.First().AddProtectedRange.ProtectedRange;
        Assert.Equal(sheetId, sheetProtection.Range.SheetId);
        Assert.Equal(ProtectionWarnings.SheetWarning, sheetProtection.Description);
        Assert.True(sheetProtection.WarningOnly);
    }

    [Theory]
    [MemberData(nameof(Sheets))]
    public void GivenSheetNotProtected_ThenReturnProtectRequests(SheetModel config, BatchUpdateSpreadsheetRequest batchRequest)
    {
        var sheetId = batchRequest.Requests.First().AddSheet.Properties.SheetId;
        var protectRange = batchRequest.Requests.Where(x => x.AddProtectedRange != null).ToList();

        if (config.ProtectSheet)
        {
            return;
        }

        var columnProtections = config.Headers.Where(x => !string.IsNullOrEmpty(x.Formula)).ToList();

        Assert.Equal(columnProtections.Count + 1, protectRange.Count); // +1 for header protection

        for (var i = 0; i < protectRange.Count; i++)
        {
            var protectedRange = protectRange[i].AddProtectedRange.ProtectedRange;
            Assert.Equal(sheetId, protectedRange.Range.SheetId);
            Assert.True(protectedRange.WarningOnly);

            if (i == protectRange.Count - 1) // Header protection (last) 
            {
                Assert.Equal(ProtectionWarnings.HeaderWarning, protectedRange.Description);
            }
            else
            {
                Assert.Equal(ProtectionWarnings.ColumnWarning, protectedRange.Description);
            }
        }
    }

    [Theory]
    [MemberData(nameof(Sheets))]
    public void GivenSheetHeaderFormatOrValidation_ThenReturnRepeatCellsRequest(SheetModel config, BatchUpdateSpreadsheetRequest batchRequest)
    {
        var sheetId = batchRequest.Requests.First().AddSheet.Properties.SheetId;
        var repeatCells = batchRequest.Requests.Where(x => x.RepeatCell != null).ToList();
        var repeatHeaders = config.Headers.Where(x => x.Format != null || !string.IsNullOrEmpty(x.Validation)).ToList();

        Assert.Equal(repeatHeaders.Count, repeatCells.Count);
    }

    [Theory]
    [InlineData(2, 1)]
    [InlineData(5, 2)]
    [InlineData(25, 25)]
    [InlineData(2, 100)]
    public void GivenRowIdRanges_ShouldReturnRangeTuples(int startRowId, int count)
    {
        var rowIds = Enumerable.Range(startRowId, count).ToList();

        var result = GoogleRequestHelpers.GenerateIndexRanges(rowIds);

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(startRowId - 1, result.First().Item1);
        Assert.Equal(startRowId + count - 1, result.First().Item2);
    }
}