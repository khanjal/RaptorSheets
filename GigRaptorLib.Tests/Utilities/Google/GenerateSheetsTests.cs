using FluentAssertions;
using GigRaptorLib.Constants;
using GigRaptorLib.Mappers;
using GigRaptorLib.Models;
using GigRaptorLib.Utilities;
using GigRaptorLib.Utilities.Google;
using Google.Apis.Sheets.v4.Data;

namespace GigRaptorLib.Tests.Utilities.Google;

public class GenerateSheetsTests
{
    public static IEnumerable<object[]> Sheets =>
    [
        [AddressMapper.GetSheet(), GenerateSheetHelper.Generate([AddressMapper.GetSheet()])],
        [DailyMapper.GetSheet(), GenerateSheetHelper.Generate([DailyMapper.GetSheet()])],
        [MonthlyMapper.GetSheet(), GenerateSheetHelper.Generate([MonthlyMapper.GetSheet()])],
        [NameMapper.GetSheet(), GenerateSheetHelper.Generate([NameMapper.GetSheet()])],
        [PlaceMapper.GetSheet(), GenerateSheetHelper.Generate([PlaceMapper.GetSheet()])],
        [RegionMapper.GetSheet(), GenerateSheetHelper.Generate([RegionMapper.GetSheet()])],
        [ServiceMapper.GetSheet(), GenerateSheetHelper.Generate([ServiceMapper.GetSheet()])],
        [ShiftMapper.GetSheet(), GenerateSheetHelper.Generate([ShiftMapper.GetSheet()])],
        [TripMapper.GetSheet(), GenerateSheetHelper.Generate([TripMapper.GetSheet()])],
        [TypeMapper.GetSheet(), GenerateSheetHelper.Generate([TypeMapper.GetSheet()])],
        [WeekdayMapper.GetSheet(), GenerateSheetHelper.Generate([WeekdayMapper.GetSheet()])],
        [WeeklyMapper.GetSheet(), GenerateSheetHelper.Generate([WeeklyMapper.GetSheet()])],
        [YearlyMapper.GetSheet(), GenerateSheetHelper.Generate([YearlyMapper.GetSheet()])],
    ];

    [Theory]
    [MemberData(nameof(Sheets))]
    public void GivenSheetConfig_ThenReturnSheetRequest(SheetModel config, BatchUpdateSpreadsheetRequest batchRequest)
    {
        var index = 0; // AddSheet should be first request

        batchRequest.Requests[index].AddSheet.Should().NotBeNull();

        var sheetRequest = batchRequest.Requests[index].AddSheet;
        sheetRequest.Properties.Title.Should().Be(config.Name);
        sheetRequest.Properties.TabColor.Should().BeEquivalentTo(SheetHelper.GetColor(config.TabColor));
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
        bandedRange.RowProperties.HeaderColor.Should().BeEquivalentTo(SheetHelper.GetColor(config.TabColor));
        bandedRange.RowProperties.SecondBandColor.Should().BeEquivalentTo(SheetHelper.GetColor(config.CellColor));
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
}
