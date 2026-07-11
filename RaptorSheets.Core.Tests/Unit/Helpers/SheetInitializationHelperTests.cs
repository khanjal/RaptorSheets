using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Helpers;
using Xunit;

namespace RaptorSheets.Core.Tests.Unit.Helpers
{
    public class SheetInitializationHelperTests
    {
        [Fact]
        public void GetMissingSheets_NullOrEmptySheets_ReturnsEmpty()
        {
            var spreadsheet = new Spreadsheet { Sheets = new List<Sheet>() };

            Assert.Empty(SheetInitializationHelper.GetMissingSheets(spreadsheet, new List<string>()));
            Assert.Empty(SheetInitializationHelper.GetMissingSheets(spreadsheet, null!));
        }

        [Fact]
        public void GetMissingSheets_AllExist_ReturnsEmpty()
        {
            var spreadsheet = new Spreadsheet
            {
                Sheets = new List<Sheet>
                {
                    new Sheet { Properties = new SheetProperties { Title = "Trips", Index = 0 } }
                }
            };

            var result = SheetInitializationHelper.GetMissingSheets(spreadsheet, new List<string> { "Trips" });

            Assert.Empty(result);
        }

        [Fact]
        public void GetMissingSheets_MissingSheet_ReturnsWithIndex()
        {
            var spreadsheet = new Spreadsheet { Sheets = new List<Sheet>() };

            var result = SheetInitializationHelper.GetMissingSheets(spreadsheet, new List<string> { "Trips" });

            Assert.Single(result);
            Assert.True(result.ContainsKey("Trips"));
        }

        [Fact]
        public void GetMissingSheets_NullSpreadsheet_ReturnsMissingWithFallbackIndex()
        {
            var result = SheetInitializationHelper.GetMissingSheets(null, new List<string> { "Trips" });

            Assert.Single(result);
            Assert.True(result.ContainsKey("Trips"));
        }

        [Fact]
        public void GetMissingSheets_PartiallyMissing_ReturnsOnlyMissing()
        {
            var spreadsheet = new Spreadsheet
            {
                Sheets = new List<Sheet>
                {
                    new Sheet { Properties = new SheetProperties { Title = "Trips", Index = 0 } }
                }
            };

            var result = SheetInitializationHelper.GetMissingSheets(spreadsheet, new List<string> { "Trips", "Shifts" });

            Assert.Single(result);
            Assert.True(result.ContainsKey("Shifts"));
            Assert.False(result.ContainsKey("Trips"));
        }
    }
}
