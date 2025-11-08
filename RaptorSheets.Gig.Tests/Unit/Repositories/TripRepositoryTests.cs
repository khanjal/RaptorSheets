using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using RaptorSheets.Gig.Repositories;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Core.Services;

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

        [Fact]
        public async Task GetTripsByDateRangeAsync_ReturnsTripsWithinRange()
        {
            // Arrange
            var trips = new List<TripEntity>
            {
                new TripEntity { Date = "2025-11-01", Pay = 100 },
                new TripEntity { Date = "2025-11-05", Pay = 200 },
                new TripEntity { Date = "2025-11-10", Pay = 300 }
            };
            _mockSheetService.Setup(s => s.GetAllAsync<TripEntity>()).ReturnsAsync(trips);

            // Act
            var result = await _repository.GetTripsByDateRangeAsync(DateTime.Parse("2025-11-01"), DateTime.Parse("2025-11-05"));

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetTripsByServiceAsync_ReturnsTripsForService()
        {
            // Arrange
            var trips = new List<TripEntity>
            {
                new TripEntity { Service = "Uber" },
                new TripEntity { Service = "Lyft" },
                new TripEntity { Service = "Uber" }
            };
            _mockSheetService.Setup(s => s.GetAllAsync<TripEntity>()).ReturnsAsync(trips);

            // Act
            var result = await _repository.GetTripsByServiceAsync("Uber");

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetTotalEarningsAsync_CalculatesCorrectTotal()
        {
            // Arrange
            var trips = new List<TripEntity>
            {
                new TripEntity { Date = "2025-11-01", Pay = 100, Tip = 20, Bonus = 10 },
                new TripEntity { Date = "2025-11-05", Pay = 200, Tip = 30, Bonus = 20 }
            };
            _mockSheetService.Setup(s => s.GetAllAsync<TripEntity>()).ReturnsAsync(trips);

            // Act
            var result = await _repository.GetTotalEarningsAsync(DateTime.Parse("2025-11-01"), DateTime.Parse("2025-11-05"));

            // Assert
            Assert.Equal(380, result);
        }
    }
}