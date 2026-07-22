using RaptorSheets.Test.Common.Constants;
using RaptorSheets.Test.Common.Helpers;
using Xunit;

namespace RaptorSheets.Test.Common.Attributes;

/// <summary>
/// Shared skip-check every domain's own FactCheckUserSecrets attribute (Gig, Stock, and future
/// domains) delegates to: skip the test unless real credentials and a spreadsheet ID are
/// configured. Each domain still declares its own sealed FactCheckUserSecrets so xunit's discovery
/// works per-domain and each passes its own domain's spreadsheet ID (GetGigSpreadsheet(),
/// GetStockSpreadsheet(), etc.) - only that one call differs.
/// </summary>
public abstract class FactCheckUserSecretsBaseAttribute : FactAttribute
{
    protected FactCheckUserSecretsBaseAttribute(string spreadsheetId)
    {
        if (!GoogleCredentialHelpers.IsCredentialAndSpreadsheetId(spreadsheetId))
        {
            Skip = TestSkipMessages.MissingUserSecrets;
        }
    }
}
