using GigRaptorLib.Constants;
using GigRaptorLib.Enums;
using GigRaptorLib.Models;
using Google.Apis.Sheets.v4.Data;

namespace GigRaptorLib.Utilities.Google
{
    public static class GenerateSheets
    {
        private static SheetModel? _sheet;
        private static int? _sheetId;
        private static BatchUpdateSpreadsheetRequest? _batchUpdateSpreadsheetRequest;
        private static List<RepeatCellRequest>? _repeatCellRequests;
        
        public static BatchUpdateSpreadsheetRequest Generate(List<SheetModel> sheets)
        {
            _batchUpdateSpreadsheetRequest = new BatchUpdateSpreadsheetRequest();
            _batchUpdateSpreadsheetRequest.Requests = [];
            _repeatCellRequests = new List<RepeatCellRequest>();

            sheets.ForEach(sheet =>
            {
                _sheet = sheet;
                var random = new Random();
                _sheetId = random.Next();

                GenerateSheetPropertes();
                GenerateAppendDimension();
                GenerateAppendCells();
                GenerateHeadersFormatAndProtection();
                GenerateBandingRequest();
                GenerateProtectedRangeForHeaderOrSheet();

                // _sheet.Messages.Add($"Sheet [{sheet.Name}]: Added");
            });

            _repeatCellRequests.ForEach(request => {
                _batchUpdateSpreadsheetRequest.Requests.Add(new Request { RepeatCell = request });
            });

            return _batchUpdateSpreadsheetRequest;
            // Console.WriteLine(JsonSerializer.Serialize(batchUpdateSpreadsheetRequest.Requests));
            //var batchUpdateRequest = _googleSheetService.Spreadsheets.BatchUpdate(batchUpdateSpreadsheetRequest, _spreadsheetId);
            //batchUpdateRequest.Execute();
        }

        private static void GenerateAppendCells()
        {
            // Create Sheet Headers
            var appendCellsRequest = new AppendCellsRequest
            {
                Fields = "*",
                Rows = SheetHelper.HeadersToRowData(_sheet!),
                SheetId = _sheetId
            };

            _batchUpdateSpreadsheetRequest!.Requests.Add(new Request { AppendCells = appendCellsRequest });
        }

        private static void GenerateAppendDimension()
        {
            // Append more columns if the default amount isn't enough
            var defaultColumns = 26;
            if (_sheet!.Headers.Count > defaultColumns)
            {
                var appendDimensionRequest = new AppendDimensionRequest
                {
                    Dimension = "COLUMNS",
                    Length = _sheet.Headers.Count - defaultColumns,
                    SheetId = _sheetId
                };
                _batchUpdateSpreadsheetRequest!.Requests.Add(new Request { AppendDimension = appendDimensionRequest });
            }
        }

        private static void GenerateBandingRequest()
        {
            // Add alternating colors
            var addBandingRequest = new AddBandingRequest
            {
                BandedRange = new BandedRange
                {
                    BandedRangeId = _sheetId,
                    Range = new GridRange { SheetId = _sheetId },
                    RowProperties = new BandingProperties { HeaderColor = SheetHelper.GetColor(_sheet!.TabColor), FirstBandColor = SheetHelper.GetColor(ColorEnum.WHITE), SecondBandColor = SheetHelper.GetColor(_sheet!.CellColor) }
                }
            };
            _batchUpdateSpreadsheetRequest!.Requests.Add(new Request { AddBanding = addBandingRequest });
        }

        private static void GenerateHeadersFormatAndProtection()
        {
            // Format/Protect Column Cells
            _sheet!.Headers.ForEach(header =>
            {
                var range = new GridRange
                {
                    SheetId = _sheetId,
                    StartColumnIndex = header.Index,
                    EndColumnIndex = header.Index + 1,
                    StartRowIndex = 1,
                };

                // If whole sheet isn't protected then protect certain columns
                if (!string.IsNullOrEmpty(header.Formula) && !_sheet.ProtectSheet)
                {
                    var addProtectedRangeRequest = new AddProtectedRangeRequest
                    {
                        ProtectedRange = new ProtectedRange { Description = ProtectionWarnings.ColumnWarning, Range = range, WarningOnly = true }
                    };
                    // protectCellRequests.Add(addProtectedRangeRequest);
                    _batchUpdateSpreadsheetRequest!.Requests.Add(new Request { AddProtectedRange = addProtectedRangeRequest });
                }

                // If there's no format or validation then go to next header
                if (header.Format == null && header.Validation == null)
                {
                    return;
                }

                // Set start/end for formatting
                range.StartRowIndex = 1;
                range.EndRowIndex = null;

                var repeatCellRequest = new RepeatCellRequest
                {
                    Fields = "*",
                    Range = range,
                    Cell = new CellData()
                };

                if (header.Format != null)
                {
                    repeatCellRequest.Cell.UserEnteredFormat = SheetHelper.GetCellFormat((FormatEnum)header.Format);
                }

                if (header.Validation != null)
                {
                    repeatCellRequest.Cell.DataValidation = SheetHelper.GetDataValidation((ValidationEnum)header.Validation);
                }

                _repeatCellRequests!.Add(repeatCellRequest);
                //batchUpdateSpreadsheetRequest.Requests.Add(new Request { RepeatCell = repeatCellRequest });
            });
        }

        private static void GenerateProtectedRangeForHeaderOrSheet()
        {
            // Protect sheet or header
            var addProtectedRangeRequest = new AddProtectedRangeRequest();
            if (_sheet!.ProtectSheet)
            {
                addProtectedRangeRequest = new AddProtectedRangeRequest
                {
                    ProtectedRange = new ProtectedRange { Description = ProtectionWarnings.SheetWarning, Range = new GridRange { SheetId = _sheetId }, WarningOnly = true }
                };
                _batchUpdateSpreadsheetRequest!.Requests.Add(new Request { AddProtectedRange = addProtectedRangeRequest });
            }
            else
            {
                // Protect full header if sheet isn't protected.
                var range = new GridRange
                {
                    SheetId = _sheetId,
                    StartColumnIndex = 0,
                    EndColumnIndex = _sheet!.Headers.Count,
                    StartRowIndex = 0,
                    EndRowIndex = 1
                };

                addProtectedRangeRequest.ProtectedRange = new ProtectedRange { Description = ProtectionWarnings.HeaderWarning, Range = range, WarningOnly = true };
                _batchUpdateSpreadsheetRequest!.Requests.Add(new Request { AddProtectedRange = addProtectedRangeRequest });
            }
        }

        private static void GenerateSheetPropertes()
        {
            var sheetRequest = new AddSheetRequest
            {
                Properties = new SheetProperties
                {
                    // TODO: Make request helper to build these requests.

                    // Create Sheet With Properties
                    SheetId = _sheetId,
                    // sheetRequest.Properties.Title = $"{sheet.Name} {sheetId}";
                    Title = _sheet!.Name,
                    TabColor = SheetHelper.GetColor(_sheet.TabColor),
                    GridProperties = new GridProperties { FrozenColumnCount = _sheet.FreezeColumnCount, FrozenRowCount = _sheet.FreezeRowCount }
                }
            };

            _batchUpdateSpreadsheetRequest!.Requests.Add(new Request { AddSheet = sheetRequest });
        }
    }
}
