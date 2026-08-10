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
    public Format FormatType { get; set; } = Format.DEFAULT;

    /// <summary>
    /// Gets or sets whether unparseable cell values for this column are suppressed from
    /// <see cref="RaptorSheets.Core.Models.MappingIssue"/> reporting (default: false). Reads never
    /// fail because of a bad cell either way - this only silences the diagnostic, for columns where
    /// non-conforming values are expected rather than exceptional (e.g. a formula/output column that
    /// can legitimately read back as "#N/A").
    /// </summary>
    public bool IgnoreMappingErrors { get; set; } = false;

    /// <summary>
    /// Gets or sets whether Google Sheets should get a named range for this column's data
    /// (default: false). See <see cref="ColumnAttribute.NamedRange"/>.
    /// </summary>
    public bool NamedRange { get; set; } = false;

    /// <summary>
    /// Gets or sets a domain-specific conditional-format rule identifier for this column
    /// (default: null). See <see cref="ColumnAttribute.ConditionalFormat"/>.
    /// </summary>
    public string? ConditionalFormat { get; set; }

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

    /// <summary>
    /// Validates this instance, throwing if a property holds a value with no defined meaning.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <see cref="Order"/> is less than -1. -1 is the "use declaration order" sentinel; any other
    /// negative value is silently treated the same as -1 by <see cref="ColumnAttribute.HasExplicitOrder"/>,
    /// masking what was likely meant to be an explicit priority.
    /// </exception>
    public void Validate()
    {
        if (Order < -1)
        {
            // "Order" is a property, not a parameter of this method - S3928's real-parameter check
            // is a false positive here; nameof(Order) is still the correct thing to report.
#pragma warning disable S3928
            throw new ArgumentOutOfRangeException(nameof(Order), Order,
                "Order must be -1 (use declaration order) or a non-negative explicit priority.");
#pragma warning restore S3928
        }
    }
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
    public ColumnOptionsBuilder WithFormatType(Format formatType)
    {
        _options.FormatType = formatType;
        return this;
    }

    /// <summary>
    /// Suppresses mapping-error diagnostics for this column - use for columns where unparseable
    /// values are expected (e.g. a formula/output column that can read back as "#N/A").
    /// </summary>
    public ColumnOptionsBuilder IgnoreMappingErrors()
    {
        _options.IgnoreMappingErrors = true;
        return this;
    }

    /// <summary>
    /// Gives this column's data a named range - see <see cref="ColumnAttribute.NamedRange"/>.
    /// </summary>
    public ColumnOptionsBuilder WithNamedRange()
    {
        _options.NamedRange = true;
        return this;
    }

    /// <summary>
    /// Sets a domain-specific conditional-format rule identifier for this column - see
    /// <see cref="ColumnAttribute.ConditionalFormat"/>.
    /// </summary>
    public ColumnOptionsBuilder WithConditionalFormat(string conditionalFormat)
    {
        _options.ConditionalFormat = conditionalFormat;
        return this;
    }

    /// <summary>
    /// Builds the ColumnOptions instance, after validating it.
    /// </summary>
    public ColumnOptions Build()
    {
        _options.Validate();
        return _options;
    }

    /// <summary>
    /// Implicit conversion to ColumnOptions for convenience.
    /// </summary>
    public static implicit operator ColumnOptions(ColumnOptionsBuilder builder) => builder.Build();
}
