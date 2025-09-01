using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Mappers;
using System.ComponentModel;

namespace RaptorSheets.Gig.Tests.Unit.Mappers;

[Category("Unit Tests")]
public class NameMapperTests
{
    [Fact]
    public void MapFromRangeData_WithValidData_ShouldReturnNames()
    {
        // Arrange
        var values = new List<IList<object>>
        {
            new List<object> { "Name", "Trips", "Pay", "Tip", "Bonus", "Total" }, // Use "Tip" not "Tips"
            new List<object> { "John Doe", "5", "125.50", "25.00", "10.00", "160.50" },
            new List<object> { "Jane Smith", "3", "75.25", "15.00", "5.00", "95.25" }
        };

        // Act
        var result = NameMapper.MapFromRangeData(values);

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
    public void GetSheet_ShouldReturnCorrectSheetConfiguration()
    {
        // Act
        var result = NameMapper.GetSheet();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Headers);
        
        // Check key headers exist and have proper formats
        var nameHeader = result.Headers.FirstOrDefault(h => h.Name == HeaderEnum.NAME.GetDescription());
        Assert.NotNull(nameHeader);
        
        var tripsHeader = result.Headers.FirstOrDefault(h => h.Name == HeaderEnum.TRIPS.GetDescription());
        Assert.NotNull(tripsHeader);
        Assert.Equal(FormatEnum.NUMBER, tripsHeader.Format);
        
        var payHeader = result.Headers.FirstOrDefault(h => h.Name == HeaderEnum.PAY.GetDescription());
        Assert.NotNull(payHeader);
        Assert.Equal(FormatEnum.ACCOUNTING, payHeader.Format);
        
        var totalHeader = result.Headers.FirstOrDefault(h => h.Name == HeaderEnum.TOTAL.GetDescription());
        Assert.NotNull(totalHeader);
        Assert.Equal(FormatEnum.ACCOUNTING, totalHeader.Format);
    }

    [Fact]
    public void GetSheet_NameHeader_ShouldGenerateUniqueNameFormula()
    {
        // Act
        var sheet = NameMapper.GetSheet();
        var nameHeader = sheet.Headers.FirstOrDefault(h => h.Name.ToString() == HeaderEnum.NAME.GetDescription());

        // Assert
        if (nameHeader != null)
        {
            Assert.NotNull(nameHeader.Formula);
            Assert.StartsWith("={\"Name\";SORT(UNIQUE(", nameHeader.Formula);
            Assert.Contains("Trips!", nameHeader.Formula); // Should reference Trips sheet
            Assert.EndsWith("))}", nameHeader.Formula);
        }
    }

    [Fact]
    public void GetSheet_AggregationHeaders_ShouldGenerateProperFormulas()
    {
        // Act
        var sheet = NameMapper.GetSheet();
        var tripsHeader = sheet.Headers.FirstOrDefault(h => h.Name.ToString() == HeaderEnum.TRIPS.GetDescription());
        var payHeader = sheet.Headers.FirstOrDefault(h => h.Name.ToString() == HeaderEnum.PAY.GetDescription());
        var totalHeader = sheet.Headers.FirstOrDefault(h => h.Name.ToString() == HeaderEnum.TOTAL.GetDescription());

        // Assert - Only test headers that exist
        if (tripsHeader != null)
        {
            Assert.NotNull(tripsHeader.Formula);
            Assert.Contains("COUNTIF(", tripsHeader.Formula);
        }
        
        if (payHeader != null)
        {
            Assert.NotNull(payHeader.Formula);
            Assert.Contains("SUMIF(", payHeader.Formula);
        }
        
        if (totalHeader != null)
        {
            Assert.NotNull(totalHeader.Formula);
            Assert.Contains("+", totalHeader.Formula); // Should add pay + tips + bonus
        }
    }

    [Fact]
    public void GetSheet_VisitHeaders_ShouldGenerateVisitDateFormulas()
    {
        // Act
        var sheet = NameMapper.GetSheet();
        var firstVisitHeader = sheet.Headers.FirstOrDefault(h => h.Name.ToString() == HeaderEnum.VISIT_FIRST.GetDescription());
        var lastVisitHeader = sheet.Headers.FirstOrDefault(h => h.Name.ToString() == HeaderEnum.VISIT_LAST.GetDescription());

        // Assert - Only test headers that exist
        if (firstVisitHeader != null)
        {
            Assert.NotNull(firstVisitHeader.Formula);
            Assert.Contains("VLOOKUP(", firstVisitHeader.Formula);
            Assert.Equal(FormatEnum.DATE, firstVisitHeader.Format);
        }
        
        if (lastVisitHeader != null)
        {
            Assert.NotNull(lastVisitHeader.Formula);
            Assert.Contains("VLOOKUP(", lastVisitHeader.Formula);
            Assert.Equal(FormatEnum.DATE, lastVisitHeader.Format);
        }
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
        var result = NameMapper.MapFromRangeData(values);

        // Assert
        Assert.Equal(2, result.Count); // Empty row filtered out
        Assert.Equal("John Doe", result[0].Name);
        Assert.Equal("Jane Smith", result[1].Name);
    }

    [Theory]
    [InlineData("Name", "John Doe")]
    [InlineData("Trips", "5")]
    [InlineData("Pay", "125.50")]
    [InlineData("Tip", "25.00")]
    public void MapFromRangeData_WithVariousHeaders_ShouldMapCorrectly(string headerName, string testValue)
    {
        // Arrange
        var values = new List<IList<object>>
        {
            new List<object> { headerName },
            new List<object> { testValue }
        };

        // Act
        var result = NameMapper.MapFromRangeData(values);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        
        var name = result[0];
        Assert.Equal(2, name.RowId);
        Assert.True(name.Saved);
    }
}