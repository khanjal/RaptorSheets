using Google.Apis.Sheets.v4.Data;
using Moq;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Services;
using Xunit;

namespace RaptorSheets.Core.Tests.Unit.Helpers
{
    public class SheetInitializationHelperTests
    {
        [Fact]
        public async Task EnsureMissingSheetsCreatedAsync_NullService_Throws()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => SheetInitializationHelper.EnsureMissingSheetsCreatedAsync(null!, new List<string> { "Trips" }));
        }

        [Fact]
        public async Task EnsureMissingSheetsCreatedAsync_EmptyOrNullSheets_ReturnsEmpty()
        {
            var mockSvc = new Mock<IGoogleSheetService>();

            var (foundEmpty, createdEmpty) = await SheetInitializationHelper.EnsureMissingSheetsCreatedAsync(mockSvc.Object, new List<string>());
            Assert.False(createdEmpty);
            Assert.Empty(foundEmpty);

            var (foundNull, createdNull) = await SheetInitializationHelper.EnsureMissingSheetsCreatedAsync(mockSvc.Object, (List<string>)null);
            Assert.False(createdNull);
            Assert.Empty(foundNull);
        }

        [Fact]
        public async Task EnsureMissingSheetsCreatedAsync_AllExist_ReturnsFound_NoCreate()
        {
            var mockSvc = new Mock<IGoogleSheetService>();
            var sheets = new List<string> { "Trips" };

            mockSvc.Setup(s => s.GetSheetInfo()).ReturnsAsync(new Spreadsheet
            {
                Sheets = new List<Sheet>
                {
                    new Sheet { Properties = new SheetProperties { Title = "Trips", Index = 0 } }
                }
            });

            var (found, created) = await SheetInitializationHelper.EnsureMissingSheetsCreatedAsync(mockSvc.Object, sheets);

            Assert.False(created);
            Assert.Contains("Trips", found, StringComparer.OrdinalIgnoreCase);
            mockSvc.Verify(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>()), Times.Never);
        }

        [Fact]
        public async Task EnsureMissingSheetsCreatedAsync_MissingSheets_CreatesAndReturnsFound()
        {
            var mockSvc = new Mock<IGoogleSheetService>();
            var sheets = new List<string> { "Trips" };

            mockSvc.SetupSequence(s => s.GetSheetInfo())
                .ReturnsAsync(new Spreadsheet { Sheets = new List<Sheet>() })
                .ReturnsAsync(new Spreadsheet
                {
                    Sheets = new List<Sheet>
                    {
                        new Sheet { Properties = new SheetProperties { Title = "Trips", Index = 0 } }
                    }
                });

            mockSvc.Setup(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>()))
                .ReturnsAsync(new BatchUpdateSpreadsheetResponse());

            var (found, created) = await SheetInitializationHelper.EnsureMissingSheetsCreatedAsync(mockSvc.Object, sheets);

            Assert.True(created);
            Assert.Contains("Trips", found, StringComparer.OrdinalIgnoreCase);
            mockSvc.Verify(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>()), Times.Once);
        }

        [Fact]
        public async Task EnsureMissingSheetsCreatedAsync_BatchUpdateReturnsNull_ReturnsRequestedButNotCreated()
        {
            var mockSvc = new Mock<IGoogleSheetService>();
            var sheets = new List<string> { "Trips" };

            mockSvc.Setup(s => s.GetSheetInfo()).ReturnsAsync(new Spreadsheet { Sheets = new List<Sheet>() });
            mockSvc.Setup(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>())).ReturnsAsync((BatchUpdateSpreadsheetResponse?)null);

            var (found, created) = await SheetInitializationHelper.EnsureMissingSheetsCreatedAsync(mockSvc.Object, sheets);

            Assert.False(created);
            Assert.Contains("Trips", found, StringComparer.OrdinalIgnoreCase);
            mockSvc.Verify(s => s.BatchUpdateSpreadsheet(It.IsAny<BatchUpdateSpreadsheetRequest>()), Times.Once);
        }

        [Fact]
        public async Task EnsureMissingSheetsCreatedAsync_GetSheetInfoThrows_ReturnsRequestedDistinctAndNotCreated()
        {
            var mockSvc = new Mock<IGoogleSheetService>();
            var sheets = new List<string> { "Trips", "Trips" };

            mockSvc.Setup(s => s.GetSheetInfo()).ThrowsAsync(new Exception("boom"));

            var (found, created) = await SheetInitializationHelper.EnsureMissingSheetsCreatedAsync(mockSvc.Object, sheets);

            Assert.False(created);
            Assert.Single(found);
            Assert.Contains("Trips", found, StringComparer.OrdinalIgnoreCase);
        }
    }
}
