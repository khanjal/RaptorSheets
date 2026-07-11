using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Sheets.v4.Data;
using Moq;
using RaptorSheets.Core.Services;
using RaptorSheets.Gig.Managers;
using Xunit;

namespace RaptorSheets.Gig.Tests.Unit.Managers;

public class CreateSheetsBehaviorTests
{
    [Fact]
    public async Task CreateSheets_WithDesiredIndexMap_SetsAddSheetIndices()
    {
        // Arrange
        var sheetsToCreate = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["Expenses"] = 1,
            ["Shifts"] = 0
        };

        var mockService = new Mock<IGoogleSheetService>();

        // Capture the BatchUpdateSpreadsheetRequest
        BatchUpdateSpreadsheetRequest? capturedRequest = null;
        mockService.Setup(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>()))
            .Callback<BatchUpdateSpreadsheetRequest>(r => capturedRequest = r)
            .ReturnsAsync(new BatchUpdateSpreadsheetResponse { Replies = new List<Response>() });

        // Return a Spreadsheet when GetSheetInfo is called so manager can compute defaults
        mockService.Setup(s => s.GetSheetInfo()).ReturnsAsync(new Spreadsheet { Sheets = new List<Sheet>() });

        var manager = new GoogleSheetManager(mockService.Object);

        // Act
        var result = await manager.CreateSheets(sheetsToCreate);

        // Assert
        Assert.NotNull(capturedRequest);

        // Ensure AddSheet requests have Index set according to the provided map
        var addSheetRequests = capturedRequest.Requests.Where(r => r.AddSheet != null).ToList();

        Assert.True(addSheetRequests.Count >= 2, "Expected AddSheet requests for the created sheets");

        foreach (var kv in sheetsToCreate)
        {
            var add = addSheetRequests.FirstOrDefault(r => string.Equals(r.AddSheet.Properties.Title, kv.Key, StringComparison.OrdinalIgnoreCase));
            Assert.NotNull(add);
            Assert.Equal(kv.Value, add.AddSheet.Properties.Index);
        }
    }
}
