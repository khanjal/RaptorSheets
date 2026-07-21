using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Home.Constants;
using System.Diagnostics.CodeAnalysis;

namespace RaptorSheets.Home.Entities;

[ExcludeFromCodeCoverage]
public class ProjectEntity : SheetRowEntityBase
{
    [Column(SheetsConfig.HeaderNames.Task, isInput: true)]
    public string Task { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Area, isInput: true, enableValidation: true, validationPattern: SheetsConfig.ValidationNames.RangeRoom)]
    public string Area { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Details, isInput: true)]
    public string Details { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Started, isInput: true, note: ColumnNotes.DateFormat, formatType: FormatEnum.DATE)]
    public string Started { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Completed, isInput: true, note: ColumnNotes.DateFormat, formatType: FormatEnum.DATE)]
    public string Completed { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.Notes, isInput: true)]
    public string Notes { get; set; } = "";

    [Column(SheetsConfig.HeaderNames.ApproximateCost, isInput: true, formatType: FormatEnum.ACCOUNTING)]
    public decimal? ApproximateCost { get; set; }
}
