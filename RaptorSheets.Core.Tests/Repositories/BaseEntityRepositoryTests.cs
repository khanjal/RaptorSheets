using Google.Apis.Sheets.v4.Data;
using Moq;
using RaptorSheets.Core.Repositories;
using RaptorSheets.Core.Services;
using RaptorSheets.Core.Models;
using Xunit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            _mockSheetService.Setup(s => s.GetSheetData("TestSheet")).ReturnsAsync((ValueRange?)null);

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

        [Fact]
        public async Task GetAllAsync_ShouldReturnEmptyList_WhenSheetHasOnlyHeaderRow()
        {
            // Arrange
            var valueRange = new ValueRange { Values = new List<IList<object>> { new List<object> { "Header1", "Header2" } } };
            _mockSheetService.Setup(s => s.GetSheetData("TestSheet")).ReturnsAsync(valueRange);

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnData_WhenHasHeaderRowIsFalse()
        {
            // Arrange
            var repositoryWithoutHeader = new TestEntityRepository(_mockSheetService.Object, "TestSheet", false);
            var valueRange = new ValueRange { Values = new List<IList<object>> { new List<object> { 1, "Test" } } };
            _mockSheetService.Setup(s => s.GetSheetData("TestSheet")).ReturnsAsync(valueRange);

            // Act
            var result = await repositoryWithoutHeader.GetAllAsync();

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public async Task AddAsync_ShouldThrowArgumentNullException_WhenEntityIsNull()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.AddAsync(null));
        }

        [Fact]
        public async Task AddRangeAsync_ShouldReturnTrue_WhenEntitiesCollectionIsEmpty()
        {
            // Act
            var result = await _repository.AddRangeAsync(new List<TestEntity>());

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowArgumentOutOfRangeException_WhenRowIndexIsLessThanOne()
        {
            // Arrange
            var entity = new TestEntity { Id = 1, Name = "Test" };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _repository.UpdateAsync(entity, 0));
        }

        [Fact]
        public async Task DeleteAsync_ShouldThrowArgumentOutOfRangeException_WhenRowIndexIsLessThanOne()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _repository.DeleteAsync(0));
        }

        [Fact]
        public async Task ValidateSchemaAsync_ShouldReturnInvalidResult_WhenHasHeaderRowIsFalse()
        {
            // Arrange
            var repositoryWithoutHeader = new TestEntityRepository(_mockSheetService.Object, "TestSheet", false);

            // Act
            var result = await repositoryWithoutHeader.ValidateSchemaAsync();

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("Schema validation requires a header row", result.Errors);
        }

        [Fact]
        public async Task ValidateSchemaAsync_ShouldReturnInvalidResult_WhenSheetDataIsEmpty()
        {
            // Arrange
            _mockSheetService.Setup(s => s.GetSheetData("TestSheet")).ReturnsAsync(new ValueRange());

            // Act
            var result = await _repository.ValidateSchemaAsync();

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("Sheet is empty or cannot be accessed", result.Errors);
        }

        [Fact]
        public async Task InitializeSheetAsync_ShouldReturnTrue_WhenSheetAlreadyHasData()
        {
            // Arrange
            var valueRange = new ValueRange { Values = new List<IList<object>> { new List<object> { "Header1", "Header2" } } };
            _mockSheetService.Setup(s => s.GetSheetData("TestSheet")).ReturnsAsync(valueRange);

            // Act
            var result = await _repository.InitializeSheetAsync();

            // Assert
            Assert.True(result);
        }

        private class TestEntityRepository : BaseEntityRepository<TestEntity>
        {
            public TestEntityRepository(IGoogleSheetService sheetService, string sheetName, bool hasHeaderRow = true)
                : base(sheetService, sheetName, hasHeaderRow)
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