using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Common.Constants.SheetConfigs;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Mappers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Mappers;

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
                case var s when string.Equals(s, SheetsConfig.SheetNames.Addresses, StringComparison.OrdinalIgnoreCase):
                    sheetData.Add(AddressMapper.GetSheet());
                    break;
                case var s when string.Equals(s, SheetsConfig.SheetNames.Daily, StringComparison.OrdinalIgnoreCase):
                    sheetData.Add(DailyMapper.GetSheet());
                    break;
                case var s when string.Equals(s, SheetsConfig.SheetNames.Expenses, StringComparison.OrdinalIgnoreCase):
                    sheetData.Add(GenericSheetMapper<ExpenseEntity>.GetSheet(SheetsConfig.ExpenseSheet));
                    break;
                case var s when string.Equals(s, SheetsConfig.SheetNames.Monthly, StringComparison.OrdinalIgnoreCase):
                    sheetData.Add(MonthlyMapper.GetSheet());
                    break;
                case var s when string.Equals(s, SheetsConfig.SheetNames.Names, StringComparison.OrdinalIgnoreCase):
                    sheetData.Add(NameMapper.GetSheet());
                    break;
                case var s when string.Equals(s, SheetsConfig.SheetNames.Places, StringComparison.OrdinalIgnoreCase):
                    sheetData.Add(PlaceMapper.GetSheet());
                    break;
                case var s when string.Equals(s, SheetsConfig.SheetNames.Regions, StringComparison.OrdinalIgnoreCase):
                    sheetData.Add(RegionMapper.GetSheet());
                    break;
                case var s when string.Equals(s, SheetsConfig.SheetNames.Setup, StringComparison.OrdinalIgnoreCase):
                    sheetData.Add(SetupSheetConfig.SetupSheet);
                    break;
                case var s when string.Equals(s, SheetsConfig.SheetNames.Services, StringComparison.OrdinalIgnoreCase):
                    sheetData.Add(ServiceMapper.GetSheet());
                    break;
                case var s when string.Equals(s, SheetsConfig.SheetNames.Shifts, StringComparison.OrdinalIgnoreCase):
                    sheetData.Add(ShiftMapper.GetSheet());
                    break;
                case var s when string.Equals(s, SheetsConfig.SheetNames.Trips, StringComparison.OrdinalIgnoreCase):
                    sheetData.Add(TripMapper.GetSheet());
                    break;
                case var s when string.Equals(s, SheetsConfig.SheetNames.Types, StringComparison.OrdinalIgnoreCase):
                    sheetData.Add(TypeMapper.GetSheet());
                    break;
                case var s when string.Equals(s, SheetsConfig.SheetNames.Weekdays, StringComparison.OrdinalIgnoreCase):
                    sheetData.Add(WeekdayMapper.GetSheet());
                    break;
                case var s when string.Equals(s, SheetsConfig.SheetNames.Weekly, StringComparison.OrdinalIgnoreCase):
                    sheetData.Add(WeeklyMapper.GetSheet());
                    break;
                case var s when string.Equals(s, SheetsConfig.SheetNames.Yearly, StringComparison.OrdinalIgnoreCase):
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

        switch (sheetName)
        {
            case var s when string.Equals(s, SheetsConfig.SheetNames.Addresses, StringComparison.OrdinalIgnoreCase):
                sheetEntity.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headerValues, AddressMapper.GetSheet()));
                sheetEntity.Addresses = GenericSheetMapper<AddressEntity>.MapFromRangeData(values);
                break;
            case var s when string.Equals(s, SheetsConfig.SheetNames.Daily, StringComparison.OrdinalIgnoreCase):
                sheetEntity.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headerValues, DailyMapper.GetSheet()));
                sheetEntity.Daily = GenericSheetMapper<DailyEntity>.MapFromRangeData(values);
                break;
            case var s when string.Equals(s, SheetsConfig.SheetNames.Expenses, StringComparison.OrdinalIgnoreCase):
                sheetEntity.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headerValues, GenericSheetMapper<ExpenseEntity>.GetSheet(SheetsConfig.ExpenseSheet)));
                sheetEntity.Expenses = GenericSheetMapper<ExpenseEntity>.MapFromRangeData(values);
                break;
            case var s when string.Equals(s, SheetsConfig.SheetNames.Monthly, StringComparison.OrdinalIgnoreCase):
                sheetEntity.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headerValues, MonthlyMapper.GetSheet()));
                sheetEntity.Monthly = GenericSheetMapper<MonthlyEntity>.MapFromRangeData(values);
                break;
            case var s when string.Equals(s, SheetsConfig.SheetNames.Names, StringComparison.OrdinalIgnoreCase):
                sheetEntity.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headerValues, NameMapper.GetSheet()));
                sheetEntity.Names = GenericSheetMapper<NameEntity>.MapFromRangeData(values);
                break;
            case var s when string.Equals(s, SheetsConfig.SheetNames.Places, StringComparison.OrdinalIgnoreCase):
                sheetEntity.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headerValues, PlaceMapper.GetSheet()));
                sheetEntity.Places = GenericSheetMapper<PlaceEntity>.MapFromRangeData(values);
                break;
            case var s when string.Equals(s, SheetsConfig.SheetNames.Regions, StringComparison.OrdinalIgnoreCase):
                sheetEntity.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headerValues, RegionMapper.GetSheet()));
                sheetEntity.Regions = GenericSheetMapper<RegionEntity>.MapFromRangeData(values);
                break;
            case var s when string.Equals(s, SheetsConfig.SheetNames.Services, StringComparison.OrdinalIgnoreCase):
                sheetEntity.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headerValues, ServiceMapper.GetSheet()));
                sheetEntity.Services = GenericSheetMapper<ServiceEntity>.MapFromRangeData(values);
                break;
            case var s when string.Equals(s, SheetsConfig.SheetNames.Setup, StringComparison.OrdinalIgnoreCase):
                sheetEntity.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headerValues, SetupSheetConfig.SetupSheet));
                sheetEntity.Setup = GenericSheetMapper<SetupEntity>.MapFromRangeData(values);
                break;
            case var s when string.Equals(s, SheetsConfig.SheetNames.Shifts, StringComparison.OrdinalIgnoreCase):
                sheetEntity.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headerValues, ShiftMapper.GetSheet()));
                sheetEntity.Shifts = GenericSheetMapper<ShiftEntity>.MapFromRangeData(values);
                break;
            case var s when string.Equals(s, SheetsConfig.SheetNames.Trips, StringComparison.OrdinalIgnoreCase):
                sheetEntity.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headerValues, TripMapper.GetSheet()));
                sheetEntity.Trips = GenericSheetMapper<TripEntity>.MapFromRangeData(values);
                break;
            case var s when string.Equals(s, SheetsConfig.SheetNames.Types, StringComparison.OrdinalIgnoreCase):
                sheetEntity.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headerValues, TypeMapper.GetSheet()));
                sheetEntity.Types = GenericSheetMapper<TypeEntity>.MapFromRangeData(values);
                break;
            case var s when string.Equals(s, SheetsConfig.SheetNames.Weekdays, StringComparison.OrdinalIgnoreCase):
                sheetEntity.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headerValues, WeekdayMapper.GetSheet()));
                sheetEntity.Weekdays = GenericSheetMapper<WeekdayEntity>.MapFromRangeData(values);
                break;
            case var s when string.Equals(s, SheetsConfig.SheetNames.Weekly, StringComparison.OrdinalIgnoreCase):
                sheetEntity.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headerValues, WeeklyMapper.GetSheet()));
                sheetEntity.Weekly = GenericSheetMapper<WeeklyEntity>.MapFromRangeData(values);
                break;
            case var s when string.Equals(s, SheetsConfig.SheetNames.Yearly, StringComparison.OrdinalIgnoreCase):
                sheetEntity.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headerValues, YearlyMapper.GetSheet()));
                sheetEntity.Yearly = GenericSheetMapper<YearlyEntity>.MapFromRangeData(values);
                break;
        }
    }
}