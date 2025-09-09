using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Mappers;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Entities;
using SheetEnum = RaptorSheets.Gig.Enums.SheetEnum;
using RaptorSheets.Common.Mappers;
using RaptorSheets.Gig.Constants;

namespace RaptorSheets.Gig.Helpers;

public static class GigSheetHelpers
{
    public static List<SheetModel> GetSheets()
    {
        var sheets = new List<SheetModel>
        {
            ShiftMapper.GetSheet(),
            TripMapper.GetSheet()
        };

        return sheets;
    }

    public static List<string> GetSheetNames()
    {
        var sheetNames = Enum.GetNames<SheetEnum>().ToList();
        sheetNames.AddRange(Enum.GetNames<Common.Enums.SheetEnum>());

        return sheetNames;
    }

    public static List<SheetModel> GetMissingSheets(Spreadsheet spreadsheet)
    {
        var spreadsheetSheets = spreadsheet.Sheets.Select(x => x.Properties.Title.ToUpper()).ToList();
        var sheetData = new List<SheetModel>();

        var sheetNames = GetSheetNames();

        // Loop through all sheets to see if they exist.
        foreach (var name in sheetNames)
        {
            if (spreadsheetSheets.Contains(name))
            {
                continue;
            }

            switch (name.ToUpper())
            {
                case SheetsConfig.SheetNames.ADDRESSES:
                    sheetData.Add(AddressMapper.GetSheet());
                    break;
                case SheetsConfig.SheetNames.DAILY:
                    sheetData.Add(DailyMapper.GetSheet());
                    break;
                case SheetsConfig.SheetNames.EXPENSES:
                    sheetData.Add(ExpenseMapper.GetSheet());
                    break;
                case SheetsConfig.SheetNames.MONTHLY:
                    sheetData.Add(MonthlyMapper.GetSheet());
                    break;
                case SheetsConfig.SheetNames.NAMES:
                    sheetData.Add(NameMapper.GetSheet());
                    break;
                case SheetsConfig.SheetNames.PLACES:
                    sheetData.Add(PlaceMapper.GetSheet());
                    break;
                case SheetsConfig.SheetNames.REGIONS:
                    sheetData.Add(RegionMapper.GetSheet());
                    break;
                case SheetsConfig.SheetNames.SETUP:
                    sheetData.Add(SetupMapper.GetSheet());
                    break;
                case SheetsConfig.SheetNames.SERVICES:
                    sheetData.Add(ServiceMapper.GetSheet());
                    break;
                case SheetsConfig.SheetNames.SHIFTS:
                    sheetData.Add(ShiftMapper.GetSheet());
                    break;
                case SheetsConfig.SheetNames.TRIPS:
                    sheetData.Add(TripMapper.GetSheet());
                    break;
                case SheetsConfig.SheetNames.TYPES:
                    sheetData.Add(TypeMapper.GetSheet());
                    break;
                case SheetsConfig.SheetNames.WEEKDAYS:
                    sheetData.Add(WeekdayMapper.GetSheet());
                    break;
                case SheetsConfig.SheetNames.WEEKLY:
                    sheetData.Add(WeeklyMapper.GetSheet());
                    break;
                case SheetsConfig.SheetNames.YEARLY:
                    sheetData.Add(YearlyMapper.GetSheet());
                    break;
            }
        }

        return sheetData;
    }

    public static DataValidationRule GetDataValidation(ValidationEnum validation, string? range = "")
    {
        var dataValidation = new DataValidationRule();

        switch (validation)
        {
            case ValidationEnum.BOOLEAN:
                dataValidation.Condition = new BooleanCondition { Type = "BOOLEAN" };
                break;
            case ValidationEnum.RANGE_ADDRESS:
            case ValidationEnum.RANGE_NAME:
            case ValidationEnum.RANGE_PLACE:
            case ValidationEnum.RANGE_REGION:
            case ValidationEnum.RANGE_SERVICE:
            case ValidationEnum.RANGE_TYPE:
                var values = new List<ConditionValue> { new() { UserEnteredValue = $"={GetSheetForRange(validation)?.GetDescription()}!A2:A" } };
                dataValidation.Condition = new BooleanCondition { Type = "ONE_OF_RANGE", Values = values };
                dataValidation.ShowCustomUi = true;
                dataValidation.Strict = false;
                break;
            case ValidationEnum.RANGE_SELF:
                var selfValues = new List<ConditionValue> { new() { UserEnteredValue = $"={range}" } };
                dataValidation.Condition = new BooleanCondition { Type = "ONE_OF_RANGE", Values = selfValues };
                dataValidation.ShowCustomUi = true;
                dataValidation.Strict = false;
                break;
        }

        return dataValidation;
    }

    private static SheetEnum? GetSheetForRange(ValidationEnum validationEnum)
    {
        return validationEnum switch
        {
            ValidationEnum.RANGE_ADDRESS => SheetEnum.ADDRESSES,
            ValidationEnum.RANGE_NAME => SheetEnum.NAMES,
            ValidationEnum.RANGE_PLACE => SheetEnum.PLACES,
            ValidationEnum.RANGE_REGION => SheetEnum.REGIONS,
            ValidationEnum.RANGE_SERVICE => SheetEnum.SERVICES,
            ValidationEnum.RANGE_TYPE => SheetEnum.TYPES,
            _ => null
        };
    }

    public static SheetEntity? MapData(Spreadsheet spreadsheet)
    {
        var sheetEntity = new SheetEntity
        {
            Properties = new PropertyEntity
            {
                Name = spreadsheet.Properties.Title,
            }
        };

        var sheetValues = SheetHelpers.GetSheetValues(spreadsheet);
        foreach (var sheet in spreadsheet.Sheets)
        {
            var values = sheetValues[sheet.Properties.Title];
            ProcessSheetData(sheetEntity, sheet.Properties.Title, values);
        }

        return sheetEntity;
    }

    public static SheetEntity? MapData(BatchGetValuesByDataFilterResponse response)
    {
        if (response.ValueRanges == null)
        {
            return null;
        }

        var sheetEntity = new SheetEntity();

        foreach (var matchedValue in response.ValueRanges)
        {
            var sheetRange = matchedValue.DataFilters[0].A1Range;
            var values = matchedValue.ValueRange.Values;
            ProcessSheetData(sheetEntity, sheetRange, values);
        }

        return sheetEntity;
    }

    private static void ProcessSheetData(SheetEntity sheetEntity, string sheetName, IList<IList<object>> values)
    {
        if (values == null || values.Count == 0)
        {
            return;
        }

        var headerValues = values.First();

        switch (sheetName.ToUpper())
        {
            case SheetsConfig.SheetNames.ADDRESSES:
                sheetEntity.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headerValues, AddressMapper.GetSheet()));
                sheetEntity.Addresses = AddressMapper.MapFromRangeData(values);
                break;
            case SheetsConfig.SheetNames.DAILY:
                sheetEntity.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headerValues, DailyMapper.GetSheet()));
                sheetEntity.Daily = DailyMapper.MapFromRangeData(values);
                break;
            case SheetsConfig.SheetNames.EXPENSES:
                sheetEntity.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headerValues, ExpenseMapper.GetSheet()));
                sheetEntity.Expenses = ExpenseMapper.MapFromRangeData(values);
                break;
            case SheetsConfig.SheetNames.MONTHLY:
                sheetEntity.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headerValues, MonthlyMapper.GetSheet()));
                sheetEntity.Monthly = MonthlyMapper.MapFromRangeData(values);
                break;
            case SheetsConfig.SheetNames.NAMES:
                sheetEntity.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headerValues, NameMapper.GetSheet()));
                sheetEntity.Names = NameMapper.MapFromRangeData(values);
                break;
            case SheetsConfig.SheetNames.PLACES:
                sheetEntity.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headerValues, PlaceMapper.GetSheet()));
                sheetEntity.Places = PlaceMapper.MapFromRangeData(values);
                break;
            case SheetsConfig.SheetNames.REGIONS:
                sheetEntity.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headerValues, RegionMapper.GetSheet()));
                sheetEntity.Regions = RegionMapper.MapFromRangeData(values);
                break;
            case SheetsConfig.SheetNames.SERVICES:
                sheetEntity.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headerValues, ServiceMapper.GetSheet()));
                sheetEntity.Services = ServiceMapper.MapFromRangeData(values);
                break;
            case SheetsConfig.SheetNames.SETUP:
                sheetEntity.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headerValues, SetupMapper.GetSheet()));
                sheetEntity.Setup = SetupMapper.MapFromRangeData(values);
                break;
            case SheetsConfig.SheetNames.SHIFTS:
                sheetEntity.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headerValues, ShiftMapper.GetSheet()));
                sheetEntity.Shifts = ShiftMapper.MapFromRangeData(values);
                break;
            case SheetsConfig.SheetNames.TRIPS:
                sheetEntity.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headerValues, TripMapper.GetSheet()));
                sheetEntity.Trips = TripMapper.MapFromRangeData(values);
                break;
            case SheetsConfig.SheetNames.TYPES:
                sheetEntity.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headerValues, TypeMapper.GetSheet()));
                sheetEntity.Types = TypeMapper.MapFromRangeData(values);
                break;
            case SheetsConfig.SheetNames.WEEKDAYS:
                sheetEntity.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headerValues, WeekdayMapper.GetSheet()));
                sheetEntity.Weekdays = WeekdayMapper.MapFromRangeData(values);
                break;
            case SheetsConfig.SheetNames.WEEKLY:
                sheetEntity.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headerValues, WeeklyMapper.GetSheet()));
                sheetEntity.Weekly = WeeklyMapper.MapFromRangeData(values);
                break;
            case SheetsConfig.SheetNames.YEARLY:
                sheetEntity.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headerValues, YearlyMapper.GetSheet()));
                sheetEntity.Yearly = YearlyMapper.MapFromRangeData(values);
                break;
        }
    }
}