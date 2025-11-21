using Moq;
using RaptorSheets.Gig.Repositories;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Core.Services;
using Google.Apis.Sheets.v4.Data;

namespace RaptorSheets.Gig.Tests.Unit.Repositories
{
    public class TripRepositoryTests
    {
        private readonly Mock<IGoogleSheetService> _mockSheetService;
        private readonly TripRepository _repository;

        public TripRepositoryTests()
        {
            _mockSheetService = new Mock<IGoogleSheetService>();
            _repository = new TripRepository(_mockSheetService.Object);
        }

        #region GetTripsByDateRangeAsync Tests

        [Fact]
        public async Task GetTripsByDateRangeAsync_ReturnsTripsWithinRange()
        {
            // Arrange - Create header row and data rows with ALL string values
            // Important: Date field in TripEntity is a string, not DateTime
            var headerRow = (IList<object>)new List<object> { "Date", "Pay" };
            var dataRows = new List<IList<object>>
            {
                (IList<object>)new List<object> { "2025-11-01", "100" },  // All values must be strings or string-compatible
                (IList<object>)new List<object> { "2025-11-05", "200" },
                (IList<object>)new List<object> { "2025-11-10", "300" }
            };
            
            // Combine header and data rows
            var allRows = new List<IList<object>> { headerRow };
            allRows.AddRange(dataRows);
            
            _mockSheetService.Setup(s => s.GetSheetData("Trips"))
                .ReturnsAsync(new ValueRange { Values = allRows });

            // Act
            var result = await _repository.GetTripsByDateRangeAsync(DateTime.Parse("2025-11-01"), DateTime.Parse("2025-11-05"));

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetTripsByDateRangeAsync_WithInvalidDateRange_ThrowsArgumentException()
        {
            // Arrange
            var startDate = DateTime.Parse("2025-11-10");
            var endDate = DateTime.Parse("2025-11-01");

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _repository.GetTripsByDateRangeAsync(startDate, endDate));
        }

        [Fact]
        public async Task GetTripsByDateRangeAsync_WithNoTripsInRange_ReturnsEmptyList()
        {
            // Arrange
            var headerRow = (IList<object>)new List<object> { "Date", "Pay" };
            var dataRows = new List<IList<object>>
            {
                (IList<object>)new List<object> { "2025-10-01", "100" },
                (IList<object>)new List<object> { "2025-12-01", "200" }
            };
            
            var allRows = new List<IList<object>> { headerRow };
            allRows.AddRange(dataRows);
            
            _mockSheetService.Setup(s => s.GetSheetData("Trips"))
                .ReturnsAsync(new ValueRange { Values = allRows });

            // Act
            var result = await _repository.GetTripsByDateRangeAsync(
                DateTime.Parse("2025-11-01"), DateTime.Parse("2025-11-30"));

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region GetTripsByServiceAsync Tests

        [Fact]
        public async Task GetTripsByServiceAsync_ReturnsTripsForService()
        {
            // Arrange - Create header row and data rows
            var headerRow = (IList<object>)new List<object> { "Date", "Service" };  // Add Date column for proper mapping
            var dataRows = new List<IList<object>>
            {
                (IList<object>)new List<object> { "2025-11-01", "Uber" },
                (IList<object>)new List<object> { "2025-11-02", "Lyft" },
                (IList<object>)new List<object> { "2025-11-03", "Uber" }
            };
            
            // Combine header and data rows
            var allRows = new List<IList<object>> { headerRow };
            allRows.AddRange(dataRows);
            
            _mockSheetService.Setup(s => s.GetSheetData("Trips"))
                .ReturnsAsync(new ValueRange { Values = allRows });

            // Act
            var result = await _repository.GetTripsByServiceAsync("Uber");

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetTripsByServiceAsync_WithNullService_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _repository.GetTripsByServiceAsync(null!));
        }

        [Fact]
        public async Task GetTripsByServiceAsync_WithEmptyService_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _repository.GetTripsByServiceAsync(""));
        }

        [Fact]
        public async Task GetTripsByServiceAsync_WithWhitespaceService_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _repository.GetTripsByServiceAsync("   "));
        }

        #endregion

        #region GetTotalEarningsAsync Tests

        [Fact]
        public async Task GetTotalEarningsAsync_CalculatesCorrectTotal()
        {
            // Arrange - Create header row and data rows with ALL string values
            var headerRow = (IList<object>)new List<object> { "Date", "Pay", "Tips", "Bonus" };
            var dataRows = new List<IList<object>>
            {
                (IList<object>)new List<object> { "2025-11-01", "100", "20", "10" }, // All values as strings
                (IList<object>)new List<object> { "2025-11-05", "200", "30", "20" }
            };
            
            // Combine header and data rows
            var allRows = new List<IList<object>> { headerRow };
            allRows.AddRange(dataRows);
            
            _mockSheetService.Setup(s => s.GetSheetData("Trips"))
                .ReturnsAsync(new ValueRange { Values = allRows });

            // Act
            var result = await _repository.GetTotalEarningsAsync(DateTime.Parse("2025-11-01"), DateTime.Parse("2025-11-05"));

            // Assert
            Assert.Equal(380, result);
        }

        #endregion

        #region AddTripAsync Tests

        [Fact]
        public async Task AddTripAsync_WithNullTrip_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => 
                _repository.AddTripAsync(null!));
        }

        [Fact]
        public async Task AddTripAsync_GeneratesKeyWhenNotProvided()
        {
            // Arrange
            var trip = new TripEntity
            {
                Date = "2025-11-15",
                Service = "Uber",
                Number = 123,  // Changed from string to int
                Pay = 100
            };

            _mockSheetService.Setup(s => s.AppendData(It.IsAny<ValueRange>(), It.IsAny<string>()))
                .ReturnsAsync(new AppendValuesResponse());

            // Act
            var result = await _repository.AddTripAsync(trip);

            // Assert
            Assert.True(result);
            Assert.Equal("20251115_Uber_123", trip.Key);
        }

        [Fact]
        public async Task AddTripAsync_CalculatesTotalWhenNotProvided()
        {
            // Arrange
            var trip = new TripEntity
            {
                Date = "2025-11-15",
                Pay = 100,
                Tip = 20,
                Bonus = 10
            };

            _mockSheetService.Setup(s => s.AppendData(It.IsAny<ValueRange>(), It.IsAny<string>()))
                .ReturnsAsync(new AppendValuesResponse());

            // Act
            var result = await _repository.AddTripAsync(trip);

            // Assert
            Assert.True(result);
            Assert.Equal(130, trip.Total);
        }

        [Fact]
        public async Task AddTripAsync_ExtractsDateComponents()
        {
            // Arrange
            var trip = new TripEntity
            {
                Date = "2025-11-15",
                Service = "Uber",
                Pay = 100
            };

            _mockSheetService.Setup(s => s.AppendData(It.IsAny<ValueRange>(), It.IsAny<string>()))
                .ReturnsAsync(new AppendValuesResponse());

            // Act
            var result = await _repository.AddTripAsync(trip);

            // Assert
            Assert.True(result);
            Assert.Equal("15", trip.Day);
            Assert.Equal("11", trip.Month);
            Assert.Equal("2025", trip.Year);
        }

        [Fact]
        public async Task AddTripAsync_WithExistingKey_DoesNotOverwrite()
        {
            // Arrange
            var existingKey = "CUSTOM_KEY";
            var trip = new TripEntity
            {
                Key = existingKey,
                Date = "2025-11-15",
                Service = "Uber",
                Number = 123,  // Changed from string to int
                Pay = 100
            };

            _mockSheetService.Setup(s => s.AppendData(It.IsAny<ValueRange>(), It.IsAny<string>()))
                .ReturnsAsync(new AppendValuesResponse());

            // Act
            var result = await _repository.AddTripAsync(trip);

            // Assert
            Assert.True(result);
            Assert.Equal(existingKey, trip.Key);
        }

        [Fact]
        public async Task AddTripAsync_WithExistingTotal_DoesNotRecalculate()
        {
            // Arrange
            var trip = new TripEntity
            {
                Date = "2025-11-15",
                Pay = 100,
                Tip = 20,
                Bonus = 10,
                Total = 999 // Pre-existing total that shouldn't be changed
            };

            _mockSheetService.Setup(s => s.AppendData(It.IsAny<ValueRange>(), It.IsAny<string>()))
                .ReturnsAsync(new AppendValuesResponse());

            // Act
            var result = await _repository.AddTripAsync(trip);

            // Assert
            Assert.True(result);
            Assert.Equal(999, trip.Total);
        }

        #endregion

        #region UpdateTripAsync Tests

        [Fact]
        public async Task UpdateTripAsync_WithNullTrip_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => 
                _repository.UpdateTripAsync(null!, 1));
        }

        [Fact]
        public async Task UpdateTripAsync_RecalculatesTotal()
        {
            // Arrange
            var trip = new TripEntity
            {
                Date = "2025-11-15",
                Pay = 150,
                Tip = 30,
                Bonus = 20,
                Total = 999 // Should be recalculated
            };

            _mockSheetService.Setup(s => s.UpdateData(It.IsAny<ValueRange>(), It.IsAny<string>()))
                .ReturnsAsync(new UpdateValuesResponse());

            // Act
            var result = await _repository.UpdateTripAsync(trip, 1);

            // Assert
            Assert.True(result);
            Assert.Equal(200, trip.Total);
        }

        [Fact]
        public async Task UpdateTripAsync_UpdatesDateComponents()
        {
            // Arrange
            var trip = new TripEntity
            {
                Date = "2025-12-25",
                Pay = 100
            };

            _mockSheetService.Setup(s => s.UpdateData(It.IsAny<ValueRange>(), It.IsAny<string>()))
                .ReturnsAsync(new UpdateValuesResponse());

            // Act
            var result = await _repository.UpdateTripAsync(trip, 1);

            // Assert
            Assert.True(result);
            Assert.Equal("25", trip.Day);
            Assert.Equal("12", trip.Month);
            Assert.Equal("2025", trip.Year);
        }

        [Fact]
        public async Task UpdateTripAsync_WithInvalidDate_DoesNotUpdateDateComponents()
        {
            // Arrange
            var trip = new TripEntity
            {
                Date = "invalid-date",
                Pay = 100
            };

            _mockSheetService.Setup(s => s.UpdateData(It.IsAny<ValueRange>(), It.IsAny<string>()))
                .ReturnsAsync(new UpdateValuesResponse());

            // Act
            var result = await _repository.UpdateTripAsync(trip, 1);

            // Assert
            Assert.True(result);
            // Date components should remain empty strings (default values) when date parsing fails
            Assert.Equal("", trip.Day);
            Assert.Equal("", trip.Month);
            Assert.Equal("", trip.Year);
        }

        #endregion
    }
}