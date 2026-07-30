using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Sheets.v4.Data;
using Moq;
using RaptorSheets.Core.Constants;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Models;
using RaptorSheets.Core.Services;
using RaptorSheets.Stock.Managers;
using Xunit;
using SheetName = RaptorSheets.Stock.Enums.SheetName;

namespace RaptorSheets.Stock.Tests.Unit.Managers;

/// <summary>
/// Cross-domain check that the includeStructure opt-in and GetLiveSheetStructure(s) - built once in
/// SheetManagerBase{TEntity} - actually work through a second domain's concrete manager, not just
/// Gig's. See RaptorSheets.Gig.Tests.Unit.Managers.GetSheetsStructureBehaviorTests/
/// GetLiveSheetStructureTests for the full behavior coverage.
/// </summary>
public class GetSheetsStructureBehaviorTests
{
    private static Spreadsheet BuildGridDataSpreadsheet(string sheetName, int sheetId, IList<string> headerNames)
    {
        var headerCells = headerNames.Select(h => new CellData { FormattedValue = h }).ToList();

        return new Spreadsheet
        {
            Properties = new SpreadsheetProperties { Title = "MyStockSheet" },
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
    public async Task GetSheets_WithIncludeStructureTrue_PopulatesStructures()
    {
        var mockService = new Mock<IGoogleSheetService>();
        var accountsSheet = SheetName.ACCOUNTS.GetDescription();

        mockService
            .Setup(s => s.GetSheetInfoResult(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(GoogleApiResult<Spreadsheet>.Ok(BuildGridDataSpreadsheet(accountsSheet, 5, new List<string> { "Account", "Description" })));

        var manager = new SheetManager(mockService.Object);

        var result = await manager.GetSheets(new List<string> { accountsSheet }, includeStructure: true);

        Assert.True(result.Structures.ContainsKey(accountsSheet));
        Assert.Equal(2, result.Structures[accountsSheet].Headers.Count);
        Assert.Equal("Account", result.Structures[accountsSheet].Headers[0].Name);
    }

    [Fact]
    public async Task GetLiveSheetStructure_ReturnsParsedStructure()
    {
        var mockService = new Mock<IGoogleSheetService>();
        var accountsSheet = SheetName.ACCOUNTS.GetDescription();

        mockService
            .Setup(s => s.GetSheetInfo(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildGridDataSpreadsheet(accountsSheet, 5, new List<string> { "Account", "Description" }));

        var manager = new SheetManager(mockService.Object);

        var result = await manager.GetLiveSheetStructure(accountsSheet);

        Assert.NotNull(result);
        Assert.Equal(accountsSheet, result!.Name);
        Assert.Equal(2, result.Headers.Count);
    }

    [Fact]
    public async Task GetLiveSheetStructures_WorksForASheetNameNotKnownToTheRegistry()
    {
        var mockService = new Mock<IGoogleSheetService>();

        mockService
            .Setup(s => s.GetSheetInfo(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildGridDataSpreadsheet("SomeHandWrittenTab", 9, new List<string> { "A", "B" }));

        var manager = new SheetManager(mockService.Object);

        var result = await manager.GetLiveSheetStructure("SomeHandWrittenTab");

        Assert.Null(manager.GetSheetLayout("SomeHandWrittenTab"));
        Assert.NotNull(result);
    }
}
