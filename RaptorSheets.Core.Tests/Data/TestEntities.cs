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
    [SheetOrder(TestHeaderNames.Pay)]
    public decimal? Pay { get; set; }

    [SheetOrder(TestHeaderNames.Tips)]
    public decimal? Tips { get; set; }

    [SheetOrder(TestHeaderNames.Bonus)]
    public decimal? Bonus { get; set; }

    [SheetOrder(TestHeaderNames.Total)]
    public decimal? Total { get; set; }

    [SheetOrder(TestHeaderNames.Cash)]
    public decimal? Cash { get; set; }
}

// Middle test entity
public class TestVisitEntity : TestAmountEntity
{
    [SheetOrder(TestHeaderNames.Trips)]
    public int Trips { get; set; }

    [SheetOrder(TestHeaderNames.FirstTrip)]
    public string FirstTrip { get; set; } = "";

    [SheetOrder(TestHeaderNames.LastTrip)]
    public string LastTrip { get; set; } = "";
}

// Derived test entity
public class TestAddressEntity : TestVisitEntity
{
    public int RowId { get; set; } // No SheetOrder - should be ignored

    [SheetOrder(TestHeaderNames.Address)]
    public string Address { get; set; } = "";

    [SheetOrder(TestHeaderNames.Distance)]
    public decimal Distance { get; set; }

    public bool Saved { get; set; } // No SheetOrder - should be ignored
}

// Simple entity without inheritance
public class TestSimpleEntity
{
    [SheetOrder(TestHeaderNames.Name)]
    public string Name { get; set; } = "";

    [SheetOrder(TestHeaderNames.Date)]
    public string Date { get; set; } = "";

    public int Id { get; set; } // No SheetOrder - should be ignored
}

// Entity with invalid header reference (for error testing)
public class TestInvalidEntity
{
    [SheetOrder("Invalid Header Name")]
    public string InvalidProperty { get; set; } = "";

    [SheetOrder(TestHeaderNames.Name)]
    public string ValidProperty { get; set; } = "";
}

// Entity with no SheetOrder attributes
public class TestNoAttributesEntity
{
    public string Name { get; set; } = "";
    public string Address { get; set; } = "";
    public int Id { get; set; }
}