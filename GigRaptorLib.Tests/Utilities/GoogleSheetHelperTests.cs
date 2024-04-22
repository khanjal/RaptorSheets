using GigRaptorLib.Utilities.Google;

namespace GigRaptorLib.Tests.Utilities;

public class GoogleSheetHelperTests
{
    [Fact]
    public async void GivenGoogleSheetCall_ThenReturnInfo()
    {
        var googleSheetHelper = new GoogleSheetHelper();

        await googleSheetHelper.GetAllData();

        // Test all demo data.

        // Look into replacing individual json sheet tests.
    }
}
