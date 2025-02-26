using FluentAssertions;
using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Mappers;
using RaptorSheets.Core.Constants;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Helpers;
using RaptorSheets.Core.Helpers;

namespace RaptorSheets.Gig.Tests.Helpers;

public class GoogleSheetHelpersTests
{
    public static IEnumerable<object[]> Sheets =>
    [
        [AddressMapper.GetSheet(), GenerateSheetsHelpers.Generate([SheetEnum.ADDRESSES])],
        [DailyMapper.GetSheet(), GenerateSheetsHelpers.Generate([SheetEnum.DAILY])],
        [MonthlyMapper.GetSheet(), GenerateSheetsHelpers.Generate([SheetEnum.MONTHLY])],
        [NameMapper.GetSheet(), GenerateSheetsHelpers.Generate([SheetEnum.NAMES])],
        [PlaceMapper.GetSheet(), GenerateSheetsHelpers.Generate([SheetEnum.PLACES])],
        [RegionMapper.GetSheet(), GenerateSheetsHelpers.Generate([SheetEnum.REGIONS])],
        [ServiceMapper.GetSheet(), GenerateSheetsHelpers.Generate([SheetEnum.SERVICES])],
        [ShiftMapper.GetSheet(), GenerateSheetsHelpers.Generate([SheetEnum.SHIFTS])],
        [TripMapper.GetSheet(), GenerateSheetsHelpers.Generate([SheetEnum.TRIPS])],
        [TypeMapper.GetSheet(), GenerateSheetsHelpers.Generate([SheetEnum.TYPES])],
        [WeekdayMapper.GetSheet(), GenerateSheetsHelpers.Generate([SheetEnum.WEEKDAYS])],
        [WeeklyMapper.GetSheet(), GenerateSheetsHelpers.Generate([SheetEnum.WEEKLY])],
        [YearlyMapper.GetSheet(), GenerateSheetsHelpers.Generate([SheetEnum.YEARLY])],
    ];

    [Theory]
    [MemberData(nameof(Sheets))]
    public void GivenSheetConfig_ThenReturnSheetRequest(SheetModel config, BatchUpdateSpreadsheetRequest batchRequest)
    {
        var index = 0; // AddSheet should be first request

        batchRequest.Requests[index].AddSheet.Should().NotBeNull();

        var sheetRequest = batchRequest.Requests[index].AddSheet;
        sheetRequest.Properties.Title.Should().Be(config.Name);
        sheetRequest.Properties.TabColor.Should().BeEquivalentTo(SheetHelpers.GetColor(config.TabColor));
        sheetRequest.Properties.GridProperties.FrozenColumnCount.Should().Be(config.FreezeColumnCount);
        sheetRequest.Properties.GridProperties.FrozenRowCount.Should().Be(config.FreezeRowCount);
    }

    [Theory]
    [MemberData(nameof(Sheets))]
    public void GivenSheetHeaders_ThenReturnSheetHeaders(SheetModel config, BatchUpdateSpreadsheetRequest batchRequest)
    {
        var sheetId = batchRequest.Requests.First().AddSheet.Properties.SheetId;

        // Check on if it had to expand the number of rows (headers > 26)
        if (config.Headers.Count > 26)
        {
            var appendDimension = batchRequest.Requests.First(x => x.AppendDimension != null).AppendDimension;
            appendDimension.Dimension.Should().Be("COLUMNS");
            appendDimension.Length.Should().Be(config.Headers.Count - 26);
            appendDimension.SheetId.Should().Be(sheetId);
        }

        var appendCells = batchRequest.Requests.First(x => x.AppendCells != null).AppendCells;
        appendCells.SheetId.Should().Be(sheetId);
        appendCells.Rows.Should().HaveCount(1);
        appendCells.Rows[0].Values.Should().HaveCount(config.Headers.Count);
    }

    [Theory]
    [MemberData(nameof(Sheets))]
    public void GivenSheetColors_ThenReturnSheetBanding(SheetModel config, BatchUpdateSpreadsheetRequest batchRequest)
    {
        var sheetId = batchRequest.Requests.First().AddSheet.Properties.SheetId;

        var bandedRange = batchRequest.Requests.First(x => x.AddBanding != null).AddBanding.BandedRange;
        bandedRange.Range.SheetId.Should().Be(sheetId);
        bandedRange.RowProperties.HeaderColor.Should().BeEquivalentTo(SheetHelpers.GetColor(config.TabColor));
        bandedRange.RowProperties.SecondBandColor.Should().BeEquivalentTo(SheetHelpers.GetColor(config.CellColor));
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

        protectRange.Should().HaveCount(1);
        var sheetProtection = protectRange.First().AddProtectedRange.ProtectedRange;
        sheetProtection.Range.SheetId.Should().Be(sheetId);
        sheetProtection.Description.Should().Be(ProtectionWarnings.SheetWarning);
        sheetProtection.WarningOnly.Should().BeTrue();
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

        protectRange.Should().HaveCount(columnProtections.Count + 1); // +1 for header protection

        for (var i = 0; i < protectRange.Count; i++)
        {
            var protectedRange = protectRange[i].AddProtectedRange.ProtectedRange;
            protectedRange.Range.SheetId.Should().Be(sheetId);
            protectedRange.WarningOnly.Should().BeTrue();

            if (i == protectRange.Count - 1) // Header protection (last) 
            {
                protectedRange.Description.Should().Be(ProtectionWarnings.HeaderWarning);
            }
            else
            {
                protectedRange.Description.Should().Be(ProtectionWarnings.ColumnWarning);
            }
        }
    }

    [Theory]
    [MemberData(nameof(Sheets))]
    public void GivenSheetHeaderFormatOrValidation_ThenReturnRepeatCellsRequest(SheetModel config, BatchUpdateSpreadsheetRequest batchRequest)
    {
        var sheetId = batchRequest.Requests.First().AddSheet.Properties.SheetId;
        var repeatCells = batchRequest.Requests.Where(x => x.RepeatCell != null).ToList();
        var repeatHeaders = config.Headers.Where(x => x.Format != null || x.Validation != null).ToList();

        repeatCells.Should().HaveCount(repeatHeaders.Count);
    }

    [Theory]
    [InlineData(2, 1)]
    [InlineData(5, 2)]
    [InlineData(25, 25)]
    [InlineData(2,100)]
    public void GivenRowIdRanges_ShouldReturnRangeTuples(int startRowId, int count)
    {
        var rowIds = Enumerable.Range(startRowId, count).ToList();

        var result = GoogleRequestHelpers.GenerateIndexRanges(rowIds);

        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().Item1.Should().Be(startRowId - 1);
        result.First().Item2.Should().Be(startRowId + count - 1);
    }
}
