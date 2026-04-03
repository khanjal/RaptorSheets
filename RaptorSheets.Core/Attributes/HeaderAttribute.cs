using System;

namespace RaptorSheets.Core.Attributes;

/// <summary>
/// Attribute used to specify the header name for a property/column.
/// This replaces the legacy `[Column(...)]` attribute for header identity.
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
