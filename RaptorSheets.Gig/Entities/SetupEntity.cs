using RaptorSheets.Core.Attributes;
using RaptorSheets.Gig.Constants;
using System.Diagnostics.CodeAnalysis;

namespace RaptorSheets.Gig.Entities;

[ExcludeFromCodeCoverage]
public class SetupEntity
{
    public int RowId { get; set; }
    public string Action { get; set; } = "";

    [Header(SheetsConfig.HeaderNames.Name)]
        public string Name { get; set; } = string.Empty;

    [Header(SheetsConfig.HeaderNames.Value)]
        public string Value { get; set; } = string.Empty;

    public bool Saved { get; set; }
}