using Google.Apis.Http;
using Google.Apis.Services;
using Google.Apis.Util;
using RaptorSheets.Core.Constants;
using RaptorSheets.Core.Models;
using System.Diagnostics.CodeAnalysis;

namespace RaptorSheets.Core.Helpers;

/// <summary>
/// Builds the Google client service initializer shared by <see cref="Wrappers.SheetServiceWrapper"/>
/// and <see cref="Wrappers.DriveServiceWrapper"/>, including transient-failure retry.
/// </summary>
[ExcludeFromCodeCoverage]
public static class GoogleServiceInitializerHelper
{
    private const int TooManyRequests = 429;

    /// <summary>
    /// Creates an initializer with the app name and the built-in exponential backoff policy applied.
    /// The built-in policy covers 503s and transport exceptions only - rate limiting (429) needs
    /// <see cref="ApplyRateLimitBackOff"/> on the constructed service as well.
    /// </summary>
    public static BaseClientService.Initializer CreateInitializer(
        IConfigurableHttpClientInitializer httpInitializer,
        GoogleRetryOptions? retryOptions = null)
    {
        var options = retryOptions ?? GoogleRetryOptions.Default;

        return new BaseClientService.Initializer
        {
            HttpClientInitializer = httpInitializer,
            ApplicationName = GoogleConfig.AppName,
            DefaultExponentialBackOffPolicy = options.Enabled
                ? ExponentialBackOffPolicy.Exception | ExponentialBackOffPolicy.UnsuccessfulResponse503
                : ExponentialBackOffPolicy.None
        };
    }

    /// <summary>
    /// Registers backoff for HTTP 429 responses, which the built-in policy does not cover. The Sheets
    /// API enforces low per-minute quotas, so 429 is the transient failure callers actually hit;
    /// without this, a burst of concurrent work fails hard instead of pausing and succeeding.
    /// </summary>
    public static void ApplyRateLimitBackOff(BaseClientService service, GoogleRetryOptions? retryOptions = null)
    {
        var options = retryOptions ?? GoogleRetryOptions.Default;

        if (!options.Enabled || options.MaxRetries == 0)
        {
            return;
        }

        var backOff = new ExponentialBackOff(options.BackOffDelta, options.MaxRetries);
        var handler = new BackOffHandler(new BackOffHandler.Initializer(backOff)
        {
            MaxTimeSpan = options.MaxDelay,
            HandleUnsuccessfulResponseFunc = response => (int)response.StatusCode == TooManyRequests
        });

        service.HttpClient.MessageHandler.AddUnsuccessfulResponseHandler(handler);
    }
}
