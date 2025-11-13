using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Mappers;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Gig.Entities;
using HeaderEnum = RaptorSheets.Gig.Enums.HeaderEnum;
using RaptorSheets.Gig.Constants;

namespace RaptorSheets.Gig.Tests.Unit.Mappers;

public class ExpenseMapperTests
{
    #region Core Mapping Tests (Essential)
    
    [Fact]
    public void MapFromRangeData_WithValidData_ShouldReturnExpenses()
    {
        // Arrange
        var values = new List<IList<object>>
        {
            new List<object> { "Date", "Name", "Description", "Amount", "Category" },
            new List<object> { "2024-01-15", "Gas", "Fuel for vehicle", "45.67", "Transportation" },
            new List<object> { "2024-01-16", "Food", "Lunch", "12.50", "Meals" }
        };

        // Act
        var result = GenericSheetMapper<ExpenseEntity>.MapFromRangeData(values);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        
        var firstExpense = result[0];
        Assert.Equal(2, firstExpense.RowId);
        Assert.Equal("2024-01-15", firstExpense.Date);  // Now string, not DateTime
        Assert.Equal("Gas", firstExpense.Name);
        Assert.Equal("Fuel for vehicle", firstExpense.Description);
        Assert.Equal(45.67m, firstExpense.Amount);
        Assert.Equal("Transportation", firstExpense.Category);
        
        var secondExpense = result[1];
        Assert.Equal(3, secondExpense.RowId);
        Assert.Equal("2024-01-16", secondExpense.Date);  // Now string, not DateTime
        Assert.Equal("Food", secondExpense.Name);
    }

    [Fact]
    public void MapFromRangeData_WithEmptyRows_ShouldFilterOutEmptyRows()
    {
        // Arrange
        var values = new List<IList<object>>
        {
            new List<object> { "Date", "Name", "Amount" },
            new List<object> { "2024-01-15", "Gas", "45.67" },
            new List<object> { "", "", "" }, // Empty row
            new List<object> { "2024-01-16", "Food", "12.50" }
        };

        // Act
        var result = GenericSheetMapper<ExpenseEntity>.MapFromRangeData(values);

        // Assert
        Assert.Equal(2, result.Count); // Empty row filtered out
        Assert.Equal("Gas", result[0].Name);
        Assert.Equal("Food", result[1].Name);
    }

    [Fact]
    public void MapToRangeData_WithValidExpenses_ShouldReturnCorrectData()
    {
        // Arrange
        var expenses = new List<ExpenseEntity>
        {
            new()
            {
                Date = "2024-01-15",  // Now string, not DateTime
                Name = "Gas",
                Description = "Fuel for vehicle",
                Amount = 45.67m,
                Category = "Transportation"
            }
        };
        var headers = new List<object> { "Date", "Name", "Description", "Amount", "Category" };

        // Act
        var result = GenericSheetMapper<ExpenseEntity>.MapToRangeData(expenses, headers);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        
        var row = result[0];
        Assert.Equal("2024-01-15", row[0]);
        Assert.Equal("Gas", row[1]);
        Assert.Equal("Fuel for vehicle", row[2]);
        Assert.Equal("45.67", row[3]);
        Assert.Equal("Transportation", row[4]);
    }
    
    #endregion

    #region Sheet Configuration Tests (Core Structure)
    
    [Fact]
    public void GetSheet_ShouldReturnCorrectSheetConfiguration()
    {
        // Act
        var result = GenericSheetMapper<ExpenseEntity>.GetSheet(SheetsConfig.ExpenseSheet);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Expenses", result.Name);
        Assert.NotNull(result.Headers);
        Assert.True(result.Headers.Count >= 5, "Should have at least basic expense fields");
        
        // Verify essential headers exist with proper formatting
        var dateHeader = result.Headers.FirstOrDefault(h => h.Name == HeaderEnum.DATE.GetDescription());
        Assert.NotNull(dateHeader);
        Assert.Equal(FormatEnum.DATE, dateHeader.Format);
        
        var amountHeader = result.Headers.FirstOrDefault(h => h.Name == HeaderEnum.AMOUNT.GetDescription());
        Assert.NotNull(amountHeader);
        Assert.Equal(FormatEnum.ACCOUNTING, amountHeader.Format);
        
        // Verify all headers have proper column assignments
        Assert.All(result.Headers, header => 
        {
            Assert.True(header.Index >= 0, "All headers should have valid indexes");
            Assert.False(string.IsNullOrEmpty(header.Column), "All headers should have column letters");
        });
    }
    
    #endregion

    #region Basic Validation Tests (High-Level Only)
    
    [Theory]
    [InlineData("Date", "2024-01-15")]
    [InlineData("Name", "Test Expense")]
    [InlineData("Amount", "25.50")]
    public void MapFromRangeData_WithSingleColumn_ShouldMapCorrectly(string headerName, string testValue)
    {
        // Arrange
        var values = new List<IList<object>>
        {
            new List<object> { headerName },
            new List<object> { testValue }
        };

        // Act
        var result = GenericSheetMapper<ExpenseEntity>.MapFromRangeData(values);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        
        var expense = result[0];
        switch (headerName)
        {
            case "Date":
                Assert.Equal(testValue, expense.Date);  // Now string comparison
                break;
            case "Name":
                Assert.Equal(testValue, expense.Name);
                break;
            case "Amount":
                Assert.Equal(25.50m, expense.Amount);
                break;
        }
    }

    [Fact]
    public void MapToRowData_WithValidExpenses_ShouldReturnRowData()
    {
        // Arrange
        var expenses = new List<ExpenseEntity>
        {
            new()
            {
                Date = "2024-01-15",  // Now string, not DateTime
                Name = "Gas",
                Amount = 45.67m
            }
        };
        var headers = new List<object> { "Date", "Name", "Amount" };

        // Act
        var result = GenericSheetMapper<ExpenseEntity>.MapToRowData(expenses, headers);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        
        var rowData = result[0];
        Assert.Equal(3, rowData.Values.Count);
        
        // Basic validation - should have proper cell values
        Assert.NotNull(rowData.Values[0].UserEnteredValue); // Date
        Assert.Equal("Gas", rowData.Values[1].UserEnteredValue.StringValue); // Name
        Assert.Equal(45.67, rowData.Values[2].UserEnteredValue.NumberValue); // Amount
    }
    
    #endregion
}
