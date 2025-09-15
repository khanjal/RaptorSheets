using System;

namespace RaptorSheets.Core.Attributes;

/// <summary>
/// Specifies the order for a sheet when generating the tab order in a workbook.
/// The sheet name should reference a constant from SheetsConfig.SheetNames.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class SheetOrderAttribute : Attribute
{
    /// <summary>
    /// Gets the order position for this sheet in the workbook.
    /// Lower numbers appear first (leftmost tabs).
    /// </summary>
    public int Order { get; }

    /// <summary>
    /// Gets the sheet name that this property represents.
    /// Should reference a constant from SheetsConfig.SheetNames.
    /// </summary>
    public string SheetName { get; }

    /// <summary>
    /// Initializes a new instance of the SheetOrderAttribute.
    /// </summary>
    /// <param name="order">The order position (0-based, lower numbers appear first)</param>
    /// <param name="sheetName">The sheet name from SheetsConfig.SheetNames</param>
    public SheetOrderAttribute(int order, string sheetName)
    {
        Order = order;
        SheetName = sheetName ?? throw new ArgumentNullException(nameof(sheetName));
    }
}