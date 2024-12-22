using RaptorSheets.Test.Constants;
using RaptorSheets.Test.Helpers;

namespace RaptorSheets.Gig.Tests.Data.Attributes;

public sealed class FactCheckUserSecrets : FactAttribute
{
    public FactCheckUserSecrets()
    {
        if (!GoogleCredentialHelpers.IsCredentialAndSpreadsheetId(TestConfigurationHelpers.GetGigSpreadsheet()))
        {
            Skip = TestSkipMessages.MissingUserSecrets;
        }
    }
}
