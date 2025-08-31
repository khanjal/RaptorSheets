using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Interfaces;
using RaptorSheets.Core.Services;

namespace RaptorSheets.Core.Managers;

/// <summary>
/// Abstract base class for domain-specific Google Sheet managers.
/// Provides common functionality to reduce code duplication between Stock, Gig, and future packages.
/// </summary>
/// <typeparam name="TSheetEntity">The domain-specific sheet entity type</typeparam>
/// <typeparam name="TSheetEnum">The domain-specific sheet enum type</typeparam>
public abstract class BaseDomainSheetManager<TSheetEntity, TSheetEnum> : IDomainSheetOperations
    where TSheetEntity : class, new()
    where TSheetEnum : Enum
{
    protected readonly GoogleSheetService _googleSheetService;

    protected BaseDomainSheetManager(string accessToken, string spreadsheetId)
    {
        _googleSheetService = new GoogleSheetService(accessToken, spreadsheetId);
    }

    protected BaseDomainSheetManager(Dictionary<string, string> parameters, string spreadsheetId)
    {
        _googleSheetService = new GoogleSheetService(parameters, spreadsheetId);
    }

    /// <summary>
    /// Create a success message with consistent formatting
    /// </summary>
    protected static MessageEntity CreateSuccessMessage(string operation, string details, MessageTypeEnum messageType)
    {
        return MessageHelpers.CreateInfoMessage($"{operation} successful: {details}", messageType);
    }

    /// <summary>
    /// Create an error message with consistent formatting
    /// </summary>
    protected static MessageEntity CreateErrorMessage(string operation, string details, MessageTypeEnum messageType)
    {
        return MessageHelpers.CreateErrorMessage($"{operation} failed: {details}", messageType);
    }

    /// <summary>
    /// Create a warning message with consistent formatting
    /// </summary>
    protected static MessageEntity CreateWarningMessage(string operation, string details, MessageTypeEnum messageType)
    {
        return MessageHelpers.CreateWarningMessage($"{operation}: {details}", messageType);
    }

    /// <summary>
    /// Validate that all required sheets exist
    /// </summary>
    public virtual async Task<List<MessageEntity>> ValidateSheets()
    {
        var messages = new List<MessageEntity>();
        
        try
        {
            var spreadsheet = await _googleSheetService.GetSheetInfo();
            var missingSheets = SheetHelpers.CheckSheets<TSheetEnum>(spreadsheet);
            
            if (missingSheets.Count > 0)
            {
                messages.AddRange(missingSheets.Select(sheet => 
                    CreateErrorMessage("Validation", $"Missing required sheet: {sheet}", MessageTypeEnum.MISSING_SHEETS)));
            }
            else
            {
                messages.Add(CreateSuccessMessage("Validation", "All required sheets exist", MessageTypeEnum.CHECK_SHEET));
            }
        }
        catch (Exception ex)
        {
            messages.Add(CreateErrorMessage("Validation", ex.Message, MessageTypeEnum.API_ERROR));
        }

        return messages;
    }

    /// <summary>
    /// Get available sheet names for this domain - must be implemented by derived classes
    /// </summary>
    public abstract List<string> GetAvailableSheetNames();

    /// <summary>
    /// Check if a sheet exists in the spreadsheet
    /// </summary>
    public virtual async Task<bool> SheetExists(string sheetName)
    {
        try
        {
            var spreadsheet = await _googleSheetService.GetSheetInfo();
            var existingSheets = SheetHelpers.GetSpreadsheetSheets(spreadsheet);
            return existingSheets.Contains(sheetName.ToUpperInvariant());
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Common logic for handling missing sheets - can be overridden by derived classes
    /// </summary>
    protected virtual async Task<List<MessageEntity>> HandleMissingSheets(List<string> missingSheets)
    {
        var messages = new List<MessageEntity>();
        
        if (missingSheets.Count > 0)
        {
            try
            {
                var createResult = await CreateSheets(missingSheets);
                // Assuming the derived class returns messages in their entity
                messages.Add(CreateSuccessMessage("Auto-creation", $"Created {missingSheets.Count} missing sheets", MessageTypeEnum.CREATE_SHEET));
            }
            catch (Exception ex)
            {
                messages.Add(CreateErrorMessage("Auto-creation", ex.Message, MessageTypeEnum.CREATE_SHEET));
            }
        }

        return messages;
    }

    /// <summary>
    /// Create sheets - must be implemented by derived classes
    /// </summary>
    protected abstract Task<TSheetEntity> CreateSheets(List<string> sheets);
}