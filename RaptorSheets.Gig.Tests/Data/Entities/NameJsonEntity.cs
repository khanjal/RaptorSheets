using System.Diagnostics.CodeAnalysis;

namespace RaptorSheets.Gig.Tests.Data.Entities;

[ExcludeFromCodeCoverage]
public class NameJsonEntity
{
    public required string Name { get; set; }
    public required string Address { get; set; }
}
