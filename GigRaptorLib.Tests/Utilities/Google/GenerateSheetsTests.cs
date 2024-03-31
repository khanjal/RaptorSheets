using FluentAssertions;
using GigRaptorLib.Mappers;
using GigRaptorLib.Models;
using GigRaptorLib.Utilities;
using GigRaptorLib.Utilities.Google;
using Google.Apis.Sheets.v4.Data;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;

namespace GigRaptorLib.Tests.Utilities.Google
{
    public class GenerateSheetsTests
    {
        private List<SheetModel> _sheets;
        private BatchUpdateSpreadsheetRequest _request;

        public GenerateSheetsTests()
        {
            _sheets =
            [
                AddressMapper.GetSheet(),
                DailyMapper.GetSheet(),
                MonthlyMapper.GetSheet(),
                NameMapper.GetSheet(),
                PlaceMapper.GetSheet(),
                RegionMapper.GetSheet(),
                ServiceMapper.GetSheet(),
                ShiftMapper.GetSheet(),
                TripMapper.GetSheet(),
                TypeMapper.GetSheet(),
                WeekdayMapper.GetSheet(),
                WeeklyMapper.GetSheet(),
                YearlyMapper.GetSheet()
            ];

            _request = GenerateSheets.Generate(_sheets);
        }

        [Fact]
        public void GivenSheet_ThenReturnSheetRequest()
        {
            foreach (var sheet in _sheets)
            {
                var result = GenerateSheets.Generate([sheet]);
                var index = 0;

                result.Requests[index].AddSheet.Should().NotBeNull();

                var sheetRequest = result.Requests[index].AddSheet;
                sheetRequest.Properties.Title.Should().Be(sheet.Name);
                sheetRequest.Properties.TabColor.Should().BeEquivalentTo(SheetHelper.GetColor(sheet.TabColor));
                sheetRequest.Properties.GridProperties.FrozenColumnCount.Should().Be(sheet.FreezeColumnCount);
                sheetRequest.Properties.GridProperties.FrozenRowCount.Should().Be(sheet.FreezeRowCount);
            }
        }

        [Fact]
        public void GivenSheet_ThenReturnSheetHeaders()
        {
            foreach (var sheet in _sheets)
            {
                var result = GenerateSheets.Generate([sheet]);
                var index = 1;
                var sheetId = result.Requests[0].AddSheet.Properties.SheetId;

                // Check on if it had to expand the number of rows (headers > 26)
                if (sheet.Headers.Count > 26)
                {
                    result.Requests[index].AppendDimension.Should().NotBeNull();
                    
                    var appendDimension = result.Requests[index].AppendDimension;
                    appendDimension.Dimension.Should().Be("COLUMNS");
                    appendDimension.Length.Should().Be(sheet.Headers.Count - 26);
                    appendDimension.SheetId.Should().Be(sheetId);
                    index++;
                }

                result.Requests[index].AppendCells.Should().NotBeNull();

                var appendCells = result.Requests[index].AppendCells;
                appendCells.SheetId.Should().Be(sheetId);
                appendCells.Rows.Should().HaveCount(1);
                appendCells.Rows[0].Values.Should().HaveCount(sheet.Headers.Count);
            }
        }
    }
}
