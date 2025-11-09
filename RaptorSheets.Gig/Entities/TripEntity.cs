using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Enums;
using RaptorSheets.Gig.Constants;

namespace RaptorSheets.Gig.Entities;

public class TripEntity : SheetRowEntityBase
{
    // Input columns (user-entered data)
    // Date is stored as string (for API flexibility/no timezone issues) but displayed as DATE in Google Sheets
    [Column(SheetsConfig.HeaderNames.Date, FieldTypeEnum.String, isInput: true, note: ColumnNotes.DateFormat, formatType: FormatEnum.DATE)]
    public string Date { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Service, FieldTypeEnum.String, isInput: true, enableValidation: true, validationPattern: SheetsConfig.ValidationNames.RangeService)]
    public string Service { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Number, FieldTypeEnum.Integer, jsonPropertyName: "number", isInput: true, note: ColumnNotes.ShiftNumber)]
    public int? Number { get; set; }

    [Column(SheetsConfig.HeaderNames.Exclude, FieldTypeEnum.Boolean, jsonPropertyName: "exclude", isInput: true, note: ColumnNotes.Exclude, enableValidation: true, validationPattern: SheetsConfig.ValidationNames.Boolean)]
    public bool Exclude { get; set; } = false;

    [Column(SheetsConfig.HeaderNames.Type, FieldTypeEnum.String, isInput: true, note: ColumnNotes.Types, enableValidation: true, validationPattern: SheetsConfig.ValidationNames.RangeType)]
    public string Type { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Place, FieldTypeEnum.String, isInput: true, note: ColumnNotes.Place, enableValidation: true, validationPattern: SheetsConfig.ValidationNames.RangePlace)]
    public string Place { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Pickup, FieldTypeEnum.Time, jsonPropertyName: "pickupTime", isInput: true, note: ColumnNotes.Pickup)]
    public string Pickup { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Dropoff, FieldTypeEnum.Time, jsonPropertyName: "dropoffTime", isInput: true)]
    public string Dropoff { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Duration, FieldTypeEnum.Duration, isInput: true, note: ColumnNotes.Duration)]
    public string Duration { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Pay, FieldTypeEnum.Currency, isInput: true)]
    public decimal? Pay { get; set; }

    [Column(SheetsConfig.HeaderNames.Tips, FieldTypeEnum.Currency, isInput: true)]
    public decimal? Tip { get; set; }

    [Column(SheetsConfig.HeaderNames.Bonus, FieldTypeEnum.Currency, isInput: true)]
    public decimal? Bonus { get; set; }

    // Output column (formula: Pay + Tips + Bonus) - defaults to isInput: false
    [Column(SheetsConfig.HeaderNames.Total, FieldTypeEnum.Currency)]
    public decimal? Total { get; set; }

    [Column(SheetsConfig.HeaderNames.Cash, FieldTypeEnum.Currency, isInput: true)]
    public decimal? Cash { get; set; }

    [Column(SheetsConfig.HeaderNames.OdometerStart, FieldTypeEnum.Number, formatPattern: "#,##0.0", jsonPropertyName: "startOdometer", isInput: true)]
    public decimal? OdometerStart { get; set; }

    [Column(SheetsConfig.HeaderNames.OdometerEnd, FieldTypeEnum.Number, formatPattern: "#,##0.0", jsonPropertyName: "endOdometer", isInput: true)]
    public decimal? OdometerEnd { get; set; }

    [Column(SheetsConfig.HeaderNames.Distance, FieldTypeEnum.Number, jsonPropertyName: "distance", isInput: true, note: ColumnNotes.TripDistance)]
    public decimal? Distance { get; set; }

    [Column(SheetsConfig.HeaderNames.Name, FieldTypeEnum.String, isInput: true, enableValidation: true, validationPattern: SheetsConfig.ValidationNames.RangeName)]
    public string Name { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.AddressStart, FieldTypeEnum.String, jsonPropertyName: "startAddress", isInput: true, enableValidation: true, validationPattern: SheetsConfig.ValidationNames.RangeAddress)]
    public string StartAddress { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.AddressEnd, FieldTypeEnum.String, jsonPropertyName: "endAddress", isInput: true, enableValidation: true, validationPattern: SheetsConfig.ValidationNames.RangeAddress)]
    public string EndAddress { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.UnitEnd, FieldTypeEnum.String, jsonPropertyName: "endUnit", isInput: true, note: ColumnNotes.UnitTypes)]
    public string EndUnit { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.OrderNumber, FieldTypeEnum.String, jsonPropertyName: "orderNumber", isInput: true)]
    public string OrderNumber { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Region, FieldTypeEnum.String, isInput: true, enableValidation: true, validationPattern: SheetsConfig.ValidationNames.RangeRegion)]
    public string Region { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Note, FieldTypeEnum.String, isInput: true)]
    public string Note { get; set; } = "";

    // Output columns (formulas/calculated) - default to isInput: false
    [Column(SheetsConfig.HeaderNames.Key, FieldTypeEnum.String, note: ColumnNotes.TripKey)]
    public string Key { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Day, FieldTypeEnum.String)]
    public string Day { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Month, FieldTypeEnum.String)]
    public string Month { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Year, FieldTypeEnum.String)]
    public string Year { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.AmountPerTime, FieldTypeEnum.Currency, jsonPropertyName: "amountPerTime")]
    public decimal? AmountPerTime { get; set; }

    [Column(SheetsConfig.HeaderNames.AmountPerDistance, FieldTypeEnum.Currency, jsonPropertyName: "amountPerDistance")]
    public decimal? AmountPerDistance { get; set; }
}