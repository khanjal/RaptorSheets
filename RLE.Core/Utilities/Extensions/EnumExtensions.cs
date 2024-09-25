// Credits: https://gist.github.com/cocowalla

using System.Collections.Concurrent;
using System.ComponentModel;
using System.Reflection;

namespace RLE.Core.Utilities.Extensions;

public static class EnumExtensions
{
    // Note that we never need to expire these cache items, so we just use ConcurrentDictionary rather than MemoryCache
    private static readonly
        ConcurrentDictionary<string, string> DisplayNameCache = new();

    public static string GetDescription(this Enum value)
    {
        var key = $"{value.GetType().FullName}.{value}";

        var displayName = DisplayNameCache.GetOrAdd(key, x =>
        {
            var name = value?
                .GetType()?
                .GetTypeInfo()?
                .GetField(value.ToString())?
                .GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];

            return name?.Length > 0 ? name[0].Description : value!.ToString();
        });

        return displayName;
    }

    public static string UpperName(this Enum value)
    {
        return value.GetDescription().ToUpper();
    }

    public static T? GetValueFromName<T>(this string name) where T : Enum
    {
        var type = typeof(T);

        foreach (var field in type.GetFields())
        {
            if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
            {
                if (attribute.Description == name)
                {
                    return (T?)field.GetValue(null);
                }
            }

            if (field.Name == name)
            {
                return (T?)field.GetValue(null);
            }
        }

        return default;
    }
}