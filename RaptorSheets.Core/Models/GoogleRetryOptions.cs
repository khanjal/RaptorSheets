namespace RaptorSheets.Core.Models;

/// <summary>
/// Controls automatic retry of transient Google API failures (rate limiting, 503s, and transport
/// exceptions). Defaults are tuned for callers running behind a request timeout - see
/// <see cref="MaxDelay"/> - so a burst of retries can't consume the whole budget.
/// </summary>
public class GoogleRetryOptions
{
    /// <summary>
    /// Largest value the underlying Google client accepts for <see cref="BackOffDelta"/>. The client
    /// treats the delta as a small base unit and grows it exponentially, so it rejects anything above
    /// one second outright.
    /// </summary>
    public static readonly TimeSpan MaximumBackOffDelta = TimeSpan.FromSeconds(1);

    /// <summary>Largest value the underlying Google client accepts for <see cref="MaxRetries"/>.</summary>
    public const int MaximumRetryCount = 20;

    /// <summary>
    /// Shared default instance: retries enabled, 3 attempts, a 1 second backoff delta, and an 8
    /// second ceiling on any single wait.
    /// </summary>
    public static GoogleRetryOptions Default { get; } = new();

    /// <summary>
    /// Whether transient failures are retried at all. Set false when the caller does its own
    /// orchestration and wants failures surfaced immediately.
    /// </summary>
    public bool Enabled { get; init; } = true;

    private readonly int _maxRetries = 3;

    /// <summary>
    /// How many times a transient failure is retried before the error is surfaced. Kept small
    /// deliberately: past a few attempts the caller is better served by being told than by waiting.
    /// </summary>
    public int MaxRetries
    {
        get => _maxRetries;
        init
        {
            if (value < 0 || value > MaximumRetryCount)
            {
                throw new ArgumentOutOfRangeException(nameof(MaxRetries), value,
                    $"Retry count must be between 0 and {MaximumRetryCount}.");
            }

            _maxRetries = value;
        }
    }

    private readonly TimeSpan _backOffDelta = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Base unit of the exponential curve, not the literal first delay. The client waits roughly
    /// <c>delta * (2^attempt - 1)</c> with jitter applied, so at the default of one second the actual
    /// waits land near 1.5s, 2.9s, and 4.3s. Must be greater than zero and no more than
    /// <see cref="MaximumBackOffDelta"/>; the default is that maximum, since spacing retries out
    /// generously is what actually lets a per-minute quota recover.
    /// </summary>
    public TimeSpan BackOffDelta
    {
        get => _backOffDelta;
        init
        {
            if (value <= TimeSpan.Zero || value > MaximumBackOffDelta)
            {
                throw new ArgumentOutOfRangeException(nameof(BackOffDelta), value,
                    $"Backoff delta must be greater than zero and no more than {MaximumBackOffDelta.TotalSeconds} second(s).");
            }

            _backOffDelta = value;
        }
    }

    /// <summary>
    /// Ceiling on any single wait. Once the exponential curve would exceed this, retrying stops and
    /// the error is surfaced. This is what keeps a retry storm from blowing a caller's request
    /// budget - with the defaults, total time spent waiting stays under about 9 seconds, which fits
    /// inside a 30 second gateway timeout with room for the requests themselves.
    /// </summary>
    public TimeSpan MaxDelay { get; init; } = TimeSpan.FromSeconds(8);
}
