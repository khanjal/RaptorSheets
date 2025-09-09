using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Enums;
using System.Xml;
using HeaderEnum = RaptorSheets.Gig.Enums.HeaderEnum;

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
                Date = DateTime.TryParse(HeaderHelpers.GetStringValue(HeaderEnum.DATE.GetDescription(), value, headers), out var date) ? date : DateTime.MinValue,
                Name = HeaderHelpers.GetStringValue(HeaderEnum.NAME.GetDescription(), value, headers),
                Description = HeaderHelpers.GetStringValue(HeaderEnum.DESCRIPTION.GetDescription(), value, headers),
                Amount = HeaderHelpers.GetDecimalValue(HeaderEnum.AMOUNT.GetDescription(), value, headers),
                Category = HeaderHelpers.GetStringValue(HeaderEnum.CATEGORY.GetDescription(), value, headers)
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
                var headerEnum = header.ToString()!.Trim().GetValueFromName<HeaderEnum>();

                switch (headerEnum)
                {
                    case HeaderEnum.DATE:
                        objectList.Add(expense.Date.ToString("yyyy-MM-dd"));
                        break;
                    case HeaderEnum.NAME:
                        objectList.Add(expense.Name);
                        break;
                    case HeaderEnum.DESCRIPTION:
                        objectList.Add(expense.Description);
                        break;
                    case HeaderEnum.AMOUNT:
                        objectList.Add(expense.Amount.ToString());
                        break;
                    case HeaderEnum.CATEGORY:
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
                var headerEnum = header!.ToString()!.Trim().GetValueFromName<HeaderEnum>();
                switch (headerEnum)
                {
                    case HeaderEnum.DATE:
                        cells.Add(new CellData { UserEnteredValue = new ExtendedValue { NumberValue = expense.Date.ToString("yyyy-MM-dd").ToSerialDate() } });
                        break;
                    case HeaderEnum.NAME:
                        cells.Add(new CellData { UserEnteredValue = new ExtendedValue { StringValue = expense.Name ?? null } });
                        break;
                    case HeaderEnum.DESCRIPTION:
                        cells.Add(new CellData { UserEnteredValue = new ExtendedValue { StringValue = expense.Description ?? null } });
                        break;
                    case HeaderEnum.AMOUNT:
                        cells.Add(new CellData { UserEnteredValue = new ExtendedValue { NumberValue = (double)expense.Amount } });
                        break;
                    case HeaderEnum.CATEGORY:
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
            var headerEnum = header!.ToString()!.Trim().GetValueFromName<HeaderEnum>();
            switch (headerEnum)
            {
                case HeaderEnum.DATE:
                    cells.Add(new CellData { UserEnteredFormat = SheetHelpers.GetCellFormat(FormatEnum.DATE) });
                    break;
                case HeaderEnum.AMOUNT:
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
            var headerEnum = header!.Name.ToString()!.Trim().GetValueFromName<HeaderEnum>();
            
            switch (headerEnum)
            {
                case HeaderEnum.DATE:
                    header.Note = ColumnNotes.DateFormat;
                    header.Format = FormatEnum.DATE;
                    break;
                case HeaderEnum.AMOUNT:
                    header.Format = FormatEnum.ACCOUNTING;
                    break;
                case HeaderEnum.CATEGORY:
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