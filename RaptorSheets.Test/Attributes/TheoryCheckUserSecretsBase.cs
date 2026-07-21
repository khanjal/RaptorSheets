using RaptorSheets.Test.Common.Constants;
using RaptorSheets.Test.Common.Helpers;
using Xunit;

namespace RaptorSheets.Test.Common.Attributes;

/// <summary>
/// Theory counterpart of <see cref="FactCheckUserSecretsBase"/> - see that type for the rationale.
/// </summary>
public abstract class TheoryCheckUserSecretsBase : TheoryAttribute
{
    protected TheoryCheckUserSecretsBase(string spreadsheetId)
    {
        if (!GoogleCredentialHelpers.IsCredentialAndSpreadsheetId(spreadsheetId))
        {
            Skip = TestSkipMessages.MissingUserSecrets;
        }
    }
}
