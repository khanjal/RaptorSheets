namespace RaptorSheets.Core.Models;

/// <summary>
/// Caps how many requests a single <see cref="Services.GoogleSheetService"/> instance will have in
/// flight at once, independent of retry/backoff (<see cref="GoogleRetryOptions"/>). Nothing today
/// stops concurrent operations from the same instance (e.g. parallel per-sheet writes) from stacking
/// up against the same per-minute Sheets API quota; this is a simple ceiling on that, not a batching
/// mechanism - requests still execute one at a time up to the cap, not merged into fewer calls.
/// </summary>
public class GoogleConcurrencyOptions
{
    /// <summary>Shared default instance: unlimited concurrency - matches today's behavior.</summary>
    public static GoogleConcurrencyOptions Default { get; } = new();

    /// <summary>
    /// Largest number of requests this instance will have in flight at once. Zero or negative means
    /// unlimited (no gate); this is the default, since a conservative cap could silently slow down a
    /// caller who has never hit a quota problem.
    /// </summary>
    public int MaxConcurrentRequests { get; init; } = 0;
}
