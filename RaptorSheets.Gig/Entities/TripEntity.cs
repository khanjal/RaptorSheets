using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Enums;
using RaptorSheets.Gig.Constants;

namespace RaptorSheets.Gig.Entities;

public class TripEntity
{
    public int RowId { get; set; }
    public string Action { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Date, FieldTypeEnum.String)]
    public string Date { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Service, FieldTypeEnum.String)]
    public string Service { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Number, FieldTypeEnum.Integer, jsonPropertyName: "number")]
    public int? Number { get; set; }

    [Column(SheetsConfig.HeaderNames.Exclude, FieldTypeEnum.Boolean, jsonPropertyName: "exclude")]
    public bool Exclude { get; set; } = false;

    [Column(SheetsConfig.HeaderNames.Type, FieldTypeEnum.String)]
    public string Type { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Place, FieldTypeEnum.String)]
    public string Place { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Pickup, FieldTypeEnum.String, jsonPropertyName: "pickupTime")]
    public string Pickup { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Dropoff, FieldTypeEnum.String, jsonPropertyName: "dropoffTime")]
    public string Dropoff { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Duration, FieldTypeEnum.String)]
    public string Duration { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Pay, FieldTypeEnum.Currency)]
    public decimal? Pay { get; set; }

    [Column(SheetsConfig.HeaderNames.Tips, FieldTypeEnum.Currency)]
    public decimal? Tip { get; set; }

    [Column(SheetsConfig.HeaderNames.Bonus, FieldTypeEnum.Currency)]
    public decimal? Bonus { get; set; }

    [Column(SheetsConfig.HeaderNames.Total, FieldTypeEnum.Currency)]
    public decimal? Total { get; set; }

    [Column(SheetsConfig.HeaderNames.Cash, FieldTypeEnum.Currency)]
    public decimal? Cash { get; set; }

    [Column(SheetsConfig.HeaderNames.OdometerStart, FieldTypeEnum.Number, "#,##0.0", jsonPropertyName: "startOdometer")]
    public decimal? OdometerStart { get; set; }

    [Column(SheetsConfig.HeaderNames.OdometerEnd, FieldTypeEnum.Number, "#,##0.0", jsonPropertyName: "endOdometer")]
    public decimal? OdometerEnd { get; set; }

    [Column(SheetsConfig.HeaderNames.Distance, FieldTypeEnum.Number, jsonPropertyName: "distance")]
    public decimal? Distance { get; set; }

    [Column(SheetsConfig.HeaderNames.Name, FieldTypeEnum.String)]
    public string Name { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.AddressStart, FieldTypeEnum.String, jsonPropertyName: "startAddress")]
    public string StartAddress { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.AddressEnd, FieldTypeEnum.String, jsonPropertyName: "endAddress")]
    public string EndAddress { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.UnitEnd, FieldTypeEnum.String, jsonPropertyName: "endUnit")]
    public string EndUnit { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.OrderNumber, FieldTypeEnum.String, jsonPropertyName: "orderNumber")]
    public string OrderNumber { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Region, FieldTypeEnum.String)]
    public string Region { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Note, FieldTypeEnum.String)]
    public string Note { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Key, FieldTypeEnum.String)]
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

    public bool Saved { get; set; }
}