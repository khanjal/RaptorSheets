using RaptorSheets.Core.Attributes;
using RaptorSheets.Gig.Constants;
using System.Diagnostics.CodeAnalysis;

namespace RaptorSheets.Gig.Entities;

[ExcludeFromCodeCoverage]
public class PlaceSummaryEntity : SheetRowEntityBase
{
    [Column(SheetsConfig.HeaderNames.Place)]
    public string Place { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Address)]
    public string Address { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Count)]
    public int Count { get; set; }
}
