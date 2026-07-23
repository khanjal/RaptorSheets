using Microsoft.Extensions.Logging;
using RaptorSheets.Core.Models;
using RaptorSheets.Core.Options;
using RaptorSheets.Core.Services;

namespace RaptorSheets.Core.Factories;

/// <summary>
/// Creates domain managers for a spreadsheet chosen at runtime rather than at registration.
/// <para>
/// This is the shape a multi-tenant host needs: each signed-in user has their own spreadsheet and
/// their own credentials, so the target can't be bound once from configuration. Resolve the factory
/// from the container and create a manager per request.
/// </para>
/// </summary>
/// <typeparam name="TManager">The domain's manager interface.</typeparam>
public interface ISheetManagerFactory<out TManager> where TManager : class
{
    /// <summary>Creates a manager authenticated with an OAuth access token.</summary>
    TManager Create(string accessToken, string spreadsheetId, GoogleRetryOptions? retryOptions = null);

    /// <summary>Creates a manager authenticated with service-account credential fields.</summary>
    TManager Create(Dictionary<string, string> serviceAccountCredentials, string spreadsheetId, GoogleRetryOptions? retryOptions = null);
}

/// <summary>
/// Default <see cref="ISheetManagerFactory{TManager}"/>, built from a domain-supplied constructor
/// delegate so Core never needs to know the concrete manager types.
/// </summary>
public sealed class SheetManagerFactory<TManager> : ISheetManagerFactory<TManager> where TManager : class
{
    private readonly Func<IGoogleSheetService, ILogger?, TManager> _create;
    private readonly ILogger? _logger;
    private readonly GoogleRetryOptions _defaultRetryOptions;

    public SheetManagerFactory(
        Func<IGoogleSheetService, ILogger?, TManager> create,
        ILogger? logger = null,
        GoogleRetryOptions? defaultRetryOptions = null)
    {
        _create = create ?? throw new ArgumentNullException(nameof(create));
        _logger = logger;
        _defaultRetryOptions = defaultRetryOptions ?? GoogleRetryOptions.Default;
    }

    public TManager Create(string accessToken, string spreadsheetId, GoogleRetryOptions? retryOptions = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(accessToken);
        ArgumentException.ThrowIfNullOrWhiteSpace(spreadsheetId);

        var service = new GoogleSheetService(accessToken, spreadsheetId, _logger, retryOptions ?? _defaultRetryOptions);

        return _create(service, _logger);
    }

    public TManager Create(Dictionary<string, string> serviceAccountCredentials, string spreadsheetId, GoogleRetryOptions? retryOptions = null)
    {
        ArgumentNullException.ThrowIfNull(serviceAccountCredentials);
        ArgumentException.ThrowIfNullOrWhiteSpace(spreadsheetId);

        var service = new GoogleSheetService(serviceAccountCredentials, spreadsheetId, _logger, retryOptions ?? _defaultRetryOptions);

        return _create(service, _logger);
    }

    /// <summary>
    /// Creates a manager from configuration-bound options, validating them first so a misconfigured
    /// host fails at resolution with a clear message instead of at the first API call.
    /// </summary>
    internal TManager CreateFromOptions(RaptorSheetsOptions options, string domainName)
    {
        options.Validate(domainName);

        return options.ServiceAccountCredentials is { Count: > 0 } credentials
            ? Create(credentials, options.SpreadsheetId!, options.Retry)
            : Create(options.AccessToken!, options.SpreadsheetId!, options.Retry);
    }
}
