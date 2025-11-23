using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using RaptorSheets.Core.Attributes;

namespace RaptorSheets.Core.Serialization;

/// <summary>
/// JSON naming policy that reads property names from ColumnAttribute.JsonPropertyName.
/// Falls back to camelCase for properties without ColumnAttribute.
/// </summary>
public class ColumnAttributeNamingPolicy : JsonNamingPolicy
{
    private static readonly JsonNamingPolicy CamelCase = JsonNamingPolicy.CamelCase;
    
    public override string ConvertName(string name)
    {
        // This method is called for types without reflection context,
        // so we fall back to camelCase
        return CamelCase.ConvertName(name);
    }
}

/// <summary>
/// JSON converter factory that applies ColumnAttribute.JsonPropertyName to entity properties.
/// </summary>
public class ColumnAttributeJsonConverterFactory : System.Text.Json.Serialization.JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        // Apply to any class that has properties with ColumnAttribute
        return typeToConvert.IsClass && 
               typeToConvert.GetProperties()
                   .Any(p => p.GetCustomAttribute<ColumnAttribute>() != null);
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = typeof(ColumnAttributeJsonConverter<>).MakeGenericType(typeToConvert);
        return (JsonConverter?)Activator.CreateInstance(converterType);
    }
}

/// <summary>
/// JSON converter that applies ColumnAttribute.JsonPropertyName during serialization/deserialization.
/// </summary>
public class ColumnAttributeJsonConverter<T> : System.Text.Json.Serialization.JsonConverter<T> where T : class, new()
{
    private static readonly Dictionary<string, PropertyInfo> PropertyByJsonName = new();
    private static readonly Dictionary<string, string> JsonNameByProperty = new();
    
    static ColumnAttributeJsonConverter()
    {
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        
        foreach (var property in properties)
        {
            var columnAttr = property.GetCustomAttribute<ColumnAttribute>();
            var jsonName = columnAttr?.JsonPropertyName ?? JsonNamingPolicy.CamelCase.ConvertName(property.Name);
            
            PropertyByJsonName[jsonName] = property;
            JsonNameByProperty[property.Name] = jsonName;
        }
    }

    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected StartObject token");
        }

        var entity = new T();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return entity;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException("Expected PropertyName token");
            }

            var propertyName = reader.GetString();
            if (string.IsNullOrEmpty(propertyName))
            {
                continue;
            }

            reader.Read(); // Move to the value

            if (PropertyByJsonName.TryGetValue(propertyName, out var property))
            {
                var value = JsonSerializer.Deserialize(ref reader, property.PropertyType, options);
                property.SetValue(entity, value);
            }
        }

        throw new JsonException("Expected EndObject token");
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            var jsonName = JsonNameByProperty.TryGetValue(property.Name, out var name) 
                ? name 
                : JsonNamingPolicy.CamelCase.ConvertName(property.Name);

            var propertyValue = property.GetValue(value);

            writer.WritePropertyName(jsonName);
            JsonSerializer.Serialize(writer, propertyValue, property.PropertyType, options);
        }

        writer.WriteEndObject();
    }
}
