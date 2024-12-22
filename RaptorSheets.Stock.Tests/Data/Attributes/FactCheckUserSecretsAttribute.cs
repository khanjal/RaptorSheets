using RaptorSheets.Test.Constants;
using RaptorSheets.Test.Helpers;
using Xunit;

namespace RaptorSheets.Stock.Tests.Data.Attributes;

public sealed class FactCheckUserSecrets : FactAttribute
{
    public FactCheckUserSecrets()
    {
        if (!GoogleCredentialHelpers.IsCredentialAndSpreadsheetId(TestConfigurationHelpers.GetStockSpreadsheet()))
        {
            Skip = TestSkipMessages.MissingUserSecrets;
        }
    }
}
