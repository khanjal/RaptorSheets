using System;

namespace RaptorSheets.Core.Attributes;

/// <summary>
/// Attribute used to specify the header name for a property/column.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class HeaderAttribute : Attribute
{
    public string HeaderName { get; }

    public HeaderAttribute(string headerName)
    {
        HeaderName = headerName ?? throw new ArgumentNullException(nameof(headerName));
    }
}
