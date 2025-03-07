namespace RaptorSheets.Gig.Tests.Data.Entities;

public class PlaceJsonEntity
{
    public required string Name { get; set; }
    public required List<string> Addresses { get; set; }
    public required List<string> Services { get; set; }
    public required List<string> Types { get; set; }
}
