namespace RaptorSheets.Core.Enums;

/// <summary>
/// Defines the data types for sheet fields with automatic formatting and conversion
/// </summary>
public enum FieldType
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
    /// Currency field with currency symbol and formatting ($#,##0.00)
    /// </summary>
    Currency,
    
    /// <summary>
    /// Accounting format with aligned currency symbols and negative number handling
    /// Often used for financial statements where alignment is important
    /// </summary>
    Accounting,
    
    /// <summary>
    /// Date and time field with proper serialization for Google Sheets
    /// Uses ToSerialDate() conversion for date values
    /// </summary>
    DateTime,
    
    /// <summary>
    /// Time-only field with proper serialization for Google Sheets
    /// Uses ToSerialTime() conversion for time-of-day values
    /// </summary>
    Time,
    
    /// <summary>
    /// Duration/elapsed time field with proper serialization for Google Sheets
    /// Uses ToSerialDuration() conversion for time span values
    /// </summary>
    Duration,
    
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