using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Enums;
using System.Diagnostics.CodeAnalysis;

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
[ExcludeFromCodeCoverage]
public class TestAmountEntity
{
    [Header(TestHeaderNames.Pay)]
    [Input(true)]
    [Format(FormatEnum.CURRENCY)]
    public decimal? Pay { get; set; }

    [Header(TestHeaderNames.Tips)]
    [Input(true)]
    [Format(FormatEnum.CURRENCY)]
    public decimal? Tips { get; set; }

    [Header(TestHeaderNames.Bonus)]
    [Input(true)]
    [Format(FormatEnum.CURRENCY)]
    public decimal? Bonus { get; set; }

    [Header(TestHeaderNames.Total)]
    [Input(true)]
    [Format(FormatEnum.CURRENCY)]
    public decimal? Total { get; set; }

    [Header(TestHeaderNames.Cash)]
    [Input(true)]
    [Format(FormatEnum.CURRENCY)]
    public decimal? Cash { get; set; }
}

// Middle test entity
[ExcludeFromCodeCoverage]
public class TestVisitEntity : TestAmountEntity
{
    [Header(TestHeaderNames.Trips)]
    [Input(true)]
    [Format(FormatEnum.NUMBER)]
    public int Trips { get; set; }

    [Header(TestHeaderNames.FirstTrip)]
    [Input(true)]
    public string FirstTrip { get; set; } = "";

    [Header(TestHeaderNames.LastTrip)]
    [Input(true)]
    public string LastTrip { get; set; } = "";
}

// Derived test entity
[ExcludeFromCodeCoverage]
public class TestAddressEntity : TestVisitEntity
{
    public int RowId { get; set; } // No Column attribute - should be ignored

    [Header(TestHeaderNames.Address)]
    [Input(true)]
    public string Address { get; set; } = "";

    [Header(TestHeaderNames.Distance)]
    [Input(true)]
    [Format(FormatEnum.NUMBER)]
    public decimal Distance { get; set; }

    public bool Saved { get; set; } // No Column attribute - should be ignored
}

// Simple entity without inheritance
[ExcludeFromCodeCoverage]
public class TestSimpleEntity
{
    [Header(TestHeaderNames.Name)]
    [Input(true)]
    public string Name { get; set; } = "";

    [Header(TestHeaderNames.Date)]
    [Input(true)]
    public string Date { get; set; } = "";

    public int Id { get; set; } // No Column attribute - should be ignored
}

// Entity with invalid header reference (for error testing)
[ExcludeFromCodeCoverage]
public class TestInvalidEntity
{
    [Header("Invalid Header Name")]
    [Input(true)]
    public string InvalidProperty { get; set; } = "";

    [Header(TestHeaderNames.Name)]
    [Input(true)]
    public string ValidProperty { get; set; } = "";
}

// Entity with no Column attributes
[ExcludeFromCodeCoverage]
public class TestNoAttributesEntity
{
    public string Name { get; set; } = "";
    public int Value { get; set; }
}