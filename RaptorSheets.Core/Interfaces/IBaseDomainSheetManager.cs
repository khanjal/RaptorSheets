using RaptorSheets.Core.Entities;

namespace RaptorSheets.Core.Interfaces;

/// <summary>
/// Base interface for domain-specific Google Sheet managers.
/// Provides consistent method signatures across Stock, Gig, and future domain packages.
/// </summary>
/// <typeparam name="TSheetEntity">The domain-specific sheet entity type</typeparam>
/// <typeparam name="TSheetEnum">The domain-specific sheet enum type</typeparam>
public interface IBaseDomainSheetManager<TSheetEntity, TSheetEnum> 
    where TSheetEntity : class, new()
    where TSheetEnum : Enum
{
    /// <summary>
    /// Change data in specified sheets
    /// </summary>
    Task<TSheetEntity> ChangeSheetData(List<string> sheets, TSheetEntity sheetEntity);

    /// <summary>
    /// Create all predefined sheets for this domain
    /// </summary>
    Task<TSheetEntity> CreateSheets();

    /// <summary>
    /// Create specific sheets
    /// </summary>
    Task<TSheetEntity> CreateSheets(List<string> sheets);

    /// <summary>
    /// Delete specified sheets
    /// </summary>
    Task<TSheetEntity> DeleteSheets(List<string> sheets);

    /// <summary>
    /// Get data from a single sheet
    /// </summary>
    Task<TSheetEntity> GetSheet(string sheet);

    /// <summary>
    /// Get data from all sheets
    /// </summary>
    Task<TSheetEntity> GetSheets();

    /// <summary>
    /// Get data from specified sheets
    /// </summary>
    Task<TSheetEntity> GetSheets(List<string> sheets);

    /// <summary>
    /// Get sheet properties for all sheets
    /// </summary>
    Task<List<PropertyEntity>> GetSheetProperties();

    /// <summary>
    /// Get sheet properties for specified sheets
    /// </summary>
    Task<List<PropertyEntity>> GetSheetProperties(List<string> sheets);
}

/// <summary>
/// Common operations that all domain sheet managers should support
/// </summary>
public interface IDomainSheetOperations
{
    /// <summary>
    /// Validate that all required sheets exist
    /// </summary>
    Task<List<MessageEntity>> ValidateSheets();

    /// <summary>
    /// Get available sheet names for this domain
    /// </summary>
    List<string> GetAvailableSheetNames();

    /// <summary>
    /// Check if a sheet exists in the spreadsheet
    /// </summary>
    Task<bool> SheetExists(string sheetName);
}