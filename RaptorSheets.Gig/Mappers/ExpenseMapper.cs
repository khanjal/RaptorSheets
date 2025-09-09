using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Enums;
using System.Xml;

namespace RaptorSheets.Gig.Mappers;

public static class ExpenseMapper
{
    public static List<ExpenseEntity> MapFromRangeData(IList<IList<object>> values)
    {
        var expenses = new List<ExpenseEntity>();
        var headers = new Dictionary<int, string>();
        values = values!.Where(x => !string.IsNullOrEmpty(x[0]?.ToString())).ToList();
        var id = 0;

        foreach (var value in values)
        {
            id++;
            if (id == 1)
            {
                headers = HeaderHelpers.ParserHeader(value);
                continue;
            }

            ExpenseEntity expense = new()
            {
                RowId = id,
                Date = DateTime.TryParse(HeaderHelpers.GetStringValue(SheetsConfig.HeaderNames.Date, value, headers), out var date) ? date : DateTime.MinValue,
                Name = HeaderHelpers.GetStringValue(SheetsConfig.HeaderNames.Name, value, headers),
                Description = HeaderHelpers.GetStringValue(SheetsConfig.HeaderNames.Description, value, headers),
                Amount = HeaderHelpers.GetDecimalValue(SheetsConfig.HeaderNames.Amount, value, headers),
                Category = HeaderHelpers.GetStringValue(SheetsConfig.HeaderNames.Category, value, headers)
            };

            expenses.Add(expense);
        }
        return expenses;
    }

    public static IList<IList<object?>> MapToRangeData(List<ExpenseEntity> expenses, IList<object> expenseHeaders)
    {
        var rangeData = new List<IList<object?>>();

        foreach (var expense in expenses)
        {
            var objectList = new List<object?>();

            foreach (var header in expenseHeaders)
            {
                var headerName = header.ToString()!.Trim();

                switch (headerName)
                {
                    case SheetsConfig.HeaderNames.Date:
                        objectList.Add(expense.Date.ToString("yyyy-MM-dd"));
                        break;
                    case SheetsConfig.HeaderNames.Name:
                        objectList.Add(expense.Name);
                        break;
                    case SheetsConfig.HeaderNames.Description:
                        objectList.Add(expense.Description);
                        break;
                    case SheetsConfig.HeaderNames.Amount:
                        objectList.Add(expense.Amount.ToString());
                        break;
                    case SheetsConfig.HeaderNames.Category:
                        objectList.Add(expense.Category);
                        break;
                    default:
                        objectList.Add(null);
                        break;
                }
            }

            rangeData.Add(objectList);
        }
        return rangeData;
    }

    public static IList<RowData> MapToRowData(List<ExpenseEntity> expenseEntities, IList<object> headers)
    {
        var rows = new List<RowData>();

        foreach (ExpenseEntity expense in expenseEntities)
        {
            var rowData = new RowData();
            var cells = new List<CellData>();
            foreach (var header in headers)
            {
                var headerName = header!.ToString()!.Trim();
                switch (headerName)
                {
                    case SheetsConfig.HeaderNames.Date:
                        cells.Add(new CellData { UserEnteredValue = new ExtendedValue { NumberValue = expense.Date.ToString("yyyy-MM-dd").ToSerialDate() } });
                        break;
                    case SheetsConfig.HeaderNames.Name:
                        cells.Add(new CellData { UserEnteredValue = new ExtendedValue { StringValue = expense.Name ?? null } });
                        break;
                    case SheetsConfig.HeaderNames.Description:
                        cells.Add(new CellData { UserEnteredValue = new ExtendedValue { StringValue = expense.Description ?? null } });
                        break;
                    case SheetsConfig.HeaderNames.Amount:
                        cells.Add(new CellData { UserEnteredValue = new ExtendedValue { NumberValue = (double)expense.Amount } });
                        break;
                    case SheetsConfig.HeaderNames.Category:
                        cells.Add(new CellData { UserEnteredValue = new ExtendedValue { StringValue = expense.Category ?? null } });
                        break;
                    default:
                        cells.Add(new CellData());
                        break;
                }
            }
            rowData.Values = cells;
            rows.Add(rowData);
        }

        return rows;
    }

    public static RowData MapToRowFormat(IList<object> headers)
    {
        var rowData = new RowData();
        var cells = new List<CellData>();
        foreach (var header in headers)
        {
            var headerName = header!.ToString()!.Trim();
            switch (headerName)
            {
                case SheetsConfig.HeaderNames.Date:
                    cells.Add(new CellData { UserEnteredFormat = SheetHelpers.GetCellFormat(FormatEnum.DATE) });
                    break;
                case SheetsConfig.HeaderNames.Amount:
                    cells.Add(new CellData { UserEnteredFormat = SheetHelpers.GetCellFormat(FormatEnum.ACCOUNTING) });
                    break;
                default:
                    cells.Add(new CellData());
                    break;
            }
        }
        rowData.Values = cells;

        return rowData;
    }

    public static SheetModel GetSheet()
    {
        // Use the new configuration helper for consistency and cleaner code
        return SheetConfigurationHelpers.ConfigureSheet(SheetsConfig.ExpenseSheet, (header, index) =>
        {
            var headerName = header!.Name;
            
            switch (headerName)
            {
                case SheetsConfig.HeaderNames.Date:
                    header.Note = ColumnNotes.DateFormat;
                    header.Format = FormatEnum.DATE;
                    break;
                case SheetsConfig.HeaderNames.Amount:
                    header.Format = FormatEnum.ACCOUNTING;
                    break;
                case SheetsConfig.HeaderNames.Category:
                    header.Validation = ValidationEnum.RANGE_SELF.GetDescription();
                    break;
                default:
                    // Apply common formatting patterns automatically
                    SheetConfigurationHelpers.ApplyCommonFormats(header, header.Name);
                    break;
            }
        });
    }
}