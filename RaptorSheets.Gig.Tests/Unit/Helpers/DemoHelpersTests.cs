using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Gig.Helpers;

namespace RaptorSheets.Gig.Tests.Unit.Helpers;

public class DemoHelpersTests
{
    [Fact]
    public void GenerateDemoData_WithSeed_ProducesDeterministicResults()
    {
        // Arrange
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 7);
        const int seed = 42;

        // Act
        var result1 = DemoHelpers.GenerateDemoData(startDate, endDate, seed);
        var result2 = DemoHelpers.GenerateDemoData(startDate, endDate, seed);

        // Assert - same seed should produce identical results
        Assert.Equal(result1.Shifts.Count, result2.Shifts.Count);
        Assert.Equal(result1.Trips.Count, result2.Trips.Count);
        Assert.Equal(result1.Expenses.Count, result2.Expenses.Count);

        // Verify first shift is identical
        if (result1.Shifts.Count > 0)
        {
            var shift1 = result1.Shifts[0];
            var shift2 = result2.Shifts[0];
            
            Assert.Equal(shift1.Date, shift2.Date);
            Assert.Equal(shift1.Service, shift2.Service);
            Assert.Equal(shift1.Pay, shift2.Pay);
            Assert.Equal(shift1.Tip, shift2.Tip);
            Assert.Equal(shift1.Distance, shift2.Distance);
        }

        // Verify first trip is identical
        if (result1.Trips.Count > 0)
        {
            var trip1 = result1.Trips[0];
            var trip2 = result2.Trips[0];
            
            Assert.Equal(trip1.Date, trip2.Date);
            Assert.Equal(trip1.Service, trip2.Service);
            Assert.Equal(trip1.Pay, trip2.Pay);
            Assert.Equal(trip1.Tip, trip2.Tip);
            Assert.Equal(trip1.Place, trip2.Place);
        }
    }

    [Fact]
    public void GenerateDemoData_WithoutSeed_ProducesNonDeterministicResults()
    {
        // Arrange
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 7);

        // Act
        var result1 = DemoHelpers.GenerateDemoData(startDate, endDate);
        var result2 = DemoHelpers.GenerateDemoData(startDate, endDate);

        // Assert - without seed, results should likely differ
        // Note: There's a tiny chance they could be identical, but extremely unlikely
        var identical = result1.Shifts.Count == result2.Shifts.Count &&
                       result1.Trips.Count == result2.Trips.Count &&
                       result1.Expenses.Count == result2.Expenses.Count;

        if (identical && result1.Shifts.Count > 0)
        {
            var shift1 = result1.Shifts[0];
            var shift2 = result2.Shifts[0];
            identical = shift1.Pay == shift2.Pay && shift1.Tip == shift2.Tip;
        }

        // Very likely to be different (not a strict assertion due to randomness)
        Assert.False(identical || result1.Shifts.Count == 0);
    }

    [Fact]
    public void GenerateDemoData_CreatesDataWithinDateRange()
    {
        // Arrange
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 10);
        const int seed = 123;

        // Act
        var result = DemoHelpers.GenerateDemoData(startDate, endDate, seed);

        // Assert - all data should be within the date range
        Assert.All(result.Shifts, shift =>
        {
            var shiftDate = DateTime.Parse(shift.Date);
            Assert.True(shiftDate >= startDate && shiftDate <= endDate);
        });

        Assert.All(result.Trips, trip =>
        {
            var tripDate = DateTime.Parse(trip.Date);
            Assert.True(tripDate >= startDate && tripDate <= endDate);
        });

        Assert.All(result.Expenses, expense =>
        {
            var expenseDate = DateTime.Parse(expense.Date);
            Assert.True(expenseDate >= startDate && expenseDate <= endDate);
        });
    }

    [Fact]
    public void GenerateDemoData_Shifts_HaveReasonablePayRanges()
    {
        // Arrange
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 31);
        const int seed = 456;

        // Act
        var result = DemoHelpers.GenerateDemoData(startDate, endDate, seed);

        // Assert - pay should be in reasonable range based on code ($15-$120)
        Assert.All(result.Shifts.Where(s => s.Pay.HasValue), shift =>
        {
            Assert.True(shift.Pay >= 15m && shift.Pay <= 120m,
                $"Shift pay {shift.Pay} is outside expected range $15-$120");
        });
    }

    [Fact]
    public void GenerateDemoData_Shifts_HaveReasonableTipRanges()
    {
        // Arrange
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 31);
        const int seed = 789;

        // Act
        var result = DemoHelpers.GenerateDemoData(startDate, endDate, seed);

        // Assert - tips should be in reasonable range ($5-$50 per code)
        Assert.All(result.Shifts.Where(s => s.Tip.HasValue), shift =>
        {
            Assert.True(shift.Tip >= 5m && shift.Tip <= 50m,
                $"Shift tip {shift.Tip} is outside expected range $5-$50");
        });
    }

    [Fact]
    public void GenerateDemoData_Shifts_HaveReasonableDistanceRanges()
    {
        // Arrange
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 31);
        const int seed = 101;

        // Act
        var result = DemoHelpers.GenerateDemoData(startDate, endDate, seed);

        // Assert - distance should be 10-80 miles per code
        Assert.All(result.Shifts.Where(s => s.Distance.HasValue), shift =>
        {
            Assert.True(shift.Distance >= 10m && shift.Distance <= 80m,
                $"Shift distance {shift.Distance} is outside expected range 10-80 miles");
        });
    }

    [Fact]
    public void GenerateDemoData_Trips_HaveReasonablePayRanges()
    {
        // Arrange
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 31);
        const int seed = 202;

        // Act
        var result = DemoHelpers.GenerateDemoData(startDate, endDate, seed);

        // Assert - trip pay should be reasonable ($2-$10.75 for delivery, up to $30 for others)
        Assert.All(result.Trips.Where(t => t.Pay.HasValue), trip =>
        {
            Assert.True(trip.Pay >= 2m && trip.Pay <= 35m,
                $"Trip pay {trip.Pay} is outside expected range $2-$35");
        });
    }

    [Fact]
    public void GenerateDemoData_Trips_HaveReasonableTipRanges()
    {
        // Arrange
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 31);
        const int seed = 303;

        // Act
        var result = DemoHelpers.GenerateDemoData(startDate, endDate, seed);

        // Assert - trip tips should be $0.50-$16.25 per code
        Assert.All(result.Trips.Where(t => t.Tip.HasValue), trip =>
        {
            Assert.True(trip.Tip >= 0.5m && trip.Tip <= 17m,
                $"Trip tip {trip.Tip} is outside expected range $0.50-$17");
        });
    }

    [Fact]
    public void GenerateDemoData_Trips_HaveReasonableDistanceRanges()
    {
        // Arrange
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 31);
        const int seed = 404;

        // Act
        var result = DemoHelpers.GenerateDemoData(startDate, endDate, seed);

        // Assert - trip distance should be 0.6-15.1 miles per code
        Assert.All(result.Trips.Where(t => t.Distance.HasValue), trip =>
        {
            Assert.True(trip.Distance >= 0.6m && trip.Distance <= 16m,
                $"Trip distance {trip.Distance} is outside expected range 0.6-16 miles");
        });
    }

    [Fact]
    public void GenerateDemoData_Trips_HaveReasonableDurations()
    {
        // Arrange
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 31);
        const int seed = 505;

        // Act
        var result = DemoHelpers.GenerateDemoData(startDate, endDate, seed);

        // Assert - trip duration should be 5-45 minutes per code
        Assert.All(result.Trips.Where(t => !string.IsNullOrEmpty(t.Duration)), trip =>
        {
            var duration = TimeSpan.Parse(trip.Duration);
            Assert.True(duration >= TimeSpan.FromMinutes(5) && duration <= TimeSpan.FromMinutes(45),
                $"Trip duration {duration} is outside expected range 5-45 minutes");
        });
    }

    [Fact]
    public void GenerateDemoData_Expenses_HaveReasonableAmounts()
    {
        // Arrange
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 31);
        const int seed = 606;

        // Act
        var result = DemoHelpers.GenerateDemoData(startDate, endDate, seed);

        // Assert - expense amounts should be reasonable by category
        Assert.All(result.Expenses.Where(e => e.Amount != 0), expense =>
        {
            var amount = expense.Amount;
            var maxExpected = expense.Category switch
            {
                "Fuel" => 55m,
                "Maintenance" => 200m,
                "Car Wash" => 18m,
                "Supplies" => 25m,
                "Parking" => 15m,
                "Tolls" => 8m,
                "Phone" => 80m,
                _ => 200m
            };

            Assert.True(amount > 0 && amount <= maxExpected,
                $"Expense amount {amount} for category {expense.Category} is outside expected range");
        });
    }

    [Fact]
    public void GenerateDemoData_TripsRelatedToShifts_HaveSameDateAndService()
    {
        // Arrange
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 7);
        const int seed = 707;

        // Act
        var result = DemoHelpers.GenerateDemoData(startDate, endDate, seed);

        // Assert - trips should match shifts by date, shift number, and service
        var shiftsGrouped = result.Shifts
            .GroupBy(s => (s.Date, s.Number, s.Service))
            .ToDictionary(g => g.Key, g => g.First());

        Assert.All(result.Trips, trip =>
        {
            var key = (trip.Date, trip.Number, trip.Service);
            Assert.True(shiftsGrouped.ContainsKey(key),
                $"Trip has no matching shift: Date={trip.Date}, Number={trip.Number}, Service={trip.Service}");
        });
    }

    [Fact]
    public void GenerateDemoData_RowIds_AreSequentialStartingAt2()
    {
        // Arrange
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 5);
        const int seed = 808;

        // Act
        var result = DemoHelpers.GenerateDemoData(startDate, endDate, seed);

        // Assert - RowIds should start at 2 (row 1 is headers) and be sequential
        if (result.Shifts.Count > 0)
        {
            Assert.Equal(2, result.Shifts[0].RowId);
            for (int i = 1; i < result.Shifts.Count; i++)
            {
                Assert.Equal(result.Shifts[i - 1].RowId + 1, result.Shifts[i].RowId);
            }
        }

        if (result.Trips.Count > 0)
        {
            Assert.Equal(2, result.Trips[0].RowId);
            for (int i = 1; i < result.Trips.Count; i++)
            {
                Assert.Equal(result.Trips[i - 1].RowId + 1, result.Trips[i].RowId);
            }
        }

        if (result.Expenses.Count > 0)
        {
            Assert.Equal(2, result.Expenses[0].RowId);
            for (int i = 1; i < result.Expenses.Count; i++)
            {
                Assert.Equal(result.Expenses[i - 1].RowId + 1, result.Expenses[i].RowId);
            }
        }
    }

    [Fact]
    public void GenerateDemoData_AllEntities_HaveInsertAction()
    {
        // Arrange
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 5);
        const int seed = 909;

        // Act
        var result = DemoHelpers.GenerateDemoData(startDate, endDate, seed);

        // Assert - all entities should have INSERT action
        Assert.All(result.Shifts, shift =>
        {
            Assert.Equal(ActionTypeEnum.INSERT.GetDescription(), shift.Action);
        });

        Assert.All(result.Trips, trip =>
        {
            Assert.Equal(ActionTypeEnum.INSERT.GetDescription(), trip.Action);
        });

        Assert.All(result.Expenses, expense =>
        {
            Assert.Equal(ActionTypeEnum.INSERT.GetDescription(), expense.Action);
        });
    }

    [Fact]
    public void GenerateDemoData_SingleDay_CreatesData()
    {
        // Arrange
        var date = new DateTime(2025, 1, 1);
        const int seed = 111;

        // Act
        var result = DemoHelpers.GenerateDemoData(date, date, seed);

        // Assert - should have at least some data for a single day
        Assert.NotNull(result);
        Assert.NotEmpty(result.Shifts);
    }

    [Fact]
    public void GenerateDemoData_LongerPeriod_CreatesMoreData()
    {
        // Arrange
        var shortStart = new DateTime(2025, 1, 1);
        var shortEnd = new DateTime(2025, 1, 3);
        var longStart = new DateTime(2025, 1, 1);
        var longEnd = new DateTime(2025, 1, 31);
        const int seed = 222;

        // Act
        var shortResult = DemoHelpers.GenerateDemoData(shortStart, shortEnd, seed);
        var longResult = DemoHelpers.GenerateDemoData(longStart, longEnd, seed);

        // Assert - longer period should have more data
        Assert.True(longResult.Shifts.Count > shortResult.Shifts.Count,
            "Longer period should generate more shifts");
        Assert.True(longResult.Trips.Count > shortResult.Trips.Count,
            "Longer period should generate more trips");
    }

    [Fact]
    public void GenerateDemoData_Trips_HaveValidServiceTypes()
    {
        // Arrange
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 7);
        const int seed = 333;
        var validServices = new[] { "DoorDash", "Uber Eats", "Grubhub", "Instacart", "Shipt" };

        // Act
        var result = DemoHelpers.GenerateDemoData(startDate, endDate, seed);

        // Assert - all trips should have valid service types
        Assert.All(result.Trips, trip =>
        {
            Assert.Contains(trip.Service, validServices);
        });
    }

    [Fact]
    public void GenerateDemoData_Trips_HaveValidTripTypes()
    {
        // Arrange
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 7);
        const int seed = 444;
        var validTypes = new[] { "Pickup", "Shop" };

        // Act
        var result = DemoHelpers.GenerateDemoData(startDate, endDate, seed);

        // Assert - all trips should have valid trip types
        Assert.All(result.Trips, trip =>
        {
            Assert.Contains(trip.Type, validTypes);
        });
    }

    [Fact]
    public void GenerateDemoData_Trips_HaveLocations()
    {
        // Arrange
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 7);
        const int seed = 555;

        // Act
        var result = DemoHelpers.GenerateDemoData(startDate, endDate, seed);

        // Assert - trips should have location data
        Assert.All(result.Trips, trip =>
        {
            Assert.False(string.IsNullOrWhiteSpace(trip.Place), "Trip should have a place");
            Assert.False(string.IsNullOrWhiteSpace(trip.StartAddress), "Trip should have a start address");
            Assert.False(string.IsNullOrWhiteSpace(trip.EndAddress), "Trip should have an end address");
            Assert.False(string.IsNullOrWhiteSpace(trip.Name), "Trip should have a customer name");
        });
    }

    [Fact]
    public void GenerateDemoData_Expenses_HaveValidCategories()
    {
        // Arrange
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 31);
        const int seed = 666;
        var validCategories = new[] { "Fuel", "Maintenance", "Car Wash", "Supplies", "Parking", "Tolls", "Phone" };

        // Act
        var result = DemoHelpers.GenerateDemoData(startDate, endDate, seed);

        // Assert - all expenses should have valid categories
        Assert.All(result.Expenses, expense =>
        {
            Assert.Contains(expense.Category, validCategories);
        });
    }

    [Fact]
    public void GenerateDemoData_Shifts_HaveReasonableTimeDurations()
    {
        // Arrange
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 7);
        const int seed = 777;

        // Act
        var result = DemoHelpers.GenerateDemoData(startDate, endDate, seed);

        // Assert - shift duration should be 2-8 hours per code
        Assert.All(result.Shifts.Where(s => !string.IsNullOrEmpty(s.Time)), shift =>
        {
            var duration = TimeSpan.Parse(shift.Time);
            Assert.True(duration >= TimeSpan.FromHours(2) && duration <= TimeSpan.FromHours(8),
                $"Shift duration {duration} is outside expected range 2-8 hours");
        });
    }

    [Fact]
    public void GenerateDemoData_OdometerValues_AreRealistic()
    {
        // Arrange
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 7);
        const int seed = 888;

        // Act
        var result = DemoHelpers.GenerateDemoData(startDate, endDate, seed);

        // Assert - when odometer values exist, end should be greater than start
        Assert.All(result.Shifts.Where(s => s.OdometerStart.HasValue && s.OdometerEnd.HasValue), shift =>
        {
            Assert.True(shift.OdometerEnd > shift.OdometerStart,
                $"Odometer end {shift.OdometerEnd} should be greater than start {shift.OdometerStart}");
        });

        Assert.All(result.Trips.Where(t => t.OdometerStart.HasValue && t.OdometerEnd.HasValue), trip =>
        {
            Assert.True(trip.OdometerEnd > trip.OdometerStart,
                $"Odometer end {trip.OdometerEnd} should be greater than start {trip.OdometerStart}");
        });
    }
}
