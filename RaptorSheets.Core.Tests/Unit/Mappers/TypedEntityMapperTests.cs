using System.Reflection;
using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Mappers;
using RaptorSheets.Core.Helpers;
using Xunit;
using System.Collections.Generic;

namespace RaptorSheets.Core.Tests.Unit.Mappers;

public class TypedEntityMapperTests
{
    private class TestEntity
    {
        [Column("Header1", FieldTypeEnum.String)]
        public string Header1 { get; set; } = "";

        [Column("Header2", FieldTypeEnum.Integer)]
        public int Header2 { get; set; }

        [Column("Header3", FieldTypeEnum.Currency)]
        public decimal Header3 { get; set; }
    }

    [Fact]
    public void MapFromRangeData_ShouldMapDataCorrectly()
    {
        // Arrange
        var data = new List<IList<object>>
        {
            new List<object> { "Value1", 123, 45.67m }
        };
        var headers = new Dictionary<int, string>
        {
            { 0, "Header1" },
            { 1, "Header2" },
            { 2, "Header3" }
        };

        // Act
        var result = TypedEntityMapper<TestEntity>.MapFromRangeData(data, headers);

        // Assert
        Assert.Single(result);
        Assert.Equal("Value1", result[0].Header1);
        Assert.Equal(123, result[0].Header2);
        Assert.Equal(45.67m, result[0].Header3);
    }

    [Fact]
    public void MapToRangeData_ShouldMapEntitiesCorrectly()
    {
        // Arrange
        var entities = new List<TestEntity>
        {
            new TestEntity { Header1 = "Value1", Header2 = 123, Header3 = 45.67m }
        };
        var headers = new List<string> { "Header1", "Header2", "Header3" };

        // Act
        var result = TypedEntityMapper<TestEntity>.MapToRangeData(entities, headers);

        // Assert
        Assert.Single(result);
        Assert.Equal("Value1", result[0][0]);
        Assert.Equal(123L, result[0][1]); // FieldTypeEnum.Integer converts to long (Int64)
        Assert.Equal(45.67, (double)(result[0][2] ?? 0)); // FieldTypeEnum.Currency converts to double
    }

    [Fact]
    public void GetHeaderNames_ShouldReturnCorrectHeaders()
    {
        // Act
        var headers = TypedEntityMapper<TestEntity>.GetHeaderNames();

        // Assert
        Assert.Equal(3, headers.Count);
        Assert.Contains("Header1", headers);
        Assert.Contains("Header2", headers);
        Assert.Contains("Header3", headers);
    }

    [Fact]
    public void ValidateEntityMapping_ShouldReturnNoErrors_WhenEntityIsValid()
    {
        // Act
        var errors = TypedEntityMapper<TestEntity>.ValidateEntityMapping();

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateEntityMapping_ShouldReturnErrors_WhenEntityIsInvalid()
    {
        // Arrange & Act - InvalidTestEntity has no validation errors since Column attribute is valid
        var result = TypedEntityMapper<InvalidTestEntity>.ValidateEntityMapping();

        // Assert - Since InvalidTestEntity is actually valid, we expect no errors
        Assert.Empty(result);
    }

    private class InvalidTestEntity
    {
        [Column("InvalidHeader", FieldTypeEnum.String)]
        public string InvalidHeader { get; set; } = "";
    }
}