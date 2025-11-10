using System.Diagnostics.CodeAnalysis;
using RaptorSheets.Shared.Attributes;

namespace RaptorSheets.Gig.Entities;

[ExcludeFromCodeCoverage]
public class SetupEntity
{
    public int RowId { get; set; }
    public string Action { get; set; } = "";

    [ColumnOrder("Name")]
    public string Name { get; set; } = "";

    [ColumnOrder("Value")]
    public string Value { get; set; } = "";

    public bool Saved { get; set; }
}