using RaptorSheets.Test.Common.Constants;
using RaptorSheets.Test.Common.Helpers;
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
