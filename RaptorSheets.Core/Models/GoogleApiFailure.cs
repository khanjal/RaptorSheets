using System.Net;

namespace RaptorSheets.Core.Models;

/// <summary>
/// Why a Google API call failed, named for the response the caller would actually give a distinct
/// reaction to. The one that matters most: <see cref="QuotaExceeded"/> is transient and means "try
/// again shortly" - it must never be treated the same as an empty or missing spreadsheet.
/// </summary>
public enum GoogleApiFailureReason
{
    /// <summary>Credentials are missing, expired, or revoked. Retrying will not help.</summary>
    Unauthorized,

    /// <summary>Authenticated, but lacking access to this spreadsheet or resource.</summary>
    Forbidden,

    /// <summary>The spreadsheet, sheet, or range does not exist.</summary>
    NotFound,

    /// <summary>
    /// Rate limit or quota exceeded (HTTP 429). Transient - retry later. Explicitly not the same
    /// condition as "the data is missing"; a caller must not treat this as an empty result.
    /// </summary>
    QuotaExceeded,

    /// <summary>Any other API failure, including transport-level exceptions with no HTTP status.</summary>
    Unknown
}

/// <summary>
/// Carries the reason a Google API call failed, without the caller needing to inspect
/// <see cref="global::Google.GoogleApiException"/> or an HTTP status code directly.
/// </summary>
public class GoogleApiFailure
{
    public required GoogleApiFailureReason Reason { get; init; }

    /// <summary>Raw HTTP status code, when the failure came from an API response rather than a transport exception.</summary>
    public HttpStatusCode? StatusCode { get; init; }

    public required string Message { get; init; }

    /// <summary>
    /// Classifies a caught exception into a <see cref="GoogleApiFailure"/>. Internal: callers get
    /// this attached to a result, not the classification logic itself.
    /// </summary>
    internal static GoogleApiFailure FromException(Exception ex)
    {
        if (ex is global::Google.GoogleApiException apiEx)
        {
            var reason = apiEx.HttpStatusCode switch
            {
                HttpStatusCode.Unauthorized => GoogleApiFailureReason.Unauthorized,
                HttpStatusCode.Forbidden => GoogleApiFailureReason.Forbidden,
                HttpStatusCode.NotFound => GoogleApiFailureReason.NotFound,
                HttpStatusCode.TooManyRequests => GoogleApiFailureReason.QuotaExceeded,
                _ => GoogleApiFailureReason.Unknown
            };

            return new GoogleApiFailure
            {
                Reason = reason,
                StatusCode = apiEx.HttpStatusCode,
                Message = apiEx.Message
            };
        }

        return new GoogleApiFailure
        {
            Reason = GoogleApiFailureReason.Unknown,
            StatusCode = null,
            Message = ex.Message
        };
    }
}

/// <summary>
/// Result of a Google API call: either the payload, or the reason it failed. Distinguishes "nothing
/// there" from "we couldn't tell" - a caller reading <see cref="Value"/> alone on failure would see
/// the same default as a genuinely empty response, which is exactly the ambiguity this type exists
/// to remove. See <see cref="GoogleApiFailureReason.QuotaExceeded"/> in particular: a caller that
/// only checks <see cref="Value"/> for null cannot tell a quota failure from an empty sheet.
/// </summary>
public class GoogleApiResult<T>
{
    public bool Success { get; private init; }
    public T? Value { get; private init; }
    public GoogleApiFailure? Failure { get; private init; }

    public static GoogleApiResult<T> Ok(T value) => new() { Success = true, Value = value };

    public static GoogleApiResult<T> Failed(GoogleApiFailure failure) => new() { Success = false, Failure = failure };
}
