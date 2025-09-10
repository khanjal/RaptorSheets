using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Mappers;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Entities;
using RaptorSheets.Common.Mappers;
using RaptorSheets.Gig.Constants;

namespace RaptorSheets.Gig.Helpers;

/// <summary>
/// Helper methods for Google Sheets operations in the Gig domain.
/// 
/// HYBRID APPROACH:
/// Uses both constants and enums for optimal performance and maintainability:
/// - Switch statements use normalized strings for performance
/// - Enums provide type safety for API operations
/// - Constants ensure consistent string values throughout
/// </summary>
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
        return SheetsConfig.SheetUtilities.GetAllSheetNames();
    }

    public static List<SheetModel> GetMissingSheets(Spreadsheet spreadsheet)
    {
        var spreadsheetSheets = spreadsheet.Sheets.Select(x => x.Properties.Title).ToList();
        var sheetData = new List<SheetModel>();

        var sheetNames = GetSheetNames();

        // Loop through all sheets to see if they exist.
        foreach (var name in sheetNames)
        {
            if (spreadsheetSheets.Any(s => string.Equals(s, name, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            switch (name)
            {
                case SheetsConfig.SheetNames.Addresses:
                    sheetData.Add(AddressMapper.GetSheet());
                    break;
                case SheetsConfig.SheetNames.Daily:
                    sheetData.Add(DailyMapper.GetSheet());
                    break;
                case SheetsConfig.SheetNames.Expenses:
                    sheetData.Add(ExpenseMapper.GetSheet());
                    break;
                case SheetsConfig.SheetNames.Monthly:
                    sheetData.Add(MonthlyMapper.GetSheet());
                    break;
                case SheetsConfig.SheetNames.Names:
                    sheetData.Add(NameMapper.GetSheet());
                    break;
                case SheetsConfig.SheetNames.Places:
                    sheetData.Add(PlaceMapper.GetSheet());
                    break;
                case SheetsConfig.SheetNames.Regions:
                    sheetData.Add(RegionMapper.GetSheet());
                    break;
                case SheetsConfig.SheetNames.Setup:
                    sheetData.Add(SetupMapper.GetSheet());
                    break;
                case SheetsConfig.SheetNames.Services:
                    sheetData.Add(ServiceMapper.GetSheet());
                    break;
                case SheetsConfig.SheetNames.Shifts:
                    sheetData.Add(ShiftMapper.GetSheet());
                    break;
                case SheetsConfig.SheetNames.Trips:
                    sheetData.Add(TripMapper.GetSheet());
                    break;
                case SheetsConfig.SheetNames.Types:
                    sheetData.Add(TypeMapper.GetSheet());
                    break;
                case SheetsConfig.SheetNames.Weekdays:
                    sheetData.Add(WeekdayMapper.GetSheet());
                    break;
                case SheetsConfig.SheetNames.Weekly:
                    sheetData.Add(WeeklyMapper.GetSheet());
                    break;
                case SheetsConfig.SheetNames.Yearly:
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
                var values = new List<ConditionValue> { new() { UserEnteredValue = $"={GetSheetForRange(validation)}!A2:A" } };
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

    private static string? GetSheetForRange(ValidationEnum validationEnum)
    {
        return validationEnum switch
        {
            ValidationEnum.RANGE_ADDRESS => SheetsConfig.SheetNames.Addresses,
            ValidationEnum.RANGE_NAME => SheetsConfig.SheetNames.Names,
            ValidationEnum.RANGE_PLACE => SheetsConfig.SheetNames.Places,
            ValidationEnum.RANGE_REGION => SheetsConfig.SheetNames.Regions,
            ValidationEnum.RANGE_SERVICE => SheetsConfig.SheetNames.Services,
            ValidationEnum.RANGE_TYPE => SheetsConfig.SheetNames.Types,
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

        // Normalize sheet name for consistent comparison
        var normalizedSheetName = sheetName.ToUpperInvariant();

        switch (normalizedSheetName)
        {
            case SheetsConfig.SheetUtilities.UpperCase.Addresses:
                sheetEntity.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headerValues, AddressMapper.GetSheet()));
                sheetEntity.Addresses = AddressMapper.MapFromRangeData(values);
                break;
            case SheetsConfig.SheetUtilities.UpperCase.Daily:
                sheetEntity.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headerValues, DailyMapper.GetSheet()));
                sheetEntity.Daily = DailyMapper.MapFromRangeData(values);
                break;
            case SheetsConfig.SheetUtilities.UpperCase.Expenses:
                sheetEntity.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headerValues, ExpenseMapper.GetSheet()));
                sheetEntity.Expenses = ExpenseMapper.MapFromRangeData(values);
                break;
            case SheetsConfig.SheetUtilities.UpperCase.Monthly:
                sheetEntity.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headerValues, MonthlyMapper.GetSheet()));
                sheetEntity.Monthly = MonthlyMapper.MapFromRangeData(values);
                break;
            case SheetsConfig.SheetUtilities.UpperCase.Names:
                sheetEntity.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headerValues, NameMapper.GetSheet()));
                sheetEntity.Names = NameMapper.MapFromRangeData(values);
                break;
            case SheetsConfig.SheetUtilities.UpperCase.Places:
                sheetEntity.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headerValues, PlaceMapper.GetSheet()));
                sheetEntity.Places = PlaceMapper.MapFromRangeData(values);
                break;
            case SheetsConfig.SheetUtilities.UpperCase.Regions:
                sheetEntity.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headerValues, RegionMapper.GetSheet()));
                sheetEntity.Regions = RegionMapper.MapFromRangeData(values);
                break;
            case SheetsConfig.SheetUtilities.UpperCase.Services:
                sheetEntity.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headerValues, ServiceMapper.GetSheet()));
                sheetEntity.Services = ServiceMapper.MapFromRangeData(values);
                break;
            case SheetsConfig.SheetUtilities.UpperCase.Setup:
                sheetEntity.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headerValues, SetupMapper.GetSheet()));
                sheetEntity.Setup = SetupMapper.MapFromRangeData(values);
                break;
            case SheetsConfig.SheetUtilities.UpperCase.Shifts:
                sheetEntity.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headerValues, ShiftMapper.GetSheet()));
                sheetEntity.Shifts = ShiftMapper.MapFromRangeData(values);
                break;
            case SheetsConfig.SheetUtilities.UpperCase.Trips:
                sheetEntity.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headerValues, TripMapper.GetSheet()));
                sheetEntity.Trips = TripMapper.MapFromRangeData(values);
                break;
            case SheetsConfig.SheetUtilities.UpperCase.Types:
                sheetEntity.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headerValues, TypeMapper.GetSheet()));
                sheetEntity.Types = TypeMapper.MapFromRangeData(values);
                break;
            case SheetsConfig.SheetUtilities.UpperCase.Weekdays:
                sheetEntity.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headerValues, WeekdayMapper.GetSheet()));
                sheetEntity.Weekdays = WeekdayMapper.MapFromRangeData(values);
                break;
            case SheetsConfig.SheetUtilities.UpperCase.Weekly:
                sheetEntity.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headerValues, WeeklyMapper.GetSheet()));
                sheetEntity.Weekly = WeeklyMapper.MapFromRangeData(values);
                break;
            case SheetsConfig.SheetUtilities.UpperCase.Yearly:
                sheetEntity.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headerValues, YearlyMapper.GetSheet()));
                sheetEntity.Yearly = YearlyMapper.MapFromRangeData(values);
                break;
        }
    }
}