using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
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
    }
}