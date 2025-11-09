using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Sheets.v4.Data;
using Moq;
using RaptorSheets.Core.Repositories;
using RaptorSheets.Core.Services;
using Xunit;

namespace RaptorSheets.Core.Tests.Repositories
{
    public class BaseEntityRepositoryTests
    {
        private readonly Mock<IGoogleSheetService> _mockSheetService;
        private readonly TestEntityRepository _repository;

        public BaseEntityRepositoryTests()
        {
            _mockSheetService = new Mock<IGoogleSheetService>();
            _repository = new TestEntityRepository(_mockSheetService.Object, "TestSheet");
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnEmptyList_WhenSheetDataIsNull()
        {
            // Arrange
            _mockSheetService.Setup(s => s.GetSheetData("TestSheet")).ReturnsAsync((ValueRange)null);

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task AddAsync_ShouldReturnTrue_WhenAppendDataIsSuccessful()
        {
            // Arrange
            var entity = new TestEntity { Id = 1, Name = "Test" };
            _mockSheetService.Setup(s => s.AppendData(It.IsAny<ValueRange>(), "TestSheet!A:Z"))
                .ReturnsAsync(new AppendValuesResponse());

            // Act
            var result = await _repository.AddAsync(entity);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task AddRangeAsync_ShouldReturnTrue_WhenAppendDataIsSuccessful()
        {
            // Arrange
            var entities = new List<TestEntity>
            {
                new TestEntity { Id = 1, Name = "Test1" },
                new TestEntity { Id = 2, Name = "Test2" }
            };
            _mockSheetService.Setup(s => s.AppendData(It.IsAny<ValueRange>(), "TestSheet!A:Z"))
                .ReturnsAsync(new AppendValuesResponse());

            // Act
            var result = await _repository.AddRangeAsync(entities);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnTrue_WhenUpdateDataIsSuccessful()
        {
            // Arrange
            var entity = new TestEntity { Id = 1, Name = "Updated" };
            _mockSheetService.Setup(s => s.UpdateData(It.IsAny<ValueRange>(), It.IsAny<string>()))
                .ReturnsAsync(new UpdateValuesResponse());

            // Act
            var result = await _repository.UpdateAsync(entity, 1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnTrue_WhenUpdateDataIsSuccessful()
        {
            // Arrange
            _mockSheetService.Setup(s => s.UpdateData(It.IsAny<ValueRange>(), It.IsAny<string>()))
                .ReturnsAsync(new UpdateValuesResponse());

            // Act
            var result = await _repository.DeleteAsync(1);

            // Assert
            Assert.True(result);
        }

        private class TestEntityRepository : BaseEntityRepository<TestEntity>
        {
            public TestEntityRepository(IGoogleSheetService sheetService, string sheetName)
                : base(sheetService, sheetName)
            {
            }
        }

        private class TestEntity
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }
    }
}