using RaptorSheets.Test.Common.Constants;
using RaptorSheets.Test.Common.Helpers;

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
