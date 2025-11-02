using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Enums;

namespace RaptorSheets.Core.Tests.Data;

// Test constants similar to SheetsConfig.HeaderNames
public static class TestHeaderNames
{
    public const string Name = "Name";
    public const string Pay = "Pay";
    public const string Tips = "Tips";
    public const string Bonus = "Bonus";
    public const string Total = "Total";
    public const string Cash = "Cash";
    public const string Trips = "Trips";
    public const string FirstTrip = "First Trip";
    public const string LastTrip = "Last Trip";
    public const string Address = "Address";
    public const string Distance = "Distance";
    public const string Date = "Date";
    public const string Service = "Service";
}

// Base test entity - mark as input for testing
public class TestAmountEntity
{
    [Column(TestHeaderNames.Pay, FieldTypeEnum.Currency, isInput: true)]
    public decimal? Pay { get; set; }

    [Column(TestHeaderNames.Tips, FieldTypeEnum.Currency, isInput: true)]
    public decimal? Tips { get; set; }

    [Column(TestHeaderNames.Bonus, FieldTypeEnum.Currency, isInput: true)]
    public decimal? Bonus { get; set; }

    [Column(TestHeaderNames.Total, FieldTypeEnum.Currency, isInput: true)]
    public decimal? Total { get; set; }

    [Column(TestHeaderNames.Cash, FieldTypeEnum.Currency, isInput: true)]
    public decimal? Cash { get; set; }
}

// Middle test entity
public class TestVisitEntity : TestAmountEntity
{
    [Column(TestHeaderNames.Trips, FieldTypeEnum.Integer, isInput: true)]
    public int Trips { get; set; }

    [Column(TestHeaderNames.FirstTrip, FieldTypeEnum.String, isInput: true)]
    public string FirstTrip { get; set; } = "";

    [Column(TestHeaderNames.LastTrip, FieldTypeEnum.String, isInput: true)]
    public string LastTrip { get; set; } = "";
}

// Derived test entity
public class TestAddressEntity : TestVisitEntity
{
    public int RowId { get; set; } // No Column attribute - should be ignored

    [Column(TestHeaderNames.Address, FieldTypeEnum.String, isInput: true)]
    public string Address { get; set; } = "";

    [Column(TestHeaderNames.Distance, FieldTypeEnum.Number, isInput: true)]
    public decimal Distance { get; set; }

    public bool Saved { get; set; } // No Column attribute - should be ignored
}

// Simple entity without inheritance
public class TestSimpleEntity
{
    [Column(TestHeaderNames.Name, FieldTypeEnum.String, isInput: true)]
    public string Name { get; set; } = "";

    [Column(TestHeaderNames.Date, FieldTypeEnum.String, isInput: true)]
    public string Date { get; set; } = "";

    public int Id { get; set; } // No Column attribute - should be ignored
}

// Entity with invalid header reference (for error testing)
public class TestInvalidEntity
{
    [Column("Invalid Header Name", FieldTypeEnum.String, isInput: true)]
    public string InvalidProperty { get; set; } = "";

    [Column(TestHeaderNames.Name, FieldTypeEnum.String, isInput: true)]
    public string ValidProperty { get; set; } = "";
}

// Entity with no Column attributes
public class TestNoAttributesEntity
{
    public string Name { get; set; } = "";
    public int Value { get; set; }
}