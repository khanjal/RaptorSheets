using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Entities;
using RaptorSheets.Home.Constants;
using System.Diagnostics.CodeAnalysis;

namespace RaptorSheets.Home.Entities;

/// <summary>
/// A single property fact as a name/value pair (e.g. "Beds" / "3", "Built" / "1998").
/// </summary>
[ExcludeFromCodeCoverage]
public class StatEntity : SheetRowEntityBase
{
    [Column(SheetsConfig.HeaderNames.Name, isInput: true)]
    public string Name { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Value, isInput: true)]
    public string Value { get; set; } = "";
}
