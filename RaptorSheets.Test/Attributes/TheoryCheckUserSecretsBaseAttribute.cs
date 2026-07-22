using RaptorSheets.Test.Common.Constants;
using RaptorSheets.Test.Common.Helpers;
using Xunit;

namespace RaptorSheets.Test.Common.Attributes;

/// <summary>
/// Theory counterpart of <see cref="FactCheckUserSecretsBaseAttribute"/> - see that type for the rationale.
/// </summary>
public abstract class TheoryCheckUserSecretsBaseAttribute : TheoryAttribute
{
    protected TheoryCheckUserSecretsBaseAttribute(string spreadsheetId)
    {
        if (!GoogleCredentialHelpers.IsCredentialAndSpreadsheetId(spreadsheetId))
        {
            Skip = TestSkipMessages.MissingUserSecrets;
        }
    }
}
