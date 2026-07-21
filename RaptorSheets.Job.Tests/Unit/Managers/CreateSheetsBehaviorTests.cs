using Google.Apis.Sheets.v4.Data;
using Moq;
using RaptorSheets.Core.Services;
using RaptorSheets.Job.Constants;
using RaptorSheets.Job.Managers;

namespace RaptorSheets.Job.Tests.Unit.Managers;

public class CreateSheetsBehaviorTests
{
    [Fact]
    public async Task CreateSheets_WithDesiredIndexMap_SetsAddSheetIndices()
    {
        var sheetsToCreate = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            [SheetsConfig.SheetNames.Applications] = 1,
            [SheetsConfig.SheetNames.Interviews] = 0
        };

        var mockService = new Mock<IGoogleSheetService>();

        BatchUpdateSpreadsheetRequest? capturedRequest = null;
        mockService.Setup(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>()))
            .Callback<BatchUpdateSpreadsheetRequest>(r => capturedRequest = r)
            .ReturnsAsync(new BatchUpdateSpreadsheetResponse { Replies = new List<Response>() });

        mockService.Setup(s => s.GetSheetInfo()).ReturnsAsync(new Spreadsheet { Sheets = new List<Sheet>() });

        var manager = new GoogleSheetManager(mockService.Object);

        var result = await manager.CreateSheets(sheetsToCreate);

        Assert.NotNull(capturedRequest);

        var addSheetRequests = capturedRequest.Requests.Where(r => r.AddSheet != null).ToList();
        Assert.True(addSheetRequests.Count >= 2, "Expected AddSheet requests for the created sheets");

        foreach (var kv in sheetsToCreate)
        {
            var add = addSheetRequests.FirstOrDefault(r => string.Equals(r.AddSheet.Properties.Title, kv.Key, StringComparison.OrdinalIgnoreCase));
            Assert.NotNull(add);
            Assert.Equal(kv.Value, add.AddSheet.Properties.Index);
        }
    }

    [Fact]
    public async Task CreateSheets_WhenBatchUpdateFails_ReturnsErrorPerSheet()
    {
        var mockService = new Mock<IGoogleSheetService>();

        mockService.Setup(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>()))
            .ReturnsAsync((BatchUpdateSpreadsheetResponse?)null);
        mockService.Setup(s => s.GetSheetInfo()).ReturnsAsync(new Spreadsheet { Sheets = new List<Sheet>() });

        var manager = new GoogleSheetManager(mockService.Object);

        var result = await manager.CreateSheets([SheetsConfig.SheetNames.Applications]);

        Assert.Contains(result.Messages, m => m.Message.Contains("Applications") && m.Message.Contains("not created"));
    }
}
