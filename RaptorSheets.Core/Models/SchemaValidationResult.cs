namespace RaptorSheets.Core.Models;

/// <summary>
/// Represents the result of schema validation for a Google Sheets entity
/// </summary>
public class SchemaValidationResult
{
    /// <summary>
    /// Gets or sets whether the schema validation passed
    /// </summary>
    public bool IsValid { get; set; } = true;

    /// <summary>
    /// Gets the list of validation errors
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Gets the list of validation warnings
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// Gets a detailed error message combining all errors
    /// </summary>
    public string ErrorMessage => string.Join("; ", Errors);

    /// <summary>
    /// Gets a detailed warning message combining all warnings
    /// </summary>
    public string WarningMessage => string.Join("; ", Warnings);

    /// <summary>
    /// Gets whether there are any warnings
    /// </summary>
    public bool HasWarnings => Warnings.Count > 0;

    /// <summary>
    /// Adds an error to the validation result and sets IsValid to false
    /// </summary>
    /// <param name="error">The error message to add</param>
    public void AddError(string error)
    {
        if (!string.IsNullOrWhiteSpace(error))
        {
            Errors.Add(error);
            IsValid = false;
        }
    }

    /// <summary>
    /// Adds a warning to the validation result
    /// </summary>
    /// <param name="warning">The warning message to add</param>
    public void AddWarning(string warning)
    {
        if (!string.IsNullOrWhiteSpace(warning))
        {
            Warnings.Add(warning);
        }
    }

    /// <summary>
    /// Merges another validation result into this one
    /// </summary>
    /// <param name="other">The other validation result to merge</param>
    public void Merge(SchemaValidationResult other)
    {
        if (other == null) return;

        foreach (var error in other.Errors)
        {
            AddError(error);
        }

        foreach (var warning in other.Warnings)
        {
            AddWarning(warning);
        }
    }

    /// <summary>
    /// Creates a successful validation result
    /// </summary>
    public static SchemaValidationResult Success() => new() { IsValid = true };

    /// <summary>
    /// Creates a failed validation result with an error
    /// </summary>
    /// <param name="error">The error message</param>
    public static SchemaValidationResult Failure(string error) => new() { IsValid = false, Errors = { error } };
}