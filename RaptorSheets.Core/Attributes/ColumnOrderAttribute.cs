namespace RaptorSheets.Core.Attributes;

/// <summary>
/// [OBSOLETE] Use ColumnAttribute instead, which provides comprehensive column configuration.
/// ColumnAttribute supports field types, formatting, validation, ordering, and input/output distinction.
/// This attribute is kept for backward compatibility but will be removed in a future version.
/// </summary>
//[Obsolete("Use ColumnAttribute instead. ColumnAttribute provides comprehensive column configuration including field types, formatting, validation, and ordering.", error: true)]
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class ColumnOrderAttribute : Attribute
{
    /// <summary>
    /// Gets the header name that this property should map to in the sheet.
    /// </summary>
    public string HeaderName { get; }

    /// <summary>
    /// [OBSOLETE] Use ColumnAttribute instead.
    /// </summary>
    public ColumnOrderAttribute(string headerName)
    {
        HeaderName = headerName ?? throw new ArgumentNullException(nameof(headerName));
    }
}