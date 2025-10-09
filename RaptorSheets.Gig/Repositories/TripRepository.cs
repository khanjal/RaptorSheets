using RaptorSheets.Core.Repositories;
using RaptorSheets.Core.Services;
using RaptorSheets.Gig.Entities;

namespace RaptorSheets.Gig.Repositories;

/// <summary>
/// Repository for Trip entities with automatic CRUD operations using Column attributes
/// Demonstrates the simplified repository pattern with automatic type conversion
/// </summary>
public class TripRepository : BaseEntityRepository<TripEntity>
{
    public TripRepository(IGoogleSheetService sheetService) 
        : base(sheetService, "Trips", hasHeaderRow: true)
    {
    }

    /// <summary>
    /// Gets trips for a specific date range
    /// </summary>
    /// <param name="startDate">Start date (inclusive)</param>
    /// <param name="endDate">End date (inclusive)</param>
    /// <returns>List of trips in the date range</returns>
    public async Task<List<TripEntity>> GetTripsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var allTrips = await GetAllAsync();
        return allTrips
            .Where(t => !string.IsNullOrEmpty(t.Date) && 
                       DateTime.TryParse(t.Date, out var tripDate) &&
                       tripDate.Date >= startDate.Date && 
                       tripDate.Date <= endDate.Date)
            .OrderBy(t => DateTime.TryParse(t.Date, out var d) ? d : DateTime.MinValue)
            .ToList();
    }

    /// <summary>
    /// Gets trips for a specific service
    /// </summary>
    /// <param name="service">The service name</param>
    /// <returns>List of trips for the service</returns>
    public async Task<List<TripEntity>> GetTripsByServiceAsync(string service)
    {
        var allTrips = await GetAllAsync();
        return allTrips
            .Where(t => string.Equals(t.Service, service, StringComparison.OrdinalIgnoreCase))
            .OrderBy(t => DateTime.TryParse(t.Date, out var d) ? d : DateTime.MinValue)
            .ToList();
    }

    /// <summary>
    /// Calculates total earnings for a date range
    /// </summary>
    /// <param name="startDate">Start date (inclusive)</param>
    /// <param name="endDate">End date (inclusive)</param>
    /// <returns>Total earnings (pay + tips + bonus)</returns>
    public async Task<decimal> GetTotalEarningsAsync(DateTime startDate, DateTime endDate)
    {
        var trips = await GetTripsByDateRangeAsync(startDate, endDate);
        return trips.Sum(t => (t.Pay ?? 0) + (t.Tip ?? 0) + (t.Bonus ?? 0));
    }

    /// <summary>
    /// Adds a trip with automatic key generation
    /// </summary>
    /// <param name="trip">The trip to add</param>
    /// <returns>True if successful</returns>
    public async Task<bool> AddTripAsync(TripEntity trip)
    {
        if (trip == null) throw new ArgumentNullException(nameof(trip));

        // Generate key if not provided
        if (string.IsNullOrEmpty(trip.Key) && !string.IsNullOrEmpty(trip.Date))
        {
            if (DateTime.TryParse(trip.Date, out var tripDate))
            {
                trip.Key = $"{tripDate:yyyyMMdd}_{trip.Service}_{trip.Number}";
            }
        }

        // Auto-calculate total if not provided
        if (!trip.Total.HasValue)
        {
            trip.Total = (trip.Pay ?? 0) + (trip.Tip ?? 0) + (trip.Bonus ?? 0);
        }

        // Extract date components for formulas
        if (!string.IsNullOrEmpty(trip.Date) && DateTime.TryParse(trip.Date, out var parsedDate))
        {
            trip.Day = parsedDate.Day.ToString();
            trip.Month = parsedDate.Month.ToString();
            trip.Year = parsedDate.Year.ToString();
        }

        return await AddAsync(trip);
    }

    /// <summary>
    /// Updates a trip with automatic recalculation
    /// </summary>
    /// <param name="trip">The trip to update</param>
    /// <param name="rowIndex">The row index to update</param>
    /// <returns>True if successful</returns>
    public async Task<bool> UpdateTripAsync(TripEntity trip, int rowIndex)
    {
        if (trip == null) throw new ArgumentNullException(nameof(trip));

        // Auto-calculate total
        trip.Total = (trip.Pay ?? 0) + (trip.Tip ?? 0) + (trip.Bonus ?? 0);

        // Update date components
        if (!string.IsNullOrEmpty(trip.Date) && DateTime.TryParse(trip.Date, out var parsedDate))
        {
            trip.Day = parsedDate.Day.ToString();
            trip.Month = parsedDate.Month.ToString();
            trip.Year = parsedDate.Year.ToString();
        }

        return await UpdateAsync(trip, rowIndex);
    }
}