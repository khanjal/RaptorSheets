using FluentAssertions;
using RaptorSheets.Core.Services;
using RaptorSheets.Test.Helpers;

namespace RaptorSheets.Gig.Tests.Services;

public class GoogleDriveServiceTests
{
    private readonly GoogleDriveService _googleDriveService;
    private readonly Dictionary<string, string> _credential;

    public GoogleDriveServiceTests()
    {
        _credential = TestConfigurationHelpers.GetJsonCredential();

        // _googleDriveService = new GoogleDriveService(_credential);
    }

    [Fact(Skip = "Need to look into Drive authentication")]
    public async Task GivenGetAllData_ThenReturnInfo()
    {
        var result = await _googleDriveService.GetSheetFiles();
        result.Should().NotBeNull();
        
    }
}
