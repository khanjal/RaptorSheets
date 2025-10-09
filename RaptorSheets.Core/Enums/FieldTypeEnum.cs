namespace RaptorSheets.Core.Enums;

/// <summary>
/// Defines the data types for sheet fields with automatic formatting and conversion
/// </summary>
public enum FieldTypeEnum
{
    /// <summary>
    /// Plain text field
    /// </summary>
    String,
    
    /// <summary>
    /// Numeric field with decimal places
    /// </summary>
    Number,
    
    /// <summary>
    /// Currency field with currency symbol and formatting
    /// </summary>
    Currency,
    
    /// <summary>
    /// Date and time field with proper serialization for Google Sheets
    /// </summary>
    DateTime,
    
    /// <summary>
    /// Phone number field with standardized formatting
    /// </summary>
    PhoneNumber,
    
    /// <summary>
    /// Boolean field (true/false)
    /// </summary>
    Boolean,
    
    /// <summary>
    /// Integer field (whole numbers)
    /// </summary>
    Integer,
    
    /// <summary>
    /// Email address field
    /// </summary>
    Email,
    
    /// <summary>
    /// URL field for web addresses
    /// </summary>
    Url,
    
    /// <summary>
    /// Percentage field with % symbol
    /// </summary>
    Percentage
}