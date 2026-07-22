using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Stock.Enums;
using RaptorSheets.Stock.Helpers;
using Xunit;

namespace RaptorSheets.Stock.Tests.Unit.Helpers
{
    public class StockSheetHelpersTests
    {
        [Fact]
        public void GetSheets_ShouldReturnAllSheets()
        {
            // Act
            var result = StockSheetHelpers.GetSheets();

            // Assert
            Assert.Contains(result, sheet => sheet.Name == "Accounts");
            Assert.Contains(result, sheet => sheet.Name == "Stocks");
            Assert.Contains(result, sheet => sheet.Name == "Tickers");
        }

        [Fact]
        public void GetMissingSheets_ShouldReturnMissingSheets()
        {
            // Arrange
            var spreadsheet = new Spreadsheet
            {
                Sheets = new List<Sheet>
                {
                    new Sheet { Properties = new SheetProperties { Title = "Accounts" } }
                }
            };

            // Act
            var result = StockSheetHelpers.GetMissingSheets(spreadsheet);

            // Assert
            Assert.Contains(result, sheet => sheet.Name == "Stocks");
            Assert.Contains(result, sheet => sheet.Name == "Tickers");
        }

        [Fact]
        public void GetDataValidation_ShouldReturnBooleanValidation()
        {
            // Act
            var result = StockSheetHelpers.GetDataValidation(Validation.BOOLEAN);

            // Assert
            Assert.Equal("BOOLEAN", result.Condition.Type);
        }

        [Fact]
        public void GetDataValidation_ShouldReturnRangeValidation()
        {
            // Act
            var result = StockSheetHelpers.GetDataValidation(Validation.RANGE_ACCOUNT);

            // Assert
            Assert.Equal("ONE_OF_RANGE", result.Condition.Type);
            Assert.Contains(result.Condition.Values, v => v.UserEnteredValue == "=Accounts!A2:A");
        }

        [Fact]
        public void Registry_GetDependents_DetectsRealCrossSheetFormulaGraph()
        {
            // SheetRegistry.GetDependents derives dependencies dynamically by scanning each mapper's
            // built formulas for other sheets' quoted range pattern ('Name'!) - no manual
            // declaration. This confirms it actually finds Stock's real graph:
            // StockMapper.GetSheet() reads tickerSheet.GetRange(...), and AccountMapper.GetSheet()
            // separately reads stockSheet.GetRange(...) - so Accounts transitively depends on
            // Tickers too, through Stocks.
            var tickersDependents = StockSheetHelpers.Registry.GetDependents(["Tickers"]);

            Assert.Contains("Stocks", tickersDependents);
            Assert.Contains("Accounts", tickersDependents);

            var stocksDependents = StockSheetHelpers.Registry.GetDependents(["Stocks"]);

            Assert.Contains("Accounts", stocksDependents);
        }

        [Fact]
        public void MapData_ShouldReturnSheetEntity()
        {
            // Arrange
            var response = new BatchGetValuesByDataFilterResponse
            {
                ValueRanges = new List<MatchedValueRange>
                {
                    new MatchedValueRange
                    {
                        DataFilters = new List<DataFilter> { new DataFilter { A1Range = "Accounts" } },
                        ValueRange = new ValueRange { Values = new List<IList<object>> { new List<object> { "Header1", "Header2" } } }
                    }
                }
            };

            // Act
            var result = StockSheetHelpers.MapData(response);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Sheets.Accounts);
        }
    }
}
