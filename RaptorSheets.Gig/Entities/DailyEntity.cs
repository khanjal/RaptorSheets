using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Enums;
using RaptorSheets.Gig.Constants;

namespace RaptorSheets.Gig.Entities;

public class DailyEntity : SheetRowEntityBase
{
    // Date is stored as string (for API flexibility/no timezone issues) but displayed as DATE in Google Sheets
    [Column(SheetsConfig.HeaderNames.Date, FieldTypeEnum.String, jsonPropertyName: "date", formatType: FormatEnum.DATE)]
    public string Date { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Trips, FieldTypeEnum.Integer, jsonPropertyName: "trips")]
    public int Trips { get; set; }

    // Financial properties
    [Column(SheetsConfig.HeaderNames.Pay, FieldTypeEnum.Currency, jsonPropertyName: "pay")]
    public decimal? Pay { get; set; }

    [Column(SheetsConfig.HeaderNames.Tips, FieldTypeEnum.Currency, jsonPropertyName: "tip")]
    public decimal? Tip { get; set; }

    [Column(SheetsConfig.HeaderNames.Bonus, FieldTypeEnum.Currency, jsonPropertyName: "bonus")]
    public decimal? Bonus { get; set; }

    [Column(SheetsConfig.HeaderNames.Total, FieldTypeEnum.Currency, jsonPropertyName: "total")]
    public decimal? Total { get; set; }

    [Column(SheetsConfig.HeaderNames.Cash, FieldTypeEnum.Currency, jsonPropertyName: "cash")]
    public decimal? Cash { get; set; }

    [Column(SheetsConfig.HeaderNames.AmountPerTrip, FieldTypeEnum.Currency, jsonPropertyName: "amt/trip")]
    public decimal AmountPerTrip { get; set; }

    [Column(SheetsConfig.HeaderNames.Distance, FieldTypeEnum.Number, jsonPropertyName: "distance")]
    public decimal Distance { get; set; }

    [Column(SheetsConfig.HeaderNames.AmountPerDistance, FieldTypeEnum.Currency, jsonPropertyName: "amt/dist")]
    public decimal AmountPerDistance { get; set; }

    [Column(SheetsConfig.HeaderNames.TimeTotal, FieldTypeEnum.Duration, jsonPropertyName: "time")]
    public string Time { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.AmountPerTime, FieldTypeEnum.Currency, jsonPropertyName: "amt/hour")]
    public decimal AmountPerTime { get; set; }

    [Column(SheetsConfig.HeaderNames.Day, FieldTypeEnum.String, jsonPropertyName: "day")]
    public string Day { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Weekday, FieldTypeEnum.String)]
    public string Weekday { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Week, FieldTypeEnum.String, jsonPropertyName: "week")]
    public string Week { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Month, FieldTypeEnum.String, jsonPropertyName: "month")]
    public string Month { get; set; } = "";
}
