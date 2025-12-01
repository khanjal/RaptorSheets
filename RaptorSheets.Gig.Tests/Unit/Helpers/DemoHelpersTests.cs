using RaptorSheets.Gig.Helpers;
using System.Reflection;

namespace RaptorSheets.Gig.Tests.Unit.Helpers;

public class DemoHelpersTests
{
    // Helper method to access private static fields via reflection
    private static Dictionary<string, List<string>> GetPlaceAddresses()
    {
        var field = typeof(DemoHelpers).GetField("PlaceAddresses", BindingFlags.NonPublic | BindingFlags.Static);
        return (Dictionary<string, List<string>>)field!.GetValue(null)!;
    }

    private static Dictionary<string, List<string>> GetNameAddresses()
    {
        var field = typeof(DemoHelpers).GetField("NameAddresses", BindingFlags.NonPublic | BindingFlags.Static);
        return (Dictionary<string, List<string>>)field!.GetValue(null)!;
    }

    private static void ClearGlobalDictionaries()
    {
        GetPlaceAddresses().Clear();
        GetNameAddresses().Clear();
    }

    [Fact]
    public void GeneratePlacesWithAddresses_ShouldPopulateGlobalDictionary()
    {
        // Arrange
        ClearGlobalDictionaries();
        var random = new Random(12345);

        // Act
        DemoHelpers.GeneratePlacesWithAddresses(random);
        var placeAddresses = GetPlaceAddresses();

        // Assert
        Assert.NotEmpty(placeAddresses);
        Assert.True(placeAddresses.Count > 0, "Should have populated place addresses");
        
        // Each place should have 1-5 addresses
        foreach (var place in placeAddresses)
        {
            Assert.InRange(place.Value.Count, 1, 5);
            Assert.All(place.Value, address => Assert.False(string.IsNullOrWhiteSpace(address)));
        }
    }

    [Fact]
    public void GeneratePlacesWithAddresses_WithSeed_ShouldBeReproducible()
    {
        // Arrange
        ClearGlobalDictionaries();
        var random1 = new Random(99999);
        DemoHelpers.GeneratePlacesWithAddresses(random1);
        var firstRun = new Dictionary<string, List<string>>(GetPlaceAddresses());

        ClearGlobalDictionaries();
        var random2 = new Random(99999);
        DemoHelpers.GeneratePlacesWithAddresses(random2);
        var secondRun = GetPlaceAddresses();

        // Assert - same seed should produce same results
        Assert.Equal(firstRun.Count, secondRun.Count);
        foreach (var place in firstRun.Keys)
        {
            Assert.True(secondRun.ContainsKey(place));
            Assert.Equal(firstRun[place].Count, secondRun[place].Count);
            Assert.Equal(firstRun[place], secondRun[place]);
        }
    }

    [Fact]
    public void GeneratePeopleWithAddresses_ShouldPopulateGlobalDictionary()
    {
        // Arrange
        ClearGlobalDictionaries();
        var random = new Random(54321);
        int numberOfPeople = 100;

        // Act
        DemoHelpers.GeneratePeopleWithAddresses(random, numberOfPeople);
        var nameAddresses = GetNameAddresses();

        // Assert
        Assert.NotEmpty(nameAddresses);
        Assert.True(nameAddresses.Count >= numberOfPeople, "Should have at least the requested number of people");
        
        // Each person should have 1-2 addresses
        foreach (var person in nameAddresses)
        {
            Assert.InRange(person.Value.Count, 1, 2);
            Assert.All(person.Value, address => Assert.False(string.IsNullOrWhiteSpace(address)));
        }
    }

    [Fact]
    public void GeneratePeopleWithAddresses_ShouldGenerateHouseholdMembers()
    {
        // Arrange
        ClearGlobalDictionaries();
        var random = new Random(11111);
        int numberOfPeople = 500; // Larger sample increases chance of household members

        // Act
        DemoHelpers.GeneratePeopleWithAddresses(random, numberOfPeople);
        var nameAddresses = GetNameAddresses();

        // Assert
        // Note: Due to random name generation, we may have duplicate names reducing unique count
        // We should still have a reasonable number of unique people
        Assert.True(nameAddresses.Count >= numberOfPeople * 0.8, "Should have at least 80% of requested people due to possible name collisions");
        
        // Check for shared addresses (household members share addresses)
        var allAddresses = nameAddresses.Values.SelectMany(a => a).ToList();
        var duplicateAddresses = allAddresses.GroupBy(a => a).Where(g => g.Count() > 1).ToList();
        
        // With 500 people and 5% household chance, we should have some duplicates
        Assert.NotEmpty(duplicateAddresses);
    }

    [Fact]
    public void GeneratePeopleWithAddresses_ShouldHaveSameLastInitialForHouseholdMembers()
    {
        // Arrange
        ClearGlobalDictionaries();
        var random = new Random(22222);
        int numberOfPeople = 500;

        // Act
        DemoHelpers.GeneratePeopleWithAddresses(random, numberOfPeople);
        var nameAddresses = GetNameAddresses();

        // Assert - find people with shared addresses
        var addressToPeople = new Dictionary<string, List<string>>();
        foreach (var (name, addresses) in nameAddresses)
        {
            foreach (var address in addresses)
            {
                if (!addressToPeople.ContainsKey(address))
                {
                    addressToPeople[address] = new List<string>();
                }
                addressToPeople[address].Add(name);
            }
        }

        // Find shared addresses
        var sharedAddresses = addressToPeople.Where(kvp => kvp.Value.Count > 1).ToList();
        
        if (sharedAddresses.Any())
        {
            // Verify that we have household members sharing addresses
            Assert.NotEmpty(sharedAddresses);
            
            // Most people at same address should have same last initial
            // (There may be edge cases where address collision happens randomly)
            var householdsWithSameInitial = 0;
            foreach (var shared in sharedAddresses)
            {
                var lastInitials = shared.Value
                    .Select(name => name.Split(' ', StringSplitOptions.RemoveEmptyEntries).LastOrDefault()?.FirstOrDefault())
                    .Distinct()
                    .ToList();

                if (lastInitials.Count == 1)
                {
                    householdsWithSameInitial++;
                }
            }
            
            // At least some households should have matching initials
            Assert.True(householdsWithSameInitial > 0, "Should have at least some households with matching last initials");
        }
    }

    [Fact]
    public void GenerateDemoData_ShouldGenerateCompleteSheetEntity()
    {
        // Arrange
        ClearGlobalDictionaries();
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 1, 7); // One week
        var seed = 99999;

        // Act
        var result = DemoHelpers.GenerateDemoData(startDate, endDate, seed);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Shifts);
        Assert.NotEmpty(result.Trips);
        
        // Should have populated global dictionaries
        var placeAddresses = GetPlaceAddresses();
        var nameAddresses = GetNameAddresses();
        Assert.NotEmpty(placeAddresses);
        Assert.NotEmpty(nameAddresses);
        
        // Trips should reference places and customers from global dictionaries
        foreach (var trip in result.Trips)
        {
            if (!string.IsNullOrEmpty(trip.Place))
            {
                Assert.Contains(trip.Place, placeAddresses.Keys);
            }
            if (!string.IsNullOrEmpty(trip.Name))
            {
                Assert.Contains(trip.Name, nameAddresses.Keys);
            }
        }
    }

    [Fact]
    public void GenerateDemoData_WithSeed_ShouldBeReproducible()
    {
        // Arrange
        ClearGlobalDictionaries();
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 1, 3);
        var seed = 12345;

        // Act - First run
        var result1 = DemoHelpers.GenerateDemoData(startDate, endDate, seed);
        var places1 = new Dictionary<string, List<string>>(GetPlaceAddresses());
        var names1 = new Dictionary<string, List<string>>(GetNameAddresses());

        ClearGlobalDictionaries();

        // Act - Second run with same seed
        var result2 = DemoHelpers.GenerateDemoData(startDate, endDate, seed);
        var places2 = GetPlaceAddresses();
        var names2 = GetNameAddresses();

        // Assert - Both runs should be identical
        Assert.Equal(result1.Shifts.Count, result2.Shifts.Count);
        Assert.Equal(result1.Trips.Count, result2.Trips.Count);
        Assert.Equal(result1.Expenses.Count, result2.Expenses.Count);

        // Global dictionaries should be identical
        Assert.Equal(places1.Count, places2.Count);
        Assert.Equal(names1.Count, names2.Count);
    }

    [Fact]
    public void GenerateDemoData_ShouldGenerateDataForEachDay()
    {
        // Arrange
        ClearGlobalDictionaries();
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 1, 10);
        var seed = 77777;

        // Act
        var result = DemoHelpers.GenerateDemoData(startDate, endDate, seed);

        // Assert
        var dates = result.Shifts.Select(s => s.Date).Distinct().ToList();
        
        // Should have shifts spanning multiple days (not necessarily every day due to 85% work chance)
        Assert.True(dates.Count >= 5, "Should have shifts on at least 5 days out of 10");
        
        // All dates should be within range
        foreach (var date in dates)
        {
            var parsedDate = DateTime.Parse(date);
            Assert.True(parsedDate >= startDate && parsedDate <= endDate);
        }
    }

    [Fact]
    public void GenerateDemoData_ShiftsShouldHaveTrips()
    {
        // Arrange
        ClearGlobalDictionaries();
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 1, 5);
        var seed = 55555;

        // Act
        var result = DemoHelpers.GenerateDemoData(startDate, endDate, seed);

        // Assert
        // Most shifts should have associated trips
        var shiftsWithTrips = result.Shifts
            .Where(s => result.Trips.Any(t => t.Date == s.Date && t.Number == s.Number && t.Service == s.Service))
            .ToList();
        
        Assert.True(shiftsWithTrips.Count > 0, "Should have some shifts with trips");
        
        // Verify trip data structure
        foreach (var trip in result.Trips)
        {
            Assert.False(string.IsNullOrWhiteSpace(trip.Date));
            Assert.False(string.IsNullOrWhiteSpace(trip.Service));
            Assert.True(trip.Pay > 0);
        }
    }

    [Fact]
    public void GenerateDemoData_ShouldIncludeExpenses()
    {
        // Arrange
        ClearGlobalDictionaries();
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 1, 14); // Two weeks
        var seed = 33333;

        // Act
        var result = DemoHelpers.GenerateDemoData(startDate, endDate, seed);

        // Assert
        // Should have some expenses over two weeks (40% chance per day)
        Assert.NotEmpty(result.Expenses);
        
        foreach (var expense in result.Expenses)
        {
            Assert.False(string.IsNullOrWhiteSpace(expense.Category));
            Assert.False(string.IsNullOrWhiteSpace(expense.Date));
            Assert.True(expense.Amount > 0);
        }
    }

    [Fact]
    public void GeneratedAddresses_ShouldHaveValidFormat()
    {
        // Arrange
        ClearGlobalDictionaries();
        var random = new Random(66666);

        // Act
        DemoHelpers.GeneratePlacesWithAddresses(random);
        var placeAddresses = GetPlaceAddresses();

        // Assert
        foreach (var addresses in placeAddresses.Values)
        {
            foreach (var address in addresses)
            {
                // Address should have format: "### StreetName StreetType"
                var parts = address.Split(' ');
                Assert.True(parts.Length >= 3, $"Address should have at least 3 parts: {address}");
                
                // First part should be a number (100-999)
                Assert.True(int.TryParse(parts[0], out int streetNumber));
                Assert.InRange(streetNumber, 100, 999);
                
                // Last part should be a street type
                var streetType = parts[^1];
                Assert.Contains(streetType, new[] { "St", "Ave", "Blvd", "Rd", "Dr", "Ln", "Way", "Ct", "Pl", "Terrace" });
            }
        }
    }

    [Fact]
    public void GeneratedCustomerNames_ShouldHaveValidFormat()
    {
        // Arrange
        ClearGlobalDictionaries();
        var random = new Random(88888);

        // Act
        DemoHelpers.GeneratePeopleWithAddresses(random, 50);
        var nameAddresses = GetNameAddresses();

        // Assert
        foreach (var name in nameAddresses.Keys)
        {
            // Name should have format: "FirstName L."
            var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            Assert.Equal(2, parts.Length);
            
            // First part is first name
            Assert.False(string.IsNullOrWhiteSpace(parts[0]));
            
            // Second part should be single letter followed by period
            Assert.Matches(@"^[A-Z]\.$", parts[1]);
        }
    }

    [Fact]
    public void GlobalDictionaries_ShouldPersistAcrossMultipleCalls()
    {
        // Arrange
        ClearGlobalDictionaries();
        var random = new Random(44444);

        // Act
        DemoHelpers.GeneratePlacesWithAddresses(random);
        var initialPlaceCount = GetPlaceAddresses().Count;
        var initialAddressCount = GetPlaceAddresses().Values.Sum(v => v.Count);

        // Call again - should not duplicate, but might add more addresses
        DemoHelpers.GeneratePlacesWithAddresses(random);
        var finalPlaceCount = GetPlaceAddresses().Count;
        var finalAddressCount = GetPlaceAddresses().Values.Sum(v => v.Count);

        // Assert
        Assert.Equal(initialPlaceCount, finalPlaceCount); // Same places
        Assert.True(finalAddressCount >= initialAddressCount); // Same or more addresses
    }

    [Fact]
    public void GeneratePeopleWithAddresses_DefaultParameter_ShouldGenerate500People()
    {
        // Arrange
        ClearGlobalDictionaries();
        var random = new Random(11223);

        // Act - Call without specifying numberOfPeople (should default to 500)
        DemoHelpers.GeneratePeopleWithAddresses(random);
        var nameAddresses = GetNameAddresses();

        // Assert
        // Should have a reasonable number of unique people (accounting for name collisions)
        // With random name generation, we expect at least 80% unique names
        Assert.True(nameAddresses.Count >= 400, $"Expected at least 400 unique people (80% of 500 accounting for collisions), got {nameAddresses.Count}");
    }
}
