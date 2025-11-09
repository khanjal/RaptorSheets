using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Enums;
using RaptorSheets.Gig.Constants;

namespace RaptorSheets.Gig.Entities;

public class ShiftEntity : SheetRowEntityBase
{
    // Input columns (user-entered data)
    // Date is stored as string (for API flexibility/no timezone issues) but displayed as DATE in Google Sheets
    [Column(SheetsConfig.HeaderNames.Date, FieldTypeEnum.String, jsonPropertyName: "date", isInput: true, note: ColumnNotes.DateFormat, formatType: FormatEnum.DATE)]
    public string Date { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.TimeStart, FieldTypeEnum.Time, jsonPropertyName: "start", isInput: true)]
    public string Start { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.TimeEnd, FieldTypeEnum.Time, jsonPropertyName: "finish", isInput: true)]
    public string Finish { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Service, FieldTypeEnum.String, jsonPropertyName: "service", isInput: true, enableValidation: true, validationPattern: SheetsConfig.ValidationNames.RangeService)]
    public string Service { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Number, FieldTypeEnum.Integer, jsonPropertyName: "number", isInput: true, note: ColumnNotes.ShiftNumber)]
    public int? Number { get; set; }

    [Column(SheetsConfig.HeaderNames.TimeActive, FieldTypeEnum.Duration, jsonPropertyName: "active", isInput: true, note: ColumnNotes.ActiveTime)]
    public string Active { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.TimeTotal, FieldTypeEnum.Duration, jsonPropertyName: "time", isInput: true, note: ColumnNotes.TotalTime)]
    public string Time { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.TimeOmit, FieldTypeEnum.Boolean, jsonPropertyName: "omit", isInput: true, note: ColumnNotes.TimeOmit, enableValidation: true, validationPattern: SheetsConfig.ValidationNames.Boolean)]
    public bool? Omit { get; set; }

    [Column(SheetsConfig.HeaderNames.Trips, FieldTypeEnum.Integer, jsonPropertyName: "trips", isInput: true, note: ColumnNotes.ShiftTrips)]
    public int Trips { get; set; }

    // Financial properties
    [Column(SheetsConfig.HeaderNames.Pay, FieldTypeEnum.Currency, jsonPropertyName: "pay", isInput: true)]
    public decimal? Pay { get; set; }

    [Column(SheetsConfig.HeaderNames.Tips, FieldTypeEnum.Currency, jsonPropertyName: "tip", isInput: true)]
    public decimal? Tip { get; set; }

    [Column(SheetsConfig.HeaderNames.Bonus, FieldTypeEnum.Currency, jsonPropertyName: "bonus", isInput: true)]
    public decimal? Bonus { get; set; }

    [Column(SheetsConfig.HeaderNames.Cash, FieldTypeEnum.Currency, jsonPropertyName: "cash", isInput: true)]
    public decimal? Cash { get; set; }

    [Column(SheetsConfig.HeaderNames.OdometerStart, FieldTypeEnum.Number, formatPattern: "#,##0.0", jsonPropertyName: "startOdometer", isInput: true)]
    public decimal? OdometerStart { get; set; }

    [Column(SheetsConfig.HeaderNames.OdometerEnd, FieldTypeEnum.Number, formatPattern: "#,##0.0", jsonPropertyName: "endOdometer", isInput: true)]
    public decimal? OdometerEnd { get; set; }

    [Column(SheetsConfig.HeaderNames.Distance, FieldTypeEnum.Number, jsonPropertyName: "distance", isInput: true, note: ColumnNotes.ShiftDistance)]
    public decimal? Distance { get; set; }

    [Column(SheetsConfig.HeaderNames.Region, FieldTypeEnum.String, jsonPropertyName: "region", isInput: true, enableValidation: true, validationPattern: SheetsConfig.ValidationNames.RangeRegion)]
    public string Region { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Note, FieldTypeEnum.String, jsonPropertyName: "note", isInput: true)]
    public string Note { get; set; } = "";

    // Output columns (formulas/calculated) - defaults to isInput: false
    [Column(SheetsConfig.HeaderNames.Key, FieldTypeEnum.String, jsonPropertyName: "key", note: ColumnNotes.ShiftKey)]
    public string Key { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.TotalTimeActive, FieldTypeEnum.Duration, jsonPropertyName: "totalActive", note: ColumnNotes.TotalTimeActive)]
    public string TotalActive { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.TotalTime, FieldTypeEnum.Duration, jsonPropertyName: "totalTime")]
    public string TotalTime { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.TotalTrips, FieldTypeEnum.Integer, jsonPropertyName: "totalTrips", note: ColumnNotes.TotalTrips)]
    public int TotalTrips { get; set; }

    [Column(SheetsConfig.HeaderNames.TotalPay, FieldTypeEnum.Currency, jsonPropertyName: "totalPay")]
    public decimal? TotalPay { get; set; }

    [Column(SheetsConfig.HeaderNames.TotalTips, FieldTypeEnum.Currency, jsonPropertyName: "totalTips")]
    public decimal? TotalTips { get; set; }

    [Column(SheetsConfig.HeaderNames.TotalBonus, FieldTypeEnum.Currency, jsonPropertyName: "totalBonus")]
    public decimal? TotalBonus { get; set; }

    [Column(SheetsConfig.HeaderNames.TotalGrand, FieldTypeEnum.Currency, jsonPropertyName: "grandTotal")]
    public decimal? GrandTotal { get; set; }

    [Column(SheetsConfig.HeaderNames.TotalCash, FieldTypeEnum.Currency, jsonPropertyName: "totalCash")]
    public decimal? TotalCash { get; set; }

    [Column(SheetsConfig.HeaderNames.AmountPerTrip, FieldTypeEnum.Currency, jsonPropertyName: "amountPerTrip")]
    public decimal? AmountPerTrip { get; set; }

    [Column(SheetsConfig.HeaderNames.AmountPerTime, FieldTypeEnum.Currency, jsonPropertyName: "amountPerTime")]
    public decimal? AmountPerTime { get; set; }

    [Column(SheetsConfig.HeaderNames.TotalDistance, FieldTypeEnum.Number, jsonPropertyName: "totalDistance", note: ColumnNotes.TotalDistance)]
    public decimal? TotalDistance { get; set; }

    [Column(SheetsConfig.HeaderNames.AmountPerDistance, FieldTypeEnum.Currency, jsonPropertyName: "amountPerDistance")]
    public decimal? AmountPerDistance { get; set; }

    [Column(SheetsConfig.HeaderNames.TripsPerHour, FieldTypeEnum.Number)]
    public decimal? TripsPerHour { get; set; }

    [Column(SheetsConfig.HeaderNames.Day, FieldTypeEnum.String)]
    public string Day { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Month, FieldTypeEnum.String)]
    public string Month { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Year, FieldTypeEnum.String)]
    public string Year { get; set; } = "";
}
