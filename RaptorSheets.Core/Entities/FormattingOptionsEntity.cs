namespace RaptorSheets.Core.Entities;

/// <summary>
/// Which categories of a sheet's formatting to reapply via
/// <see cref="Managers.SheetManagerBase{TEntity}.ReapplyFormatting(List{string}, FormattingOptionsEntity?, CancellationToken)"/>
/// (see GitHub issue #28). Only <see cref="ReapplyColumnFormats"/> is implemented today - the other
/// four flags exist so the public API shape doesn't need to break when they land. Each needs its own
/// new Google API request builder (tab/banding colors and frozen rows need an UpdateSheetProperties
/// builder; protection needs a read-then-delete-then-readd pass to dedupe against this library's own
/// previously-added ProtectedRanges; borders need a new per-column attribute surface that doesn't
/// exist yet) - tracked as follow-up work, not attempted here.
/// </summary>
public class FormattingOptionsEntity
{
    public bool ReapplyColumnFormats { get; set; } = true;
    public bool ReapplyBorders { get; set; } = false;
    public bool ReapplyColors { get; set; } = false;
    public bool ReapplyProtection { get; set; } = false;
    public bool ReapplyFrozenRows { get; set; } = false;

    public static FormattingOptionsEntity None => new() { ReapplyColumnFormats = false };
    public static FormattingOptionsEntity All => new() { ReapplyColumnFormats = true, ReapplyBorders = true, ReapplyColors = true, ReapplyProtection = true, ReapplyFrozenRows = true };
    public static FormattingOptionsEntity Common => new() { ReapplyColumnFormats = true, ReapplyColors = true, ReapplyFrozenRows = true };
}
