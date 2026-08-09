using RaptorSheets.Gig.Helpers;

namespace RaptorSheets.Gig.Tests.Unit.Helpers;

/// <summary>
/// Covers #95: demo trip locations should reuse a consistent, small set of addresses per place/
/// customer within a single GenerateDemoData call, without leaking state between separate calls
/// (the bug in the archived static-dictionary implementation this replaces).
/// </summary>
public class DemoHelpersTests
{
    private static readonly string[] ValidStreetTypes = ["St", "Ave", "Blvd", "Rd", "Dr", "Ln", "Way", "Ct", "Pl", "Terrace"];

    private static void AssertValidAddress(string address)
    {
        var parts = address.Split(' ');
        Assert.True(parts.Length >= 3, $"Address should have at least 3 parts: {address}");
        Assert.True(int.TryParse(parts[0], out var streetNumber), $"First part should be numeric: {address}");
        Assert.InRange(streetNumber, 100, 999);
        Assert.Contains(parts[^1], ValidStreetTypes);
    }

    private static void AssertValidCustomerName(string name)
    {
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(2, parts.Length);
        Assert.False(string.IsNullOrWhiteSpace(parts[0]));
        Assert.Matches(@"^[A-Z]\.$", parts[1]);
    }

    #region BuildPlaceAddressPool

    [Fact]
    public void BuildPlaceAddressPool_ShouldPopulateEveryPlaceWithOneToFiveValidAddresses()
    {
        var pool = DemoHelpers.BuildPlaceAddressPool(new Random(12345));

        Assert.NotEmpty(pool);
        foreach (var addresses in pool.Values)
        {
            Assert.InRange(addresses.Count, 1, 5);
            Assert.All(addresses, AssertValidAddress);
        }
    }

    [Fact]
    public void BuildPlaceAddressPool_WithSameSeed_ShouldBeReproducible()
    {
        var pool1 = DemoHelpers.BuildPlaceAddressPool(new Random(99999));
        var pool2 = DemoHelpers.BuildPlaceAddressPool(new Random(99999));

        Assert.Equal(pool1.Count, pool2.Count);
        foreach (var (place, addresses) in pool1)
        {
            Assert.True(pool2.ContainsKey(place));
            Assert.Equal(addresses, pool2[place]);
        }
    }

    #endregion

    #region BuildCustomerAddressPool

    [Fact]
    public void BuildCustomerAddressPool_ShouldPopulateEveryCustomerWithOneToTwoValidAddresses()
    {
        var pool = DemoHelpers.BuildCustomerAddressPool(new Random(54321), 100);

        Assert.NotEmpty(pool);
        foreach (var (name, addresses) in pool)
        {
            AssertValidCustomerName(name);
            Assert.InRange(addresses.Count, 1, 2);
            Assert.All(addresses, AssertValidAddress);
        }
    }

    [Fact]
    public void BuildCustomerAddressPool_WithSameSeed_ShouldBeReproducible()
    {
        var pool1 = DemoHelpers.BuildCustomerAddressPool(new Random(11223), 50);
        var pool2 = DemoHelpers.BuildCustomerAddressPool(new Random(11223), 50);

        Assert.Equal(pool1.Count, pool2.Count);
        foreach (var (name, addresses) in pool1)
        {
            Assert.True(pool2.ContainsKey(name));
            Assert.Equal(addresses, pool2[name]);
        }
    }

    [Fact]
    public void BuildCustomerAddressPool_WithLargeSample_ShouldGenerateHouseholdMembersSharingAddressAndInitial()
    {
        // ~5% chance per customer - 300 customers makes a zero-household outcome astronomically
        // unlikely (expected ~15), so this is effectively deterministic without pinning a seed.
        var pool = DemoHelpers.BuildCustomerAddressPool(new Random(11111), 300);

        var addressToNames = new Dictionary<string, List<string>>();
        foreach (var (name, addresses) in pool)
        {
            foreach (var address in addresses)
            {
                if (!addressToNames.TryGetValue(address, out var names))
                {
                    names = [];
                    addressToNames[address] = names;
                }
                names.Add(name);
            }
        }

        var sharedAddresses = addressToNames.Values.Where(names => names.Count > 1).ToList();
        Assert.NotEmpty(sharedAddresses);

        // At least one shared address should belong to people with the same last initial -
        // that's what makes them look like a household rather than a coincidental collision.
        var hasMatchingHousehold = sharedAddresses.Any(names =>
            names.Select(n => n.Split(' ', StringSplitOptions.RemoveEmptyEntries)[^1]).Distinct().Count() == 1);
        Assert.True(hasMatchingHousehold, "Expected at least one household pair sharing both an address and last initial");
    }

    #endregion

    #region GenerateDemoData

    [Fact]
    public void GenerateDemoData_TripsShouldOnlyReferencePlacesAndCustomersFromThisCallsPools()
    {
        var startDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var endDate = new DateTime(2024, 1, 10, 0, 0, 0, DateTimeKind.Utc);

        var result = DemoHelpers.GenerateDemoData(startDate, endDate, seed: 42);

        // Rebuild the same pools independently (same seed, same call order as GenerateDemoData) to
        // verify every trip actually drew from them - not just plausible-looking output.
        var random = new Random(42);
        var placeAddresses = DemoHelpers.BuildPlaceAddressPool(random);
        var customerCount = Math.Clamp((int)(endDate - startDate).TotalDays * 3, 15, 300);
        var customerAddresses = DemoHelpers.BuildCustomerAddressPool(random, customerCount);

        Assert.NotEmpty(result.Sheets.Trips);
        foreach (var trip in result.Sheets.Trips)
        {
            Assert.Contains(trip.Place, placeAddresses.Keys);
            Assert.Contains(trip.StartAddress, placeAddresses[trip.Place]);
            Assert.Contains(trip.Name, customerAddresses.Keys);
            Assert.Contains(trip.EndAddress, customerAddresses[trip.Name]);
        }
    }

    [Fact]
    public void GenerateDemoData_OverMultipleDays_ShouldReuseAddressesAcrossTrips()
    {
        // The whole point of #95: with enough trips, some place or customer address must repeat -
        // proves reuse actually happens, not just that it's theoretically possible.
        var result = DemoHelpers.GenerateDemoData(new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 3, 1, 0, 0, 0, DateTimeKind.Utc), seed: 7);

        Assert.True(result.Sheets.Trips.Count > 10, "Need a reasonably large sample for this assertion to be meaningful");

        var startAddressReused = result.Sheets.Trips.Select(t => t.StartAddress).GroupBy(a => a).Any(g => g.Count() > 1);
        var endAddressReused = result.Sheets.Trips.Select(t => t.EndAddress).GroupBy(a => a).Any(g => g.Count() > 1);

        Assert.True(startAddressReused || endAddressReused, "Expected at least one address to repeat across trips");
    }

    [Fact]
    public void GenerateDemoData_WithSameSeed_ShouldBeReproducible()
    {
        var result1 = DemoHelpers.GenerateDemoData(new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 7, 0, 0, 0, DateTimeKind.Utc), seed: 555);
        var result2 = DemoHelpers.GenerateDemoData(new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 7, 0, 0, 0, DateTimeKind.Utc), seed: 555);

        Assert.Equal(result1.Sheets.Trips.Count, result2.Sheets.Trips.Count);
        Assert.Equal(
            result1.Sheets.Trips.Select(t => (t.Place, t.StartAddress, t.Name, t.EndAddress)),
            result2.Sheets.Trips.Select(t => (t.Place, t.StartAddress, t.Name, t.EndAddress)));
    }

    [Fact]
    public void GenerateDemoData_ConsecutiveCallsWithDifferentSeeds_ShouldNotShareState()
    {
        // The actual bug being fixed: the archived static-dictionary version permanently fixed the
        // pools on the *first* call, so a later call with a different seed still produced the same
        // pools. Two different-seed calls back to back must be free to diverge.
        var result1 = DemoHelpers.GenerateDemoData(new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 14, 0, 0, 0, DateTimeKind.Utc), seed: 1);
        var result2 = DemoHelpers.GenerateDemoData(new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 14, 0, 0, 0, DateTimeKind.Utc), seed: 2);

        var places1 = result1.Sheets.Trips.Select(t => t.Place).ToHashSet();
        var places2 = result2.Sheets.Trips.Select(t => t.Place).ToHashSet();
        var addresses1 = result1.Sheets.Trips.Select(t => t.StartAddress).ToHashSet();
        var addresses2 = result2.Sheets.Trips.Select(t => t.StartAddress).ToHashSet();

        Assert.False(places1.SetEquals(places2) && addresses1.SetEquals(addresses2),
            "Two different seeds produced identical place/address sets - pools may be leaking between calls");
    }

    #endregion
}
