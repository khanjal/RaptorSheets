using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Sheets.v4.Data;
using Moq;
using RaptorSheets.Core.Constants;
using RaptorSheets.Core.Services;
using RaptorSheets.Gig.Managers;
using Xunit;

namespace RaptorSheets.Gig.Tests.Unit.Managers;

/// <summary>
/// Covers GetLiveSheetStructure(s)/GetAllLiveSheetStructures: a live read of a sheet's structure,
/// distinct from GetSheetLayout's configured/expected shape. Unlike GetSheetLayout, these never
/// consult the registry, so they work for any live sheet name - including ones this domain doesn't
/// know about.
/// </summary>
public class GetLiveSheetStructureTests
{
    private static Spreadsheet BuildGridDataSpreadsheet(string sheetName, int sheetId, IList<string> headerNames)
    {
        var headerCells = headerNames.Select(h => new CellData { FormattedValue = h }).ToList();

        return new Spreadsheet
        {
            Sheets = new List<Sheet>
            {
                new()
                {
                    Properties = new SheetProperties { SheetId = sheetId, Title = sheetName },
                    Data = new List<GridData> { new() { RowData = new List<RowData> { new() { Values = headerCells } } } }
                }
            }
        };
    }

    [Fact]
    public async Task GetLiveSheetStructure_RequestsHeaderAndFirstDataRow()
    {
        // Not just the header row: format/validation only ever live on row 1 (the first data row,
        // see GoogleRequestHelpers.GenerateRepeatCellRequest), never on the header cell itself.
        var mockService = new Mock<IGoogleSheetService>();
        List<string>? capturedRanges = null;

        mockService
            .Setup(s => s.GetSheetInfo(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()))
            .Callback<List<string>?, CancellationToken>((ranges, _) => capturedRanges = ranges)
            .ReturnsAsync(BuildGridDataSpreadsheet("Shifts", 5, new List<string> { "Date", "Number" }));

        var manager = new SheetManager(mockService.Object);

        await manager.GetLiveSheetStructure("Shifts");

        Assert.Equal(new List<string> { $"Shifts!{GoogleConfig.HeaderStructureRange}" }, capturedRanges);
    }

    [Fact]
    public async Task GetLiveSheetStructure_ReturnsParsedStructure()
    {
        var mockService = new Mock<IGoogleSheetService>();

        mockService
            .Setup(s => s.GetSheetInfo(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildGridDataSpreadsheet("Shifts", 5, new List<string> { "Date", "Number" }));

        var manager = new SheetManager(mockService.Object);

        var result = await manager.GetLiveSheetStructure("Shifts");

        Assert.NotNull(result);
        Assert.Equal("Shifts", result!.Name);
        Assert.Equal(2, result.Headers.Count);
    }

    [Fact]
    public async Task GetLiveSheetStructure_WithSheetNotInResponse_ReturnsNull()
    {
        var mockService = new Mock<IGoogleSheetService>();

        mockService
            .Setup(s => s.GetSheetInfo(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Spreadsheet { Sheets = new List<Sheet>() });

        var manager = new SheetManager(mockService.Object);

        var result = await manager.GetLiveSheetStructure("Shifts");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetLiveSheetStructure_WithNullSpreadsheet_ReturnsNull()
    {
        var mockService = new Mock<IGoogleSheetService>();

        mockService
            .Setup(s => s.GetSheetInfo(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Spreadsheet?)null);

        var manager = new SheetManager(mockService.Object);

        var result = await manager.GetLiveSheetStructure("Shifts");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetLiveSheetStructure_WorksForASheetNameNotKnownToTheRegistry()
    {
        var mockService = new Mock<IGoogleSheetService>();

        mockService
            .Setup(s => s.GetSheetInfo(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildGridDataSpreadsheet("SomeHandWrittenTab", 9, new List<string> { "A", "B" }));

        var manager = new SheetManager(mockService.Object);

        // "SomeHandWrittenTab" is not one of Gig's canonical/registered sheets - GetSheetLayout would
        // return null for it, but a live read has no such restriction.
        var result = await manager.GetLiveSheetStructure("SomeHandWrittenTab");

        Assert.Null(manager.GetSheetLayout("SomeHandWrittenTab"));
        Assert.NotNull(result);
        Assert.Equal("SomeHandWrittenTab", result!.Name);
    }

    [Fact]
    public async Task GetLiveSheetStructures_WithEmptyList_ReturnsEmptyWithoutCallingService()
    {
        var mockService = new Mock<IGoogleSheetService>();
        var manager = new SheetManager(mockService.Object);

        var result = await manager.GetLiveSheetStructures(new List<string>());

        Assert.Empty(result);
        mockService.Verify(s => s.GetSheetInfo(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetAllLiveSheetStructures_RequestsEveryCanonicalSheet()
    {
        var mockService = new Mock<IGoogleSheetService>();
        List<string>? capturedRanges = null;

        mockService
            .Setup(s => s.GetSheetInfo(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()))
            .Callback<List<string>?, CancellationToken>((ranges, _) => capturedRanges = ranges)
            .ReturnsAsync(BuildGridDataSpreadsheet("Shifts", 5, new List<string> { "Date" }));

        var manager = new SheetManager(mockService.Object);

        await manager.GetAllLiveSheetStructures();

        Assert.NotNull(capturedRanges);
        Assert.True(capturedRanges!.Count > 1);
        Assert.Contains($"Shifts!{GoogleConfig.HeaderStructureRange}", capturedRanges);
    }
}
