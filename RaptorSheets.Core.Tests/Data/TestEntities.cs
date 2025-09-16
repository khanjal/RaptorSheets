using RaptorSheets.Core.Attributes;

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

// Base test entity
public class TestAmountEntity
{
    [ColumnOrder(TestHeaderNames.Pay)]
    public decimal? Pay { get; set; }

    [ColumnOrder(TestHeaderNames.Tips)]
    public decimal? Tips { get; set; }

    [ColumnOrder(TestHeaderNames.Bonus)]
    public decimal? Bonus { get; set; }

    [ColumnOrder(TestHeaderNames.Total)]
    public decimal? Total { get; set; }

    [ColumnOrder(TestHeaderNames.Cash)]
    public decimal? Cash { get; set; }
}

// Middle test entity
public class TestVisitEntity : TestAmountEntity
{
    [ColumnOrder(TestHeaderNames.Trips)]
    public int Trips { get; set; }

    [ColumnOrder(TestHeaderNames.FirstTrip)]
    public string FirstTrip { get; set; } = "";

    [ColumnOrder(TestHeaderNames.LastTrip)]
    public string LastTrip { get; set; } = "";
}

// Derived test entity
public class TestAddressEntity : TestVisitEntity
{
    public int RowId { get; set; } // No ColumnOrder - should be ignored

    [ColumnOrder(TestHeaderNames.Address)]
    public string Address { get; set; } = "";

    [ColumnOrder(TestHeaderNames.Distance)]
    public decimal Distance { get; set; }

    public bool Saved { get; set; } // No ColumnOrder - should be ignored
}

// Simple entity without inheritance
public class TestSimpleEntity
{
    [ColumnOrder(TestHeaderNames.Name)]
    public string Name { get; set; } = "";

    [ColumnOrder(TestHeaderNames.Date)]
    public string Date { get; set; } = "";

    public int Id { get; set; } // No ColumnOrder - should be ignored
}

// Entity with invalid header reference (for error testing)
public class TestInvalidEntity
{
    [ColumnOrder("Invalid Header Name")]
    public string InvalidProperty { get; set; } = "";

    [ColumnOrder(TestHeaderNames.Name)]
    public string ValidProperty { get; set; } = "";
}

// Entity with no ColumnOrder attributes
public class TestNoAttributesEntity
{
    public string Name { get; set; } = "";
    public int Value { get; set; }
}