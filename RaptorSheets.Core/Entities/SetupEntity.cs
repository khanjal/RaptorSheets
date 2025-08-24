namespace RaptorSheets.Core.Entities;

public class SetupEntity
{
    public int RowId { get; set; }
    public string Action { get; set; } = "";
    public string Name { get; set; } = "";
    public string Value { get; set; } = "";
    public bool Saved { get; set; }
}
