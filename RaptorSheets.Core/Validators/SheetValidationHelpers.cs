using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Helpers;

namespace RaptorSheets.Core.Validators;

/// <summary>
/// Validation utilities for sheet operations with standardized error reporting
/// </summary>
public static class SheetValidationHelpers
{
    /// <summary>
    /// Validate that required parameters are not null or empty
    /// </summary>
    public static List<MessageEntity> ValidateRequiredParameters(params (string? value, string paramName)[] parameters)
    {
        var messages = new List<MessageEntity>();

        foreach (var (value, paramName) in parameters)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                messages.Add(MessageHelpers.CreateErrorMessage(
                    $"Required parameter '{paramName}' is null or empty", 
                    MessageTypeEnum.VALIDATION));
            }
        }

        return messages;
    }

    /// <summary>
    /// Validate that collections are not null or empty
    /// </summary>
    public static List<MessageEntity> ValidateRequiredCollections<T>(params (IEnumerable<T>? collection, string collectionName)[] collections)
    {
        var messages = new List<MessageEntity>();

        foreach (var (collection, collectionName) in collections)
        {
            if (collection == null || !collection.Any())
            {
                messages.Add(MessageHelpers.CreateErrorMessage(
                    $"Required collection '{collectionName}' is null or empty", 
                    MessageTypeEnum.VALIDATION));
            }
        }

        return messages;
    }

    /// <summary>
    /// Validate spreadsheet ID format
    /// </summary>
    public static List<MessageEntity> ValidateSpreadsheetId(string spreadsheetId)
    {
        var messages = new List<MessageEntity>();

        if (string.IsNullOrWhiteSpace(spreadsheetId))
        {
            messages.Add(MessageHelpers.CreateErrorMessage(
                "Spreadsheet ID cannot be null or empty", 
                MessageTypeEnum.VALIDATION));
        }
        else if (spreadsheetId.Length < 40 || spreadsheetId.Length > 50)
        {
            messages.Add(MessageHelpers.CreateWarningMessage(
                "Spreadsheet ID format may be invalid", 
                MessageTypeEnum.VALIDATION));
        }

        return messages;
    }

    /// <summary>
    /// Validate date format for sheet operations
    /// </summary>
    public static List<MessageEntity> ValidateDateFormat(string dateString, string fieldName)
    {
        var messages = new List<MessageEntity>();

        if (!string.IsNullOrWhiteSpace(dateString) && !DateTime.TryParse(dateString, out _))
        {
            messages.Add(MessageHelpers.CreateErrorMessage(
                $"Invalid date format in field '{fieldName}': {dateString}", 
                MessageTypeEnum.VALIDATION));
        }

        return messages;
    }

    /// <summary>
    /// Validate decimal values for financial fields
    /// </summary>
    public static List<MessageEntity> ValidateDecimalField(decimal? value, string fieldName, bool allowNegative = false)
    {
        var messages = new List<MessageEntity>();

        if (value.HasValue && !allowNegative && value < 0)
        {
            messages.Add(MessageHelpers.CreateErrorMessage(
                $"Field '{fieldName}' cannot be negative: {value}", 
                MessageTypeEnum.VALIDATION));
        }

        return messages;
    }
}