using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Entities;
using RaptorSheets.Gig.Constants;
using System.Diagnostics.CodeAnalysis;

namespace RaptorSheets.Gig.Entities;

[ExcludeFromCodeCoverage]
public class SetupEntity : SheetRowEntityBase
{
    [Column(SheetsConfig.HeaderNames.Name)]
    public string Name { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Value)]
    public string Value { get; set; } = "";
}