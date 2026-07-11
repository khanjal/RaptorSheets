using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Services;
using Moq;
using System.Collections.Generic;
using Xunit;
using Google.Apis.Sheets.v4.Data;
using System.Threading.Tasks;

namespace RaptorSheets.Core.Tests.Unit.Helpers
{
    public class SheetFetchCoordinatorTests
    {
        [Fact]
        public async Task TryGetBatchDataWithCreateOnFailure_SucceedsOnFirstAttempt_ReturnsResponse()
        {
            var mockSvc = new Mock<IGoogleSheetService>();
            var sheets = new List<string> { "Trips" };

            mockSvc.Setup(s => s.GetBatchData(sheets, null)).ReturnsAsync(new BatchGetValuesByDataFilterResponse());

            var result = await SheetFetchCoordinator.TryGetBatchDataWithCreateOnFailure(mockSvc.Object, sheets);

            Assert.NotNull(result);
            mockSvc.Verify(s => s.GetBatchData(sheets, null), Times.Once);
            mockSvc.Verify(s => s.BatchUpdateSpreadsheet(It.IsAny<Google.Apis.Sheets.v4.Data.BatchUpdateSpreadsheetRequest>()), Times.Never);
        }

        [Fact]
        public async Task TryGetBatchDataWithCreateOnFailure_FailsThenCreatesAndRetries_ReturnsResponse()
        {
            var mockSvc = new Mock<IGoogleSheetService>();
            var sheets = new List<string> { "Trips" };

            // First call returns null
            mockSvc.SetupSequence(s => s.GetBatchData(sheets, null))
                .ReturnsAsync((BatchGetValuesByDataFilterResponse?)null)
                .ReturnsAsync(new BatchGetValuesByDataFilterResponse());

            // EnsureMissingSheetsCreatedAsync will call GetSheetInfo and BatchUpdateSpreadsheet internally via helper.
            mockSvc.SetupSequence(s => s.GetSheetInfo())
                .ReturnsAsync(new Spreadsheet { Sheets = new List<Sheet>() })
                .ReturnsAsync(new Spreadsheet { Sheets = new List<Sheet> { new Sheet { Properties = new SheetProperties { Title = "Trips", Index = 0 } } } });

            mockSvc.Setup(s => s.BatchUpdateSpreadsheet(It.IsAny<Google.Apis.Sheets.v4.Data.BatchUpdateSpreadsheetRequest>())).ReturnsAsync(new BatchUpdateSpreadsheetResponse());

            var result = await SheetFetchCoordinator.TryGetBatchDataWithCreateOnFailure(mockSvc.Object, sheets);

            Assert.NotNull(result);
            mockSvc.Verify(s => s.GetBatchData(sheets, null), Times.Exactly(2));
            mockSvc.Verify(s => s.BatchUpdateSpreadsheet(It.IsAny<Google.Apis.Sheets.v4.Data.BatchUpdateSpreadsheetRequest>()), Times.Once);
        }

        [Fact]
        public async Task TryGetBatchDataWithCreateOnFailure_FailsAndNoCreate_ReturnsNull()
        {
            var mockSvc = new Mock<IGoogleSheetService>();
            var sheets = new List<string> { "Trips" };

            mockSvc.Setup(s => s.GetBatchData(sheets, null)).ReturnsAsync((BatchGetValuesByDataFilterResponse?)null);
            mockSvc.Setup(s => s.GetSheetInfo()).ReturnsAsync(new Spreadsheet { Sheets = new List<Sheet>() });
            mockSvc.Setup(s => s.BatchUpdateSpreadsheet(It.IsAny<Google.Apis.Sheets.v4.Data.BatchUpdateSpreadsheetRequest>())).ReturnsAsync((BatchUpdateSpreadsheetResponse?)null);

            var result = await SheetFetchCoordinator.TryGetBatchDataWithCreateOnFailure(mockSvc.Object, sheets);

            Assert.Null(result);
            mockSvc.Verify(s => s.GetBatchData(sheets, null), Times.Once);
            mockSvc.Verify(s => s.BatchUpdateSpreadsheet(It.IsAny<Google.Apis.Sheets.v4.Data.BatchUpdateSpreadsheetRequest>()), Times.Once);
        }
    }
}
