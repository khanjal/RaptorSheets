using FluentAssertions;
using GigRaptorLib.Constants;
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
        private List<SheetModel> _sheetConfigs;
        private BatchUpdateSpreadsheetRequest _request;

        public GenerateSheetsTests()
        {
            _sheetConfigs =
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

            _request = GenerateSheets.Generate(_sheetConfigs);
        }

        [Fact]
        public void GivenSheetConfig_ThenReturnSheetRequest()
        {
            foreach (var sheet in _sheetConfigs)
            {
                var result = GenerateSheets.Generate([sheet]);
                var index = 0; // AddSheet should be first request

                result.Requests[index].AddSheet.Should().NotBeNull();

                var sheetRequest = result.Requests[index].AddSheet;
                sheetRequest.Properties.Title.Should().Be(sheet.Name);
                sheetRequest.Properties.TabColor.Should().BeEquivalentTo(SheetHelper.GetColor(sheet.TabColor));
                sheetRequest.Properties.GridProperties.FrozenColumnCount.Should().Be(sheet.FreezeColumnCount);
                sheetRequest.Properties.GridProperties.FrozenRowCount.Should().Be(sheet.FreezeRowCount);
            }
        }

        [Fact]
        public void GivenSheetHeaders_ThenReturnSheetHeaders()
        {
            foreach (var sheet in _sheetConfigs)
            {
                var result = GenerateSheets.Generate([sheet]);
                var sheetId = result.Requests.First().AddSheet.Properties.SheetId;

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
        public void GivenSheetColors_ThenReturnSheetBanding()
        {
            foreach (var sheet in _sheetConfigs)
            {
                var result = GenerateSheets.Generate([sheet]);
                var sheetId = result.Requests.First().AddSheet.Properties.SheetId;

                var bandedRange = result.Requests.First(x => x.AddBanding != null).AddBanding.BandedRange;
                bandedRange.Range.SheetId.Should().Be(sheetId);
                bandedRange.RowProperties.HeaderColor.Should().BeEquivalentTo(SheetHelper.GetColor(sheet.TabColor));
                bandedRange.RowProperties.SecondBandColor.Should().BeEquivalentTo(SheetHelper.GetColor(sheet.CellColor));
            }
        }

        [Fact]
        public void GivenSheetProtected_ThenReturnProtectRequest()
        {
            foreach (var sheet in _sheetConfigs)
            {
                var result = GenerateSheets.Generate([sheet]);
                var sheetId = result.Requests.First().AddSheet.Properties.SheetId;
                var protectRange = result.Requests.Where(x => x.AddProtectedRange != null).ToList();

                if (!sheet.ProtectSheet)
                {
                    continue;
                }

                protectRange.Should().HaveCount(1);
                var sheetProtection = protectRange.First().AddProtectedRange.ProtectedRange;
                sheetProtection.Range.SheetId.Should().Be(sheetId);
                sheetProtection.Description.Should().Be(ProtectionWarnings.SheetWarning);
                sheetProtection.WarningOnly.Should().BeTrue();
            }
        }

        [Fact]
        public void GivenSheetNotProtected_ThenReturnProtectRequests()
        {
            foreach (var sheet in _sheetConfigs)
            {
                var result = GenerateSheets.Generate([sheet]);
                var sheetId = result.Requests.First().AddSheet.Properties.SheetId;
                var protectRange = result.Requests.Where(x => x.AddProtectedRange != null).ToList();

                if (sheet.ProtectSheet)
                {
                    continue;
                }

                var columnProtections = sheet.Headers.Where(x => x.Formula != null).ToList();

                protectRange.Should().HaveCount(columnProtections.Count + 1); // +1 for header protection

                for (var i = 0; i < protectRange.Count; i++)
                {
                    var protectedRange = protectRange[i].AddProtectedRange.ProtectedRange;
                    protectedRange.Range.SheetId.Should().Be(sheetId);
                    protectedRange.WarningOnly.Should().BeTrue();

                    if (i == protectRange.Count - 1) // Header protection (last) 
                    {
                        protectedRange.Description.Should().Be(ProtectionWarnings.HeaderWarning);
                    }
                    else
                    {
                        protectedRange.Description.Should().Be(ProtectionWarnings.ColumnWarning);
                    }
                }
            }
        }

        [Fact]
        public void GivenSheetHeaderFormatOrValidation_ThenReturnRepeatCellsRequest()
        {
            foreach (var sheet in _sheetConfigs)
            {
                var result = GenerateSheets.Generate([sheet]);
                var sheetId = result.Requests.First().AddSheet.Properties.SheetId;
                var repeatCells = result.Requests.Where(x => x.RepeatCell != null).ToList();
                var repeatHeaders = sheet.Headers.Where(x => x.Format != null || x.Validation != null).ToList();

                repeatCells.Should().HaveCount(repeatHeaders.Count);
            }
        }
    }
}
