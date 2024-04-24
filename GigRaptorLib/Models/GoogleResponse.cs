using System.Text.Json.Serialization;

namespace GigRaptorLib.Models;

public class GoogleResponse
{
    [JsonPropertyName("range")]
    public string Range { get; set; }

    [JsonPropertyName("majorDimension")]
    public string MajorDimension { get; set; }

    [JsonPropertyName("values")]
    public IList<IList<object>> Values { get; set; }
}
