using Google.Apis.Sheets.v4.Data;
using Moq;
using RaptorSheets.Core.Services;
using RaptorSheets.Home.Constants;
using RaptorSheets.Home.Managers;
using RaptorSheets.Core.Models;

namespace RaptorSheets.Home.Tests.Unit.Managers;

/// <summary>
/// Exercises GetSheets end-to-end (registry dispatch -> MapData -> entity population,
/// unknown-tab detection) against a mocked IGoogleSheetService, without any live network call.
/// </summary>
public class GetSheetsBehaviorTests
{
    private static BatchGetValuesByDataFilterResponse BuildBatchResponse(string sheetName, IList<object> headerRow, IList<object>? dataRow = null)
    {
        var values = new List<IList<object>> { headerRow };
        if (dataRow != null)
        {
            values.Add(dataRow);
        }

        return new BatchGetValuesByDataFilterResponse
        {
            ValueRanges = new List<MatchedValueRange>
            {
                new()
                {
                    DataFilters = new List<DataFilter> { new() { A1Range = sheetName } },
                    ValueRange = new ValueRange { Values = values }
                }
            }
        };
    }

    [Fact]
    public async Task GetSheets_ForRooms_MapsRowDataOntoEntity()
    {
        var mockService = new Mock<IGoogleSheetService>();

        mockService
            .Setup(s => s.GetBatchData(It.IsAny<List<string>>(), It.IsAny<string>()))
            .ReturnsAsync(BuildBatchResponse(
                SheetsConfig.SheetNames.Rooms,
                new List<object> { "Room", "L", "W", "Sq. Ft", "Level" },
                new List<object> { "Living Room", "15", "12", "180", "Main" }));
        mockService
            .Setup(s => s.GetBatchDataResult(It.IsAny<List<string>>(), It.IsAny<string>()))
            .ReturnsAsync(GoogleApiResult<BatchGetValuesByDataFilterResponse>.Ok(BuildBatchResponse(
                SheetsConfig.SheetNames.Rooms,
                new List<object> { "Room", "L", "W", "Sq. Ft", "Level" },
                new List<object> { "Living Room", "15", "12", "180", "Main" })));

        mockService
            .Setup(s => s.GetSheetInfo())
            .ReturnsAsync(new Spreadsheet
            {
                Properties = new SpreadsheetProperties { Title = "MyHomeSheet" },
                Sheets = new List<Sheet>
                {
                    new() { Properties = new SheetProperties { Title = SheetsConfig.SheetNames.Rooms } }
                }
            });

        var manager = new GoogleSheetManager(mockService.Object);

        var result = await manager.GetSheets([SheetsConfig.SheetNames.Rooms]);

        Assert.NotNull(result);
        var room = Assert.Single(result.Sheets.Rooms);
        Assert.Equal("Living Room", room.Room);
        Assert.Equal(15, room.Length);
        Assert.Equal(12, room.Width);
        Assert.Equal("Main", room.Level);
        Assert.Equal("MyHomeSheet", result.Properties.Name);
    }

    [Fact]
    public async Task GetSheets_WithUnknownSheetTab_SurfacesWarning()
    {
        var mockService = new Mock<IGoogleSheetService>();

        mockService
            .Setup(s => s.GetBatchData(It.IsAny<List<string>>(), It.IsAny<string>()))
            .ReturnsAsync(BuildBatchResponse(SheetsConfig.SheetNames.Rooms, new List<object> { "Room", "L", "W", "Sq. Ft", "Level" }));
        mockService
            .Setup(s => s.GetBatchDataResult(It.IsAny<List<string>>(), It.IsAny<string>()))
            .ReturnsAsync(GoogleApiResult<BatchGetValuesByDataFilterResponse>.Ok(BuildBatchResponse(SheetsConfig.SheetNames.Rooms, new List<object> { "Room", "L", "W", "Sq. Ft", "Level" })));

        mockService
            .Setup(s => s.GetSheetInfo())
            .ReturnsAsync(new Spreadsheet
            {
                Properties = new SpreadsheetProperties { Title = "MyHomeSheet" },
                Sheets = new List<Sheet>
                {
                    new() { Properties = new SheetProperties { Title = SheetsConfig.SheetNames.Rooms } },
                    new() { Properties = new SheetProperties { Title = "SomeRandomTab" } }
                }
            });

        var manager = new GoogleSheetManager(mockService.Object);

        var result = await manager.GetSheets([SheetsConfig.SheetNames.Rooms]);

        Assert.Contains(result.Messages, m => m.Message.Contains("SomeRandomTab"));
    }

    [Fact]
    public async Task GetSheets_WhenBatchDataFails_ReturnsErrorMessage()
    {
        var mockService = new Mock<IGoogleSheetService>();

        mockService
            .Setup(s => s.GetBatchData(It.IsAny<List<string>>(), It.IsAny<string>()))
            .ReturnsAsync((BatchGetValuesByDataFilterResponse?)null);
        mockService
            .Setup(s => s.GetBatchDataResult(It.IsAny<List<string>>(), It.IsAny<string>()))
            .ReturnsAsync(GoogleApiResult<BatchGetValuesByDataFilterResponse>.Failed(new GoogleApiFailure { Reason = GoogleApiFailureReason.Unknown, Message = "test failure" }));

        // Populate every canonical Home sheet so GetSheets' self-heal finds nothing missing and
        // falls through to the "unable to retrieve" error this test is actually targeting - self-heal
        // always checks the full canonical list, not just the sheets this call requested.
        mockService
            .Setup(s => s.GetSheetInfo())
            .ReturnsAsync(new Spreadsheet
            {
                Sheets = SheetsConfig.SheetUtilities.GetAllSheetNames()
                    .Select(name => new Sheet { Properties = new SheetProperties { Title = name } })
                    .ToList()
            });

        var manager = new GoogleSheetManager(mockService.Object);

        var result = await manager.GetSheets([SheetsConfig.SheetNames.Rooms]);

        Assert.Contains(result.Messages, m => m.Message.Contains("Unable to retrieve"));
    }
}
