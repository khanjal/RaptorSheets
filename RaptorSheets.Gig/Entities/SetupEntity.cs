using RaptorSheets.Core.Attributes;
using RaptorSheets.Gig.Constants;
using System.Diagnostics.CodeAnalysis;

namespace RaptorSheets.Gig.Entities;

[ExcludeFromCodeCoverage]
public class SetupEntity
{
    public int RowId { get; set; }
    public string Action { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Name)]
    public string Name { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Value)]
    public string Value { get; set; } = "";

    public bool Saved { get; set; }
}