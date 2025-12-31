using RaptorSheets.Core.Enums;

namespace RaptorSheets.Core.Entities;

/// <summary>
/// Represents formatting options that can be reapplied to sheets or columns.
/// Allows selective reapplication of specific formatting aspects.
/// </summary>
public class FormattingOptionsEntity
{
    /// <summary>
    /// If true, reapply column data types (text, number, date, currency, etc.).
    /// </summary>
    public bool ReapplyColumnFormats { get; set; } = true;

    /// <summary>
    /// If true, reapply column borders (style and scope).
    /// </summary>
    public bool ReapplyBorders { get; set; } = false;

    /// <summary>
    /// If true, reapply tab color and alternating row colors.
    /// </summary>
    public bool ReapplyColors { get; set; } = false;

    /// <summary>
    /// If true, reapply sheet and column protection settings.
    /// </summary>
    public bool ReapplyProtection { get; set; } = false;

    /// <summary>
    /// If true, modify frozen rows/columns settings.
    /// </summary>
    public bool ReapplyFrozenRows { get; set; } = false;

    /// <summary>
    /// Gets a value indicating whether any formatting options are enabled.
    /// </summary>
    public bool HasAnyOptions => 
        ReapplyColumnFormats || ReapplyBorders || ReapplyColors || ReapplyProtection || ReapplyFrozenRows;

    /// <summary>
    /// Creates a new instance with all options disabled.
    /// </summary>
    public static FormattingOptionsEntity None => new()
    {
        ReapplyColumnFormats = false,
        ReapplyBorders = false,
        ReapplyColors = false,
        ReapplyProtection = false,
        ReapplyFrozenRows = false
    };

    /// <summary>
    /// Creates a new instance with all options enabled.
    /// </summary>
    public static FormattingOptionsEntity All => new()
    {
        ReapplyColumnFormats = true,
        ReapplyBorders = true,
        ReapplyColors = true,
        ReapplyProtection = true,
        ReapplyFrozenRows = true
    };

    /// <summary>
    /// Creates a new instance with common formatting options (formats, colors, frozen rows).
    /// </summary>
    public static FormattingOptionsEntity Common => new()
    {
        ReapplyColumnFormats = true,
        ReapplyBorders = false,
        ReapplyColors = true,
        ReapplyProtection = false,
        ReapplyFrozenRows = true
    };
}

/// <summary>
/// Represents a request to reapply formatting to a sheet.
/// </summary>
public class ReapplyFormattingRequest
{
    /// <summary>
    /// Name of the sheet to reapply formatting to.
    /// </summary>
    public required string SheetName { get; init; }

    /// <summary>
    /// Formatting options to apply.
    /// </summary>
    public FormattingOptionsEntity Options { get; init; } = FormattingOptionsEntity.Common;

    /// <summary>
    /// Optional specific column indexes to format (null means all columns).
    /// </summary>
    public List<int>? ColumnIndexes { get; init; }

    /// <summary>
    /// If true, only apply formatting to the header row.
    /// </summary>
    public bool HeaderOnly { get; init; } = false;
}
