using RaptorSheets.Test.Constants;
using RaptorSheets.Test.Helpers;
using Xunit;

namespace RaptorSheets.Stock.Tests.Data.Attributes;

public sealed class TheoryCheckUserSecrets : TheoryAttribute
{
    public TheoryCheckUserSecrets()
    {
        if (!GoogleCredentialHelpers.IsCredentialAndSpreadsheetId(TestConfigurationHelpers.GetStockSpreadsheet()))
        {
            Skip = TestSkipMessages.MissingUserSecrets;
        }
    }
}
