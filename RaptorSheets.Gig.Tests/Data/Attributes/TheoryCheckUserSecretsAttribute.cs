using RaptorSheets.Test.Constants;
using RaptorSheets.Test.Helpers;

namespace RaptorSheets.Gig.Tests.Data.Attributes;

public sealed class TheoryCheckUserSecrets : TheoryAttribute
{
    public TheoryCheckUserSecrets()
    {
        if (!GoogleCredentialHelpers.IsCredentialAndSpreadsheetId(TestConfigurationHelpers.GetGigSpreadsheet()))
        {
            Skip = TestSkipMessages.MissingUserSecrets;
        }
    }
}
