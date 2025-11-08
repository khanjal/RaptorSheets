using RaptorSheets.Common.Enums;
using RaptorSheets.Common.Mappers;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using Xunit;
using System.Collections.Generic;

namespace RaptorSheets.Test.Unit.Mappers
{
    public class SetupMapperTests
    {
        [Fact]
        public void MapFromRangeData_ValidInput_ReturnsSetupEntities()
        {
            // Arrange
            var values = new List<IList<object>>
            {
                new List<object> { "Name", "Value" },
                new List<object> { "TestName1", "TestValue1" },
                new List<object> { "TestName2", "TestValue2" }
            };

            // Act
            var result = SetupMapper.MapFromRangeData(values);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("TestName1", result[0].Name);
            Assert.Equal("TestValue1", result[0].Value);
            Assert.Equal("TestName2", result[1].Name);
            Assert.Equal("TestValue2", result[1].Value);
        }

        [Fact]
        public void MapToRangeData_ValidInput_ReturnsRangeData()
        {
            // Arrange
            var setupEntities = new List<SetupEntity>
            {
                new SetupEntity { Name = "TestName1", Value = "TestValue1" },
                new SetupEntity { Name = "TestName2", Value = "TestValue2" }
            };

            var headers = new List<object> { HeaderEnum.NAME.ToString(), HeaderEnum.VALUE.ToString() };

            // Act
            var result = SetupMapper.MapToRangeData(setupEntities, headers);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("TestName1", result[0][0]);
            Assert.Equal("TestValue1", result[0][1]);
            Assert.Equal("TestName2", result[1][0]);
            Assert.Equal("TestValue2", result[1][1]);
        }

        [Fact]
        public void MapToRowData_ValidInput_ReturnsRowData()
        {
            // Arrange
            var setupEntities = new List<SetupEntity>
            {
                new SetupEntity { Name = "TestName1", Value = "TestValue1" },
                new SetupEntity { Name = "TestName2", Value = "TestValue2" }
            };

            var headers = new List<object> { HeaderEnum.NAME.ToString(), HeaderEnum.VALUE.ToString() };

            // Act
            var result = SetupMapper.MapToRowData(setupEntities, headers);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("TestName1", result[0].Values[0].UserEnteredValue.StringValue);
            Assert.Equal("TestValue1", result[0].Values[1].UserEnteredValue.StringValue);
            Assert.Equal("TestName2", result[1].Values[0].UserEnteredValue.StringValue);
            Assert.Equal("TestValue2", result[1].Values[1].UserEnteredValue.StringValue);
        }
    }
}