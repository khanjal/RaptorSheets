using System.Collections.Generic;
using System.Linq;
using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Helpers;
using Xunit;

namespace RaptorSheets.Core.Tests.Unit.Helpers
{
    public class SheetOrderingHelperTests
    {
        [Fact]
        public void BuildAddSheetRequests_WithNoRequestedSheets_ReturnsEmpty()
        {
            // Arrange
            var spreadsheet = new Spreadsheet { Sheets = new List<Sheet>() };
            var requested = new List<string>();

            // Act
            var requests = SheetOrderingHelper.BuildAddSheetRequests(spreadsheet, requested);

            // Assert
            Assert.NotNull(requests);
            Assert.Empty(requests);
        }

        [Fact]
        public void BuildAddSheetRequests_AllExist_ReturnsEmpty()
        {
            // Arrange - spreadsheet already contains the requested sheets
            var spreadsheet = new Spreadsheet
            {
                Sheets = new List<Sheet>
                {
                    new Sheet { Properties = new SheetProperties { Title = "Alpha", Index = 0 } },
                    new Sheet { Properties = new SheetProperties { Title = "Beta", Index = 1 } },
                }
            };
            var requested = new List<string> { "Alpha", "Beta" };

            // Act
            var requests = SheetOrderingHelper.BuildAddSheetRequests(spreadsheet, requested);

            // Assert
            Assert.NotNull(requests);
            Assert.Empty(requests);
        }

        [Fact]
        public void BuildAddSheetRequests_InsertBeforeNextExisting_PreservesRequestedOrder()
        {
            // Arrange - only 'C' exists at index 2; requested order is A, B, C
            var spreadsheet = new Spreadsheet
            {
                Sheets = new List<Sheet>
                {
                    new Sheet { Properties = new SheetProperties { Title = "X", Index = 0 } },
                    new Sheet { Properties = new SheetProperties { Title = "Y", Index = 1 } },
                    new Sheet { Properties = new SheetProperties { Title = "C", Index = 2 } },
                }
            };

            var requested = new List<string> { "A", "B", "C" };

            // Act
            var requests = SheetOrderingHelper.BuildAddSheetRequests(spreadsheet, requested).ToList();

            // Assert - two requests (for A and B)
            Assert.Equal(2, requests.Count);

            // The helper orders insertions so that applying them in sequence will result
            // in the requested order A, B, C. The returned requests are in the order
            // they should be executed (descending target index), so B comes before A.
            Assert.Equal("B", requests[0].AddSheet.Properties.Title);
            Assert.Equal(2, requests[0].AddSheet.Properties.Index);

            Assert.Equal("A", requests[1].AddSheet.Properties.Title);
            Assert.Equal(2, requests[1].AddSheet.Properties.Index);
        }

        [Fact]
        public void BuildAddSheetRequests_AppendAtEnd_PreservesRelativeOrder()
        {
            // Arrange - no existing sheets
            var spreadsheet = new Spreadsheet { Sheets = new List<Sheet>() };
            var requested = new List<string> { "One", "Two", "Three" };

            // Act
            var requests = SheetOrderingHelper.BuildAddSheetRequests(spreadsheet, requested).ToList();

            // Assert - three requests
            Assert.Equal(3, requests.Count);

            // Returned insertion order is descending indexes: Three, Two, One
            Assert.Equal("Three", requests[0].AddSheet.Properties.Title);
            Assert.Equal(2, requests[0].AddSheet.Properties.Index);

            Assert.Equal("Two", requests[1].AddSheet.Properties.Title);
            Assert.Equal(1, requests[1].AddSheet.Properties.Index);

            Assert.Equal("One", requests[2].AddSheet.Properties.Title);
            Assert.Equal(0, requests[2].AddSheet.Properties.Index);
        }

        [Fact]
        public void BuildAddSheetRequests_IsCaseInsensitiveForExistingDetection()
        {
            // Arrange - existing sheet 'Existing' but requested uses different case
            var spreadsheet = new Spreadsheet
            {
                Sheets = new List<Sheet>
                {
                    new Sheet { Properties = new SheetProperties { Title = "Existing", Index = 0 } }
                }
            };
            var requested = new List<string> { "newone", "Existing" };

            // Act
            var requests = SheetOrderingHelper.BuildAddSheetRequests(spreadsheet, requested).ToList();

            // Assert - only one request for newone and it should insert before Existing (index 0)
            Assert.Single(requests);
            Assert.Equal("newone", requests[0].AddSheet.Properties.Title);
            Assert.Equal(0, requests[0].AddSheet.Properties.Index);
        }
    }
}
