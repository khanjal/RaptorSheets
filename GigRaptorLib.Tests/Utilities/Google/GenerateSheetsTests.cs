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
                var sheetId = result.Requests[0].AddSheet.Properties.SheetId;

                // Check on if it had to expand the number of rows (headers > 26)
                if (sheet.Headers.Count > 26)
                {
                    var appendDimension = result.Requests.First(x => x.AppendDimension != null).AppendDimension;
                    appendDimension.Dimension.Should().Be("COLUMNS");
                    appendDimension.Length.Should().Be(sheet.Headers.Count - 26);
                    appendDimension.SheetId.Should().Be(sheetId);
                }

                var appendCells = result.Requests.First(x => x.AppendCells != null).AppendCells;
                appendCells.SheetId.Should().Be(sheetId);
                appendCells.Rows.Should().HaveCount(1);
                appendCells.Rows[0].Values.Should().HaveCount(sheet.Headers.Count);
            }
        }

        [Fact]
        public void GivenSheet_ThenReturnSheetBanding()
        {
            foreach (var sheet in _sheets)
            {
                Console.WriteLine(sheet.Name);
                var result = GenerateSheets.Generate([sheet]);
                var index = 2;
                var sheetId = result.Requests[0].AddSheet.Properties.SheetId;

                // Check on if it had to expand the number of rows (headers > 26)
                if (sheet.Headers.Count > 26)
                {
                    //index++;
                }

                var bandedRange = result.Requests.First(x => x.AddBanding != null).AddBanding.BandedRange;
                bandedRange.Range.SheetId.Should().Be(sheetId);
                bandedRange.RowProperties.HeaderColor.Should().BeEquivalentTo(SheetHelper.GetColor(sheet.TabColor));
                bandedRange.RowProperties.SecondBandColor.Should().BeEquivalentTo(SheetHelper.GetColor(sheet.CellColor));
            }
        }

        // Protected range request (if sheet isn't protected)

        // RepeatCellRequest Test format column cells?

        // Banding request (alternating colors)

        // Sheet protected request (if true)
    }
}
