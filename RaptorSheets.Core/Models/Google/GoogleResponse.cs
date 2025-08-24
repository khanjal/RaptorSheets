using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace RaptorSheets.Core.Models.Google;

[ExcludeFromCodeCoverage]
public class GoogleResponse
{
    [JsonPropertyName("range")]
    public string Range { get; set; } = "";

    [JsonPropertyName("majorDimension")]
    public string MajorDimension { get; set; } = "";

    [JsonPropertyName("values")]
    public IList<IList<object>> Values { get; set; } = [];
}
