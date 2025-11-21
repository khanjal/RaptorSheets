using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Mappers;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Mappers;
using System.ComponentModel;

namespace RaptorSheets.Gig.Tests.Unit.Mappers;

[Category("Unit Tests")]
public class NameMapperTests
{
    #region Core Mapping Tests (Essential)
    
    [Fact]
    public void MapFromRangeData_WithValidData_ShouldReturnNames()
    {
        // Arrange
        var values = new List<IList<object>>
        {
            new List<object> { "Name", "Trips", "Pay", "Tips", "Bonus", "Total" },  // Changed "Tip" to "Tips"
            new List<object> { "John Doe", "5", "125.50", "25.00", "10.00", "160.50" },
            new List<object> { "Jane Smith", "3", "75.25", "15.00", "5.00", "95.25" }
        };

        // Act
        var result = GenericSheetMapper<NameEntity>.MapFromRangeData(values);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        
        var firstName = result[0];
        Assert.Equal(2, firstName.RowId);
        Assert.Equal("John Doe", firstName.Name);
        Assert.Equal(5, firstName.Trips);
        Assert.Equal(125.50m, firstName.Pay);
        Assert.Equal(25.00m, firstName.Tip);
        Assert.Equal(10.00m, firstName.Bonus);
        Assert.Equal(160.50m, firstName.Total);
        Assert.True(firstName.Saved);
        
        var secondName = result[1];
        Assert.Equal("Jane Smith", secondName.Name);
        Assert.Equal(3, secondName.Trips);
    }

    [Fact]
    public void MapFromRangeData_WithEmptyRows_ShouldFilterOutEmptyRows()
    {
        // Arrange
        var values = new List<IList<object>>
        {
            new List<object> { "Name", "Trips" },
            new List<object> { "John Doe", "5" },
            new List<object> { "", "" }, // Empty row
            new List<object> { "Jane Smith", "3" }
        };

        // Act
        var result = GenericSheetMapper<NameEntity>.MapFromRangeData(values);

        // Assert
        Assert.Equal(2, result.Count); // Empty row filtered out
        Assert.Equal("John Doe", result[0].Name);
        Assert.Equal("Jane Smith", result[1].Name);
    }
    
    #endregion

    #region Sheet Configuration Tests (Core Structure)
    
    [Fact]
    public void GetSheet_ShouldReturnCorrectSheetConfiguration()
    {
        // Act
        var result = NameMapper.GetSheet();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Names", result.Name);
        Assert.NotNull(result.Headers);
        Assert.True(result.Headers.Count >= 5, "Should have basic name aggregation fields");
        
        // Verify essential headers exist with proper formatting
        var nameHeader = result.Headers.FirstOrDefault(h => h.Name == HeaderEnum.NAME.GetDescription());
        Assert.NotNull(nameHeader);
        
        var tripsHeader = result.Headers.FirstOrDefault(h => h.Name == HeaderEnum.TRIPS.GetDescription());
        Assert.NotNull(tripsHeader);
        Assert.Equal(FormatEnum.NUMBER, tripsHeader.Format);
        
        var payHeader = result.Headers.FirstOrDefault(h => h.Name == HeaderEnum.PAY.GetDescription());
        Assert.NotNull(payHeader);
        // MapperFormulaHelper sets ACCOUNTING format for aggregation sheets
        Assert.Equal(FormatEnum.ACCOUNTING, payHeader.Format);
        
        // Verify all headers have proper column assignments
        Assert.All(result.Headers, header => 
        {
            Assert.True(header.Index >= 0, "All headers should have valid indexes");
            Assert.False(string.IsNullOrEmpty(header.Column), "All headers should have column letters");
        });
    }
    
    #endregion

    #region High-Level Formula Validation (No Implementation Details)
    
    [Fact]
    public void GetSheet_ShouldGenerateValidFormulas()
    {
        // Act
        var sheet = NameMapper.GetSheet();
        var formulaHeaders = sheet.Headers.Where(h => !string.IsNullOrEmpty(h.Formula)).ToList();

        // Assert - High-level validation only (don't test formula internals)
        if (formulaHeaders.Any())
        {
            // All formulas should start with =
            Assert.All(formulaHeaders, header => Assert.StartsWith("=", header.Formula));
            
            // Should not have unresolved placeholders
            Assert.All(formulaHeaders, header => 
            {
                Assert.DoesNotContain("{keyRange}", header.Formula);
                Assert.DoesNotContain("{header}", header.Formula);
            });
        }
    }

    [Theory]
    [InlineData("Name", "John Doe")]   // Representative test cases only
    [InlineData("Trips", "5")]
    [InlineData("Pay", "125.50")]
    public void MapFromRangeData_WithSingleColumn_ShouldMapCorrectly(string headerName, string testValue)
    {
        // Arrange
        var values = new List<IList<object>>
        {
            new List<object> { headerName },
            new List<object> { testValue }
        };

        // Act
        var result = GenericSheetMapper<NameEntity>.MapFromRangeData(values);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        
        var name = result[0];
        Assert.Equal(2, name.RowId);
        Assert.True(name.Saved);
        
        switch (headerName)
        {
            case "Name":
                Assert.Equal(testValue, name.Name);
                break;
            case "Trips":
                Assert.Equal(5, name.Trips);
                break;
            case "Pay":
                Assert.Equal(125.50m, name.Pay);
                break;
        }
    }
    
    #endregion
}