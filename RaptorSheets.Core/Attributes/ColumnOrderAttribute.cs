using System;

namespace RaptorSheets.Core.Attributes;

/// <summary>
/// Specifies the column order for a property when generating sheet headers.
/// The column name should reference a constant from SheetsConfig.HeaderNames.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class ColumnOrderAttribute : Attribute
{
    /// <summary>
    /// Gets the header name that this property should map to in the sheet.
    /// Should reference a constant from SheetsConfig.HeaderNames.
    /// </summary>
    public string HeaderName { get; }

    /// <summary>
    /// Initializes a new instance of the ColumnOrderAttribute.
    /// </summary>
    /// <param name="headerName">The header name from SheetsConfig.HeaderNames that this property maps to</param>
    public ColumnOrderAttribute(string headerName)
    {
        HeaderName = headerName ?? throw new ArgumentNullException(nameof(headerName));
    }
}