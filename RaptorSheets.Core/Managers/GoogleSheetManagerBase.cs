using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RaptorSheets.Core.Services;

namespace RaptorSheets.Core.Managers;

/// <summary>
/// Shared construction/plumbing for domain-specific Google Sheet managers (Gig, Stock, and future
/// domains). Each domain package keeps its own manager class, its own public API shape, and its own
/// strongly-typed entities/mappers - this only removes the duplicated constructor boilerplate
/// (DI constructor + two convenience constructors + optional logger wiring) that would otherwise be
/// hand-rolled identically in every domain package.
/// </summary>
public abstract class GoogleSheetManagerBase
{
    protected readonly IGoogleSheetService _googleSheetService;
    protected readonly ILogger _logger;

    protected GoogleSheetManagerBase(IGoogleSheetService googleSheetService, ILogger? logger = null)
    {
        _googleSheetService = googleSheetService ?? throw new ArgumentNullException(nameof(googleSheetService));
        _logger = logger ?? NullLogger.Instance;
    }

    protected GoogleSheetManagerBase(string accessToken, string spreadsheetId, ILogger? logger = null)
        : this(new GoogleSheetService(accessToken, spreadsheetId, logger), logger)
    {
    }

    protected GoogleSheetManagerBase(Dictionary<string, string> parameters, string spreadsheetId, ILogger? logger = null)
        : this(new GoogleSheetService(parameters, spreadsheetId, logger), logger)
    {
    }
}
