using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Mappers;
using Xunit;

namespace RaptorSheets.Core.Tests.Unit.Mappers;

/// <summary>
/// Tests to verify that output columns (formulas) maintain their position with null values
/// when mapping to Google Sheets data formats
/// </summary>
public class GenericSheetMapperOutputColumnTests
{
    // Test entity with mixed input/output columns
    private class TestEntity
    {
        [Column("Input1", isInput: true)]
        public string Input1 { get; set; } = "";

        [Column("Input2", isInput: true)]
        public decimal? Input2 { get; set; }

        // Output column (formula) - should write null to preserve position
        [Column("Total")]
        public decimal? Total { get; set; }

        [Column("Input3", isInput: true)]
        public string Input3 { get; set; } = "";

        // Another output column after input
        [Column("Calculated")]
        public decimal? Calculated { get; set; }
    }

    [Fact]
    public void MapToRangeData_WithOutputColumns_ShouldMaintainPositionWithNull()
    {
        // Arrange
        var entities = new List<TestEntity>
        {
            new()
            {
                Input1 = "Test1",
                Input2 = 10.50m,
                Total = 15.75m, // This is an output column, should be null in output
                Input3 = "Test3",
                Calculated = 5.25m // This is an output column, should be null in output
            }
        };
        var headers = new List<object> { "Input1", "Input2", "Total", "Input3", "Calculated" };

        // Act
        var result = GenericSheetMapper<TestEntity>.MapToRangeData(entities, headers);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);

        var row = result[0];
        Assert.Equal(5, row.Count); // Should have 5 positions

        // Input columns should have values
        Assert.Equal("Test1", row[0]);
        Assert.Equal("10.50", row[1]);

        // Output column should be null (position 2) - MapToRowData preserves formulas with null
        Assert.Null(row[2]);

        // Input column should have value
        Assert.Equal("Test3", row[3]);

        // Output column should be null (position 4) - MapToRowData preserves formulas with null
        Assert.Null(row[4]);
    }

    [Fact]
    public void MapToRowData_WithOutputColumns_ShouldMaintainPositionWithEmptyCellData()
    {
        // Arrange
        var entities = new List<TestEntity>
        {
            new()
            {
                Input1 = "Test1",
                Input2 = 10.50m,
                Total = 15.75m, // Output column
                Input3 = "Test3",
                Calculated = 5.25m // Output column
            }
        };
        var headers = new List<object> { "Input1", "Input2", "Total", "Input3", "Calculated" };

        // Act
        var result = GenericSheetMapper<TestEntity>.MapToRowData(entities, headers);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);

        var row = result[0];
        Assert.NotNull(row.Values);
        Assert.Equal(5, row.Values.Count); // Should have 5 positions

        // Input columns should have cell data with values
        Assert.NotNull(row.Values[0]);
        Assert.Equal("Test1", row.Values[0].UserEnteredValue?.StringValue);

        Assert.NotNull(row.Values[1]);
        Assert.Equal(10.50, row.Values[1].UserEnteredValue?.NumberValue);

        // Output column should be empty CellData (position 2) to preserve position
        Assert.NotNull(row.Values[2]); // Not null - should be empty CellData
        Assert.Null(row.Values[2].UserEnteredValue); // But no value

        // Input column should have value
        Assert.NotNull(row.Values[3]);
        Assert.Equal("Test3", row.Values[3].UserEnteredValue?.StringValue);

        // Output column should be empty CellData (position 4) to preserve position  
        Assert.NotNull(row.Values[4]); // Not null - should be empty CellData
        Assert.Null(row.Values[4].UserEnteredValue); // But no value
    }

    [Fact]
    public void MapToRangeData_WithOnlyOutputColumns_ShouldReturnAllNulls()
    {
        // Arrange - Entity with only output columns
        var entities = new List<TestEntity>
        {
            new()
            {
                Total = 15.75m,
                Calculated = 5.25m
            }
        };
        var headers = new List<object> { "Total", "Calculated" };

        // Act
        var result = GenericSheetMapper<TestEntity>.MapToRangeData(entities, headers);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);

        var row = result[0];
        Assert.Equal(2, row.Count);
        Assert.All(row, value => Assert.Null(value));
    }

    [Fact]
    public void MapToRangeData_WithUnknownColumn_ShouldWriteNull()
    {
        // Arrange
        var entities = new List<TestEntity>
        {
            new()
            {
                Input1 = "Test1",
                Input2 = 10.50m
            }
        };
        var headers = new List<object> { "Input1", "UnknownColumn", "Input2" };

        // Act
        var result = GenericSheetMapper<TestEntity>.MapToRangeData(entities, headers);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);

        var row = result[0];
        Assert.Equal(3, row.Count);

        Assert.Equal("Test1", row[0]);
        Assert.Null(row[1]); // Unknown column should be null
        Assert.Equal("10.50", row[2]);
    }

    [Fact]
    public void MapToRangeData_WithMultipleEntities_ShouldMaintainOutputColumnPositions()
    {
        // Arrange
        var entities = new List<TestEntity>
        {
            new() { Input1 = "A1", Input2 = 10m, Total = 100m, Input3 = "A3", Calculated = 50m },
            new() { Input1 = "B1", Input2 = 20m, Total = 200m, Input3 = "B3", Calculated = 60m },
            new() { Input1 = "C1", Input2 = 30m, Total = 300m, Input3 = "C3", Calculated = 70m }
        };
        var headers = new List<object> { "Input1", "Input2", "Total", "Input3", "Calculated" };

        // Act
        var result = GenericSheetMapper<TestEntity>.MapToRangeData(entities, headers);

        // Assert
        Assert.Equal(3, result.Count);

        foreach (var row in result)
        {
            Assert.Equal(5, row.Count);
            Assert.NotNull(row[0]); // Input1
            Assert.NotNull(row[1]); // Input2
            Assert.Null(row[2]);    // Total (output)
            Assert.NotNull(row[3]); // Input3
            Assert.Null(row[4]);    // Calculated (output)
        }
    }
}
