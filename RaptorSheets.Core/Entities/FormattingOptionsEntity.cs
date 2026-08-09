namespace RaptorSheets.Core.Entities;

/// <summary>
/// Which categories of a sheet's formatting to reapply via
/// <see cref="Managers.SheetManagerBase{TEntity}.ReapplyFormatting(List{string}, FormattingOptionsEntity?, CancellationToken)"/>
/// (see GitHub issue #28). <see cref="ReapplyBorders"/> is the one flag still unimplemented - it
/// needs a new per-column attribute surface (a border style configuration point on
/// <c>ColumnAttribute</c>/<c>SheetCellModel</c>) that doesn't exist yet, a bigger, separate task from
/// the other four (which only needed new Google API request builders, no schema change). Setting it
/// is accepted but is currently a no-op.
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
