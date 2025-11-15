using RaptorSheets.Core.Enums;

namespace RaptorSheets.Core.Attributes;

/// <summary>
/// Configuration options for ColumnAttribute when using advanced customization.
/// Provides a cleaner API when multiple optional parameters are needed.
/// </summary>
public class ColumnOptions
{
    /// <summary>
    /// Gets or sets the custom number format pattern for Google Sheets (null = use default).
    /// </summary>
    public string? FormatPattern { get; set; }

    /// <summary>
    /// Gets or sets the custom JSON property name (null = auto-generate from header).
    /// </summary>
    public string? JsonPropertyName { get; set; }

    /// <summary>
    /// Gets or sets the column order priority (-1 = use declaration order).
    /// </summary>
    public int Order { get; set; } = -1;

    /// <summary>
    /// Gets or sets whether this is a user-input column (default: false for output/formula columns).
    /// </summary>
    public bool IsInput { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to enable field validation (default: false).
    /// </summary>
    public bool EnableValidation { get; set; } = false;

    /// <summary>
    /// Gets or sets the custom validation pattern (null = use default for field type).
    /// </summary>
    public string? ValidationPattern { get; set; }

    /// <summary>
    /// Gets or sets the note/comment to display in Google Sheets (default: null).
    /// </summary>
    public string? Note { get; set; }

    /// <summary>
    /// Gets or sets the format type for Google Sheets display (DEFAULT = use default from fieldType).
    /// </summary>
    public FormatEnum FormatType { get; set; } = FormatEnum.DEFAULT;

    /// <summary>
    /// Creates a new ColumnOptions instance with default values.
    /// </summary>
    public ColumnOptions()
    {
    }

    /// <summary>
    /// Creates a fluent builder for ColumnOptions.
    /// </summary>
    public static ColumnOptionsBuilder Builder() => new();
}

/// <summary>
/// Fluent builder for ColumnOptions to provide a clean, discoverable API.
/// </summary>
public class ColumnOptionsBuilder
{
    private readonly ColumnOptions _options = new();

    /// <summary>
    /// Sets the custom number format pattern.
    /// </summary>
    public ColumnOptionsBuilder WithFormatPattern(string formatPattern)
    {
        _options.FormatPattern = formatPattern;
        return this;
    }

    /// <summary>
    /// Sets the custom JSON property name.
    /// </summary>
    public ColumnOptionsBuilder WithJsonPropertyName(string jsonPropertyName)
    {
        _options.JsonPropertyName = jsonPropertyName;
        return this;
    }

    /// <summary>
    /// Sets the column order priority.
    /// </summary>
    public ColumnOptionsBuilder WithOrder(int order)
    {
        _options.Order = order;
        return this;
    }

    /// <summary>
    /// Marks this column as a user-input column.
    /// </summary>
    public ColumnOptionsBuilder AsInput()
    {
        _options.IsInput = true;
        return this;
    }

    /// <summary>
    /// Marks this column as an output/formula column (default).
    /// </summary>
    public ColumnOptionsBuilder AsOutput()
    {
        _options.IsInput = false;
        return this;
    }

    /// <summary>
    /// Enables validation for this column.
    /// </summary>
    public ColumnOptionsBuilder WithValidation(string? validationPattern = null)
    {
        _options.EnableValidation = true;
        _options.ValidationPattern = validationPattern;
        return this;
    }

    /// <summary>
    /// Sets a note/comment to display in Google Sheets.
    /// </summary>
    public ColumnOptionsBuilder WithNote(string note)
    {
        _options.Note = note;
        return this;
    }

    /// <summary>
    /// Sets the format type for Google Sheets display.
    /// </summary>
    public ColumnOptionsBuilder WithFormatType(FormatEnum formatType)
    {
        _options.FormatType = formatType;
        return this;
    }

    /// <summary>
    /// Builds the ColumnOptions instance.
    /// </summary>
    public ColumnOptions Build() => _options;

    /// <summary>
    /// Implicit conversion to ColumnOptions for convenience.
    /// </summary>
    public static implicit operator ColumnOptions(ColumnOptionsBuilder builder) => builder.Build();
}
