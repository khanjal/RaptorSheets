using RaptorSheets.Core.Attributes;
using RaptorSheets.Gig.Constants;
using System.Diagnostics.CodeAnalysis;

namespace RaptorSheets.Gig.Entities;

[ExcludeFromCodeCoverage]
public class SetupEntity
{
    public int RowId { get; set; }
    public string Action { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Name, Core.Enums.FieldTypeEnum.String)]
    public string Name { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Value, Core.Enums.FieldTypeEnum.String)]
    public string Value { get; set; } = "";

    public bool Saved { get; set; }
}