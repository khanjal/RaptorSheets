using System;
using RaptorSheets.Core.Enums;

namespace RaptorSheets.Core.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class FormatAttribute : Attribute
{
    public FormatEnum FormatType { get; }
    public string? Pattern { get; }

    public FormatAttribute(FormatEnum formatType)
    {
        FormatType = formatType;
        Pattern = null;
    }

    public FormatAttribute(string pattern)
    {
        FormatType = FormatEnum.DEFAULT;
        Pattern = pattern;
    }

    public FormatAttribute(FormatEnum formatType, string pattern)
    {
        FormatType = formatType;
        Pattern = pattern;
    }
}
