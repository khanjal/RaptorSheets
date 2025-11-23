using System.Text.Json;
using System.Text.Json.Serialization;

namespace RaptorSheets.Core.Serialization;

/// <summary>
/// Extension methods for configuring JsonSerializerOptions to use ColumnAttribute for JSON property names.
/// </summary>
public static class JsonSerializerOptionsExtensions
{
    /// <summary>
    /// Configures JsonSerializerOptions to use ColumnAttribute.JsonPropertyName for property naming.
    /// This eliminates the need for separate [JsonPropertyName] attributes on entity properties.
    /// </summary>
    /// <param name="options">The JsonSerializerOptions to configure</param>
    /// <returns>The configured JsonSerializerOptions for method chaining</returns>
    public static JsonSerializerOptions UseColumnAttributeNaming(this JsonSerializerOptions options)
    {
        // Add the converter factory that handles ColumnAttribute naming
        options.Converters.Add(new ColumnAttributeJsonConverterFactory());
        
        // Use camelCase as the default for properties without ColumnAttribute
        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        
        return options;
    }

    /// <summary>
    /// Creates a new JsonSerializerOptions configured to use ColumnAttribute.JsonPropertyName.
    /// </summary>
    /// <returns>Configured JsonSerializerOptions instance</returns>
    public static JsonSerializerOptions CreateWithColumnAttributeNaming()
    {
        var options = new JsonSerializerOptions();
        return options.UseColumnAttributeNaming();
    }
}
