using Google.Apis.Sheets.v4.Data;
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

    // Sheet name -> SheetModel factory (case-insensitive)
    private static readonly Dictionary<string, Func<SheetModel>> s_sheetFactories = new(StringComparer.OrdinalIgnoreCase)
    {
        { SheetsConfig.SheetNames.Addresses, AddressMapper.GetSheet },
        { SheetsConfig.SheetNames.Daily, DailyMapper.GetSheet },
        { SheetsConfig.SheetNames.Expenses, () => GenericSheetMapper<ExpenseEntity>.GetSheet(SheetsConfig.ExpenseSheet) },
        { SheetsConfig.SheetNames.Monthly, MonthlyMapper.GetSheet },
        { SheetsConfig.SheetNames.Names, NameMapper.GetSheet },
        { SheetsConfig.SheetNames.Places, PlaceMapper.GetSheet },
        { SheetsConfig.SheetNames.TripSummary, TripSummaryMapper.GetSheet },
        { SheetsConfig.SheetNames.PlaceSummary, PlaceSummaryMapper.GetSheet },
        { SheetsConfig.SheetNames.Regions, RegionMapper.GetSheet },
        { SheetsConfig.SheetNames.Setup, () => GenericSheetMapper<SetupEntity>.GetSheet(SheetsConfig.SetupSheet) },
        { SheetsConfig.SheetNames.Services, ServiceMapper.GetSheet },
        { SheetsConfig.SheetNames.Shifts, ShiftMapper.GetSheet },
        { SheetsConfig.SheetNames.Trips, TripMapper.GetSheet },
        { SheetsConfig.SheetNames.Types, TypeMapper.GetSheet },
        { SheetsConfig.SheetNames.Weekdays, WeekdayMapper.GetSheet },
        { SheetsConfig.SheetNames.Weekly, WeeklyMapper.GetSheet },
        { SheetsConfig.SheetNames.Yearly, YearlyMapper.GetSheet }
    };

    // Sheet name -> processor action (case-insensitive)
    private static readonly Dictionary<string, Action<SheetEntity, IList<IList<object>>>> s_sheetProcessors = new(StringComparer.OrdinalIgnoreCase)
    {
        { SheetsConfig.SheetNames.Addresses, (se, values) => {
                var headers = values[0];
                se.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headers, AddressMapper.GetSheet()));
                se.Addresses = GenericSheetMapper<AddressEntity>.MapFromRangeData(values);
            }
        },
        { SheetsConfig.SheetNames.Daily, (se, values) => {
                var headers = values[0];
                se.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headers, DailyMapper.GetSheet()));
                se.Daily = GenericSheetMapper<DailyEntity>.MapFromRangeData(values);
            }
        },
        { SheetsConfig.SheetNames.Expenses, (se, values) => {
                var headers = values[0];
                se.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headers, GenericSheetMapper<ExpenseEntity>.GetSheet(SheetsConfig.ExpenseSheet)));
                se.Expenses = GenericSheetMapper<ExpenseEntity>.MapFromRangeData(values);
            }
        },
        { SheetsConfig.SheetNames.Monthly, (se, values) => {
                var headers = values[0];
                se.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headers, MonthlyMapper.GetSheet()));
                se.Monthly = GenericSheetMapper<MonthlyEntity>.MapFromRangeData(values);
            }
        },
        { SheetsConfig.SheetNames.Names, (se, values) => {
                var headers = values[0];
                se.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headers, NameMapper.GetSheet()));
                se.Names = GenericSheetMapper<NameEntity>.MapFromRangeData(values);
            }
        },
        { SheetsConfig.SheetNames.Places, (se, values) => {
                var headers = values[0];
                se.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headers, PlaceMapper.GetSheet()));
                se.Places = GenericSheetMapper<PlaceEntity>.MapFromRangeData(values);
            }
        },
        { SheetsConfig.SheetNames.TripSummary, (se, values) => {
                var headers = values[0];
                se.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headers, TripSummaryMapper.GetSheet()));
                se.TripSummary = GenericSheetMapper<TripSummaryEntity>.MapFromRangeData(values);
            }
        },
        { SheetsConfig.SheetNames.PlaceSummary, (se, values) => {
                var headers = values[0];
                se.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headers, PlaceSummaryMapper.GetSheet()));
                se.PlaceSummary = GenericSheetMapper<PlaceSummaryEntity>.MapFromRangeData(values);
            }
        },
        { SheetsConfig.SheetNames.Regions, (se, values) => {
                var headers = values[0];
                se.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headers, RegionMapper.GetSheet()));
                se.Regions = GenericSheetMapper<RegionEntity>.MapFromRangeData(values);
            }
        },
        { SheetsConfig.SheetNames.Services, (se, values) => {
                var headers = values[0];
                se.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headers, ServiceMapper.GetSheet()));
                se.Services = GenericSheetMapper<ServiceEntity>.MapFromRangeData(values);
            }
        },
        { SheetsConfig.SheetNames.Setup, (se, values) => {
                var headers = values[0];
                se.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headers, GenericSheetMapper<SetupEntity>.GetSheet(SheetsConfig.SetupSheet)));
                se.Setup = GenericSheetMapper<SetupEntity>.MapFromRangeData(values);
            }
        },
        { SheetsConfig.SheetNames.Shifts, (se, values) => {
                var headers = values[0];
                se.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headers, ShiftMapper.GetSheet()));
                se.Shifts = GenericSheetMapper<ShiftEntity>.MapFromRangeData(values);
            }
        },
        { SheetsConfig.SheetNames.Trips, (se, values) => {
                var headers = values[0];
                se.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headers, TripMapper.GetSheet()));
                se.Trips = GenericSheetMapper<TripEntity>.MapFromRangeData(values);
            }
        },
        { SheetsConfig.SheetNames.Types, (se, values) => {
                var headers = values[0];
                se.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headers, TypeMapper.GetSheet()));
                se.Types = GenericSheetMapper<TypeEntity>.MapFromRangeData(values);
            }
        },
        { SheetsConfig.SheetNames.Weekdays, (se, values) => {
                var headers = values[0];
                se.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headers, WeekdayMapper.GetSheet()));
                se.Weekdays = GenericSheetMapper<WeekdayEntity>.MapFromRangeData(values);
            }
        },
        { SheetsConfig.SheetNames.Weekly, (se, values) => {
                var headers = values[0];
                se.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headers, WeeklyMapper.GetSheet()));
                se.Weekly = GenericSheetMapper<WeeklyEntity>.MapFromRangeData(values);
            }
        },
        { SheetsConfig.SheetNames.Yearly, (se, values) => {
                var headers = values[0];
                se.Messages.AddRange(HeaderHelpers.CheckSheetHeaders(headers, YearlyMapper.GetSheet()));
                se.Yearly = GenericSheetMapper<YearlyEntity>.MapFromRangeData(values);
            }
        }
    };

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

            if (s_sheetFactories.TryGetValue(name, out var factory))
            {
                sheetData.Add(factory());
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

        if (s_sheetProcessors.TryGetValue(sheetName, out var processor))
        {
            processor(sheetEntity, values);
        }
    }
}