using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Managers;
using RaptorSheets.Core.Mappers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Home.Constants;
using RaptorSheets.Home.Entities;
using RaptorSheets.Home.Enums;
using RaptorSheets.Home.Mappers;

namespace RaptorSheets.Home.Helpers;

public static class GenerateSheetsHelpers
{
    public static BatchUpdateSpreadsheetRequest Generate(List<string> sheets)
    {
        return SheetGenerationHelper.Generate(sheets, GetSheetModel, GetDataValidation);
    }

    public static List<string> GetSheetNames()
    {
        return SheetsConfig.SheetUtilities.GetAllSheetNames();
    }

    private static SheetModel GetSheetModel(string sheet)
    {
        return sheet switch
        {
            var s when string.Equals(s, SheetsConfig.SheetNames.Appliances, StringComparison.OrdinalIgnoreCase) => ApplianceMapper.GetSheet(),
            var s when string.Equals(s, SheetsConfig.SheetNames.Projects, StringComparison.OrdinalIgnoreCase) => GenericSheetMapper<ProjectEntity>.GetSheet(SheetsConfig.ProjectSheet),
            var s when string.Equals(s, SheetsConfig.SheetNames.Maintenance, StringComparison.OrdinalIgnoreCase) => GenericSheetMapper<MaintenanceEntity>.GetSheet(SheetsConfig.MaintenanceSheet),
            var s when string.Equals(s, SheetsConfig.SheetNames.Doors, StringComparison.OrdinalIgnoreCase) => GenericSheetMapper<DoorEntity>.GetSheet(SheetsConfig.DoorSheet),
            var s when string.Equals(s, SheetsConfig.SheetNames.Paints, StringComparison.OrdinalIgnoreCase) => GenericSheetMapper<PaintEntity>.GetSheet(SheetsConfig.PaintSheet),
            var s when string.Equals(s, SheetsConfig.SheetNames.Power, StringComparison.OrdinalIgnoreCase) => GenericSheetMapper<PowerEntity>.GetSheet(SheetsConfig.PowerSheet),
            var s when string.Equals(s, SheetsConfig.SheetNames.Rooms, StringComparison.OrdinalIgnoreCase) => RoomMapper.GetSheet(),
            var s when string.Equals(s, SheetsConfig.SheetNames.Contacts, StringComparison.OrdinalIgnoreCase) => GenericSheetMapper<ContactEntity>.GetSheet(SheetsConfig.ContactSheet),
            var s when string.Equals(s, SheetsConfig.SheetNames.Stats, StringComparison.OrdinalIgnoreCase) => GenericSheetMapper<StatEntity>.GetSheet(SheetsConfig.StatSheet),
            // DeleteSheets' temp-sheet safety mechanism asks for a bare AddSheet request for this
            // specific ad-hoc name - anything else unrecognized is a genuine caller error.
            var s when string.Equals(s, GoogleSheetManagerBase.TempSheetName, StringComparison.OrdinalIgnoreCase) => new SheetModel { Name = s },
            _ => throw new NotImplementedException($"Sheet model not found for: {sheet}"),
        };
    }

    private static DataValidationRule? GetDataValidation(SheetCellModel header)
    {
        var columnRange = $"{header.Column}2:{header.Column}";
        return HomeSheetHelpers.GetDataValidation(header.Validation.GetValueFromName<Validation>(), columnRange);
    }
}
