namespace RaptorSheets.Gig.Entities;

/// <summary>
/// Base class for sheet row entities that support update/save logic.
/// Not used for spreadsheet serialization.
/// </summary>
public abstract class SheetRowEntityBase
{
    public int RowId { get; set; }
    public string Action { get; set; } = string.Empty;
    public bool Saved { get; set; }
}
