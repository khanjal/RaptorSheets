using RaptorSheets.Test.Common.Constants;
using RaptorSheets.Test.Common.Helpers;
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
