namespace RaptorSheets.Core.Entities;

public class PropertyEntity
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public Dictionary<string, string> Attributes { get; set; } = [];
}
