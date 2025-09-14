using System;

namespace RaptorSheets.Core.Attributes;

/// <summary>
/// Specifies the Google Sheets column order for entity properties by referencing header constants.
/// This eliminates the need for hardcoded column numbers or letters while maintaining clear ordering.
/// </summary>
/// <example>
/// Usage in entity:
/// <code>
/// public class AddressEntity : VisitEntity
/// {
///     [SheetOrder(SheetsConfig.HeaderNames.Address)]
///     public string Address { get; set; } = "";
///     
///     [SheetOrder(SheetsConfig.HeaderNames.Distance)]  
///     public decimal Distance { get; set; }
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class SheetOrderAttribute : Attribute
{
    /// <summary>
    /// The header name constant from SheetsConfig.HeaderNames that this property should map to.
    /// </summary>
    public string HeaderName { get; }

    /// <summary>
    /// Initializes a new instance of the SheetOrderAttribute with the specified header name.
    /// </summary>
    /// <param name="headerName">The header name constant from SheetsConfig.HeaderNames</param>
    /// <exception cref="ArgumentException">Thrown when headerName is null or empty</exception>
    public SheetOrderAttribute(string headerName)
    {
        if (string.IsNullOrWhiteSpace(headerName))
        {
            throw new ArgumentException("Header name cannot be null or empty", nameof(headerName));
        }
        
        HeaderName = headerName;
    }
}