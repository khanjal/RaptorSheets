using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Mappers;
using HeaderEnum = RaptorSheets.Gig.Enums.HeaderEnum;

namespace RaptorSheets.Gig.Tests.Unit.Mappers;

public class ExpenseMapperTests
{
    [Fact]
    public void MapFromRangeData_WithValidData_ShouldReturnExpenses()
    {
        // Arrange
        var values = new List<IList<object>>
        {
            new List<object> { "Date", "Name", "Description", "Amount", "Category" }, // Headers
            new List<object> { "2024-01-15", "Gas", "Fuel for vehicle", "45.67", "Transportation" },
            new List<object> { "2024-01-16", "Food", "Lunch", "12.50", "Meals" }
        };

        // Act
        var result = ExpenseMapper.MapFromRangeData(values);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        
        var firstExpense = result[0];
        Assert.Equal(2, firstExpense.RowId); // Row 2 (after headers)
        Assert.Equal(new DateTime(2024, 1, 15), firstExpense.Date);
        Assert.Equal("Gas", firstExpense.Name);
        Assert.Equal("Fuel for vehicle", firstExpense.Description);
        Assert.Equal(45.67m, firstExpense.Amount);
        Assert.Equal("Transportation", firstExpense.Category);
        
        var secondExpense = result[1];
        Assert.Equal(3, secondExpense.RowId); // Row 3
        Assert.Equal(new DateTime(2024, 1, 16), secondExpense.Date);
        Assert.Equal("Food", secondExpense.Name);
        Assert.Equal("Lunch", secondExpense.Description);
        Assert.Equal(12.50m, secondExpense.Amount);
        Assert.Equal("Meals", secondExpense.Category);
    }

    [Fact]
    public void MapFromRangeData_WithEmptyRows_ShouldFilterOutEmptyRows()
    {
        // Arrange
        var values = new List<IList<object>>
        {
            new List<object> { "Date", "Name", "Description", "Amount", "Category" }, // Headers
            new List<object> { "2024-01-15", "Gas", "Fuel", "45.67", "Transport" },
            new List<object> { "", "", "", "", "" }, // Empty row - should be filtered
            new List<object> { "2024-01-16", "Food", "Lunch", "12.50", "Meals" }
        };

        // Act
        var result = ExpenseMapper.MapFromRangeData(values);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count); // Empty row should be filtered out
        Assert.Equal("Gas", result[0].Name);
        Assert.Equal("Food", result[1].Name);
    }

    [Fact]
    public void MapFromRangeData_WithInvalidDate_ShouldUseMinValue()
    {
        // Arrange
        var values = new List<IList<object>>
        {
            new List<object> { "Date", "Name", "Description", "Amount", "Category" },
            new List<object> { "invalid-date", "Test", "Description", "10.00", "Category" }
        };

        // Act
        var result = ExpenseMapper.MapFromRangeData(values);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(DateTime.MinValue, result[0].Date);
    }

    [Fact]
    public void MapFromRangeData_WithMissingColumns_ShouldUseDefaults()
    {
        // Arrange
        var values = new List<IList<object>>
        {
            new List<object> { "Name", "Amount" }, // Missing other columns
            new List<object> { "Test Expense", "25.50" }
        };

        // Act
        var result = ExpenseMapper.MapFromRangeData(values);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Test Expense", result[0].Name);
        Assert.Equal(25.50m, result[0].Amount);
        Assert.Equal(string.Empty, result[0].Description); // Default value
        Assert.Equal(string.Empty, result[0].Category);   // Default value
        Assert.Equal(DateTime.MinValue, result[0].Date);  // Default value
    }

    [Fact]
    public void MapFromRangeData_WithOnlyHeaders_ShouldReturnEmpty()
    {
        // Arrange
        var values = new List<IList<object>>
        {
            new List<object> { "Date", "Name", "Description", "Amount", "Category" } // Only headers
        };

        // Act
        var result = ExpenseMapper.MapFromRangeData(values);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void MapToRangeData_WithValidExpenses_ShouldReturnCorrectData()
    {
        // Arrange
        var expenses = new List<ExpenseEntity>
        {
            new()
            {
                Date = new DateTime(2024, 1, 15),
                Name = "Gas",
                Description = "Fuel",
                Amount = 45.67m,
                Category = "Transportation"
            },
            new()
            {
                Date = new DateTime(2024, 1, 16),
                Name = "Food",
                Description = "Lunch",
                Amount = 12.50m,
                Category = "Meals"
            }
        };
        var headers = new List<object> { "Date", "Name", "Description", "Amount", "Category" };

        // Act
        var result = ExpenseMapper.MapToRangeData(expenses, headers);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        
        var firstRow = result[0];
        Assert.Equal("2024-01-15", firstRow[0]);
        Assert.Equal("Gas", firstRow[1]);
        Assert.Equal("Fuel", firstRow[2]);
        Assert.Equal("45.67", firstRow[3]);
        Assert.Equal("Transportation", firstRow[4]);
        
        var secondRow = result[1];
        Assert.Equal("2024-01-16", secondRow[0]);
        Assert.Equal("Food", secondRow[1]);
        Assert.Equal("Lunch", secondRow[2]);
        Assert.Equal("12.5", secondRow[3]);
        Assert.Equal("Meals", secondRow[4]);
    }

    [Fact]
    public void MapToRangeData_WithUnknownHeader_ShouldUseNull()
    {
        // Arrange
        var expenses = new List<ExpenseEntity>
        {
            new() { Name = "Test", Amount = 10.00m }
        };
        var headers = new List<object> { "Name", "UnknownColumn", "Amount" };

        // Act
        var result = ExpenseMapper.MapToRangeData(expenses, headers);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        var row = result[0];
        Assert.Equal("Test", row[0]);
        Assert.Null(row[1]); // Unknown column should be null
        Assert.Equal("10", row[2]);
    }

    [Fact]
    public void MapToRangeData_WithEmptyExpenses_ShouldReturnEmpty()
    {
        // Arrange
        var expenses = new List<ExpenseEntity>();
        var headers = new List<object> { "Date", "Name" };

        // Act
        var result = ExpenseMapper.MapToRangeData(expenses, headers);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void MapToRowData_WithValidExpenses_ShouldReturnRowData()
    {
        // Arrange
        var expenses = new List<ExpenseEntity>
        {
            new()
            {
                Date = new DateTime(2024, 1, 15),
                Name = "Gas",
                Description = "Fuel",
                Amount = 45.67m,
                Category = "Transportation"
            }
        };
        var headers = new List<object> { "Date", "Name", "Description", "Amount", "Category" };

        // Act
        var result = ExpenseMapper.MapToRowData(expenses, headers);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        
        var rowData = result[0];
        Assert.Equal(5, rowData.Values.Count); // 5 columns
        
        // Check date cell (should be number value for serial date)
        Assert.NotNull(rowData.Values[0].UserEnteredValue.NumberValue);
        
        // Check string values
        Assert.Equal("Gas", rowData.Values[1].UserEnteredValue.StringValue);
        Assert.Equal("Fuel", rowData.Values[2].UserEnteredValue.StringValue);
        
        // Check amount (should be number value)
        Assert.Equal(45.67, rowData.Values[3].UserEnteredValue.NumberValue);
        
        // Check category
        Assert.Equal("Transportation", rowData.Values[4].UserEnteredValue.StringValue);
    }

    [Fact]
    public void MapToRowData_WithNullValues_ShouldHandleGracefully()
    {
        // Arrange
        var expenses = new List<ExpenseEntity>
        {
            new()
            {
                Date = DateTime.MinValue,
                Name = null,
                Description = null,
                Amount = 0m,
                Category = null
            }
        };
        var headers = new List<object> { "Date", "Name", "Description", "Amount", "Category" };

        // Act
        var result = ExpenseMapper.MapToRowData(expenses, headers);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        
        var rowData = result[0];
        Assert.Equal(5, rowData.Values.Count);
        
        // Null string values should result in null StringValue
        Assert.Null(rowData.Values[1].UserEnteredValue.StringValue);
        Assert.Null(rowData.Values[2].UserEnteredValue.StringValue);
        Assert.Null(rowData.Values[4].UserEnteredValue.StringValue);
    }

    [Fact]
    public void MapToRowData_WithUnknownHeaders_ShouldCreateEmptyCells()
    {
        // Arrange
        var expenses = new List<ExpenseEntity> { new() { Name = "Test" } };
        var headers = new List<object> { "Name", "UnknownColumn" };

        // Act
        var result = ExpenseMapper.MapToRowData(expenses, headers);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        
        var rowData = result[0];
        Assert.Equal(2, rowData.Values.Count);
        Assert.Equal("Test", rowData.Values[0].UserEnteredValue.StringValue);
        // Unknown column should result in empty CellData
        Assert.Null(rowData.Values[1].UserEnteredValue);
    }

    [Fact]
    public void MapToRowFormat_WithAllHeaders_ShouldReturnCorrectFormats()
    {
        // Arrange
        var headers = new List<object> { "Date", "Name", "Description", "Amount", "Category" };

        // Act
        var result = ExpenseMapper.MapToRowFormat(headers);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Values.Count);
        
        // Date should have date format
        Assert.NotNull(result.Values[0].UserEnteredFormat);
        
        // Amount should have accounting format
        Assert.NotNull(result.Values[3].UserEnteredFormat);
        
        // Other columns should have empty format
        Assert.Null(result.Values[1].UserEnteredFormat);
        Assert.Null(result.Values[2].UserEnteredFormat);
        Assert.Null(result.Values[4].UserEnteredFormat);
    }

    [Fact]
    public void MapToRowFormat_WithUnknownHeaders_ShouldCreateEmptyCells()
    {
        // Arrange
        var headers = new List<object> { "UnknownColumn1", "UnknownColumn2" };

        // Act
        var result = ExpenseMapper.MapToRowFormat(headers);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Values.Count);
        Assert.Null(result.Values[0].UserEnteredFormat);
        Assert.Null(result.Values[1].UserEnteredFormat);
    }

    [Fact]
    public void GetSheet_ShouldReturnCorrectSheetConfiguration()
    {
        // Act
        var result = ExpenseMapper.GetSheet();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Headers);
        Assert.Equal(5, result.Headers.Count); // Date, Name, Description, Amount, Category
        
        // Check header names and formats
        var dateHeader = result.Headers.FirstOrDefault(h => h.Name == HeaderEnum.DATE.GetDescription());
        Assert.NotNull(dateHeader);
        Assert.Equal(FormatEnum.DATE, dateHeader.Format);
        
        var nameHeader = result.Headers.FirstOrDefault(h => h.Name == HeaderEnum.NAME.GetDescription());
        Assert.NotNull(nameHeader);
        
        var descriptionHeader = result.Headers.FirstOrDefault(h => h.Name == HeaderEnum.DESCRIPTION.GetDescription());
        Assert.NotNull(descriptionHeader);
        
        var amountHeader = result.Headers.FirstOrDefault(h => h.Name == HeaderEnum.AMOUNT.GetDescription());
        Assert.NotNull(amountHeader);
        Assert.Equal(FormatEnum.ACCOUNTING, amountHeader.Format);
        
        var categoryHeader = result.Headers.FirstOrDefault(h => h.Name == HeaderEnum.CATEGORY.GetDescription());
        Assert.NotNull(categoryHeader);
    }

    [Theory]
    [InlineData(HeaderEnum.DATE)]
    [InlineData(HeaderEnum.NAME)]
    [InlineData(HeaderEnum.DESCRIPTION)]
    [InlineData(HeaderEnum.AMOUNT)]
    [InlineData(HeaderEnum.CATEGORY)]
    public void MapFromRangeData_WithSingleColumn_ShouldMapCorrectly(HeaderEnum headerType)
    {
        // Arrange
        var headerName = headerType.GetDescription();
        var values = new List<IList<object>>
        {
            new List<object> { headerName }, // Single header
            new List<object> { GetTestValueForHeader(headerType) } // Single value
        };

        // Act
        var result = ExpenseMapper.MapFromRangeData(values);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        
        var expense = result[0];
        switch (headerType)
        {
            case HeaderEnum.DATE:
                Assert.Equal(new DateTime(2024, 1, 15), expense.Date);
                break;
            case HeaderEnum.NAME:
                Assert.Equal("Test Name", expense.Name);
                break;
            case HeaderEnum.DESCRIPTION:
                Assert.Equal("Test Description", expense.Description);
                break;
            case HeaderEnum.AMOUNT:
                Assert.Equal(123.45m, expense.Amount);
                break;
            case HeaderEnum.CATEGORY:
                Assert.Equal("Test Category", expense.Category);
                break;
        }
    }

    private static object GetTestValueForHeader(HeaderEnum headerType)
    {
        return headerType switch
        {
            HeaderEnum.DATE => "2024-01-15",
            HeaderEnum.NAME => "Test Name",
            HeaderEnum.DESCRIPTION => "Test Description",
            HeaderEnum.AMOUNT => "123.45",
            HeaderEnum.CATEGORY => "Test Category",
            _ => "Test Value"
        };
    }

    [Fact]
    public void MapToRangeData_WithAllHeaderTypes_ShouldMapAllValues()
    {
        // Arrange
        var expense = new ExpenseEntity
        {
            Date = new DateTime(2024, 1, 15),
            Name = "Test Name",
            Description = "Test Description", 
            Amount = 123.45m,
            Category = "Test Category"
        };
        var expenses = new List<ExpenseEntity> { expense };
        var headers = new List<object> 
        { 
            HeaderEnum.DATE.GetDescription(),
            HeaderEnum.NAME.GetDescription(),
            HeaderEnum.DESCRIPTION.GetDescription(),
            HeaderEnum.AMOUNT.GetDescription(),
            HeaderEnum.CATEGORY.GetDescription()
        };

        // Act
        var result = ExpenseMapper.MapToRangeData(expenses, headers);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        
        var row = result[0];
        Assert.Equal("2024-01-15", row[0]);
        Assert.Equal("Test Name", row[1]);
        Assert.Equal("Test Description", row[2]);
        Assert.Equal("123.45", row[3]);
        Assert.Equal("Test Category", row[4]);
    }

    [Fact]
    public void MapToRowData_WithAllHeaderTypes_ShouldCreateCorrectCellTypes()
    {
        // Arrange
        var expense = new ExpenseEntity
        {
            Date = new DateTime(2024, 1, 15),
            Name = "Test Name",
            Description = "Test Description",
            Amount = 123.45m,
            Category = "Test Category"
        };
        var expenses = new List<ExpenseEntity> { expense };
        var headers = new List<object> 
        { 
            HeaderEnum.DATE.GetDescription(),
            HeaderEnum.NAME.GetDescription(),
            HeaderEnum.DESCRIPTION.GetDescription(),
            HeaderEnum.AMOUNT.GetDescription(),
            HeaderEnum.CATEGORY.GetDescription()
        };

        // Act
        var result = ExpenseMapper.MapToRowData(expenses, headers);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        
        var rowData = result[0];
        Assert.Equal(5, rowData.Values.Count);
        
        // Date should be NumberValue (serial date)
        Assert.NotNull(rowData.Values[0].UserEnteredValue.NumberValue);
        Assert.Null(rowData.Values[0].UserEnteredValue.StringValue);
        
        // Name should be StringValue
        Assert.Null(rowData.Values[1].UserEnteredValue.NumberValue);
        Assert.Equal("Test Name", rowData.Values[1].UserEnteredValue.StringValue);
        
        // Description should be StringValue
        Assert.Null(rowData.Values[2].UserEnteredValue.NumberValue);
        Assert.Equal("Test Description", rowData.Values[2].UserEnteredValue.StringValue);
        
        // Amount should be NumberValue
        Assert.Equal(123.45, rowData.Values[3].UserEnteredValue.NumberValue);
        Assert.Null(rowData.Values[3].UserEnteredValue.StringValue);
        
        // Category should be StringValue
        Assert.Null(rowData.Values[4].UserEnteredValue.NumberValue);
        Assert.Equal("Test Category", rowData.Values[4].UserEnteredValue.StringValue);
    }
}