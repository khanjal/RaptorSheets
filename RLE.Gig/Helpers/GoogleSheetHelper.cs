using Google.Apis.Sheets.v4.Data;
using RLE.Gig.Enums;
using RLE.Gig.Mappers;
using RLE.Core.Constants;
using RLE.Core.Utilities;
using RLE.Core.Enums;
using RLE.Core.Models.Google;

namespace RLE.Gig.Helpers;

public static class GoogleSheetHelper
{
    private static SheetModel? _sheet;
    private static int? _sheetId;
    private static BatchUpdateSpreadsheetRequest? _batchUpdateSpreadsheetRequest;
    private static List<RepeatCellRequest>? _repeatCellRequests;

    public static BatchUpdateSpreadsheetRequest Generate(List<SheetEnum> sheets)
    {
        _batchUpdateSpreadsheetRequest = new BatchUpdateSpreadsheetRequest();
        _batchUpdateSpreadsheetRequest.Requests = [];
        _repeatCellRequests = [];

        sheets.ForEach(sheet =>
        {
            _sheet = GetSheetModel(sheet);
            var random = new Random();
            _sheetId = random.Next();

            GenerateSheetPropertes();
            GenerateAppendDimension();
            GenerateAppendCells();
            GenerateHeadersFormatAndProtection();
            GenerateBandingRequest();
            GenerateProtectedRangeForHeaderOrSheet();
        });

        _repeatCellRequests.ForEach(request =>
        {
            _batchUpdateSpreadsheetRequest.Requests.Add(new Request { RepeatCell = request });
        });

        return _batchUpdateSpreadsheetRequest;
    }

    private static SheetModel GetSheetModel(SheetEnum sheetEnum)
    {
        return sheetEnum switch
        {
            SheetEnum.ADDRESSES => AddressMapper.GetSheet(),
            SheetEnum.DAILY => DailyMapper.GetSheet(),
            SheetEnum.MONTHLY => MonthlyMapper.GetSheet(),
            SheetEnum.NAMES => NameMapper.GetSheet(),
            SheetEnum.PLACES => PlaceMapper.GetSheet(),
            SheetEnum.REGIONS => RegionMapper.GetSheet(),
            SheetEnum.SERVICES => ServiceMapper.GetSheet(),
            SheetEnum.SHIFTS => ShiftMapper.GetSheet(),
            SheetEnum.TRIPS => TripMapper.GetSheet(),
            SheetEnum.TYPES => TypeMapper.GetSheet(),
            SheetEnum.WEEKDAYS => WeekdayMapper.GetSheet(),
            SheetEnum.WEEKLY => WeeklyMapper.GetSheet(),
            SheetEnum.YEARLY => YearlyMapper.GetSheet(),
            _ => throw new NotImplementedException(),
        };
    }

    private static void GenerateAppendCells()
    {
        // Create Sheet Headers
        var appendCellsRequest = new AppendCellsRequest
        {
            Fields = GoogleConfig.FieldsUpdate,
            Rows = SheetHelpers.HeadersToRowData(_sheet!),
            SheetId = _sheetId
        };

        _batchUpdateSpreadsheetRequest!.Requests.Add(new Request { AppendCells = appendCellsRequest });
    }

    private static void GenerateAppendDimension()
    {
        // Append more columns if the default amount isn't enough
        var defaultColumns = GoogleConfig.DefaultColumnCount;
        if (_sheet!.Headers.Count > defaultColumns)
        {
            var appendDimensionRequest = new AppendDimensionRequest
            {
                Dimension = GoogleConfig.AppendDimensionType,
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
                RowProperties = new BandingProperties { HeaderColor = SheetHelpers.GetColor(_sheet!.TabColor), FirstBandColor = SheetHelpers.GetColor(ColorEnum.WHITE), SecondBandColor = SheetHelpers.GetColor(_sheet!.CellColor) }
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
                Fields = GoogleConfig.FieldsUpdate,
                Range = range,
                Cell = new CellData()
            };

            if (header.Format != null)
            {
                repeatCellRequest.Cell.UserEnteredFormat = SheetHelpers.GetCellFormat((FormatEnum)header.Format);
            }

            if (header.Validation != null)
            {
                repeatCellRequest.Cell.DataValidation = GigSheetHelpers.GetDataValidation((ValidationEnum)header.Validation);
            }

            _repeatCellRequests!.Add(repeatCellRequest);
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
                // Create Sheet With Properties
                SheetId = _sheetId,
                Title = _sheet!.Name,
                TabColor = SheetHelpers.GetColor(_sheet.TabColor),
                GridProperties = new GridProperties { FrozenColumnCount = _sheet.FreezeColumnCount, FrozenRowCount = _sheet.FreezeRowCount }
            }
        };

        _batchUpdateSpreadsheetRequest!.Requests.Add(new Request { AddSheet = sheetRequest });
    }
}
