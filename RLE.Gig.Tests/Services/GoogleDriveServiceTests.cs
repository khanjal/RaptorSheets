using FluentAssertions;
using Google.Apis.Sheets.v4.Data;
using RLE.Core.Services;
using RLE.Gig.Enums;
using RLE.Gig.Helpers;
using RLE.Gig.Tests.Data.Helpers;

namespace RLE.Gig.Tests.Services;

public class GoogleDriveServiceTests
{
    private readonly GoogleDriveService _googleDriveService;
    private readonly Dictionary<string, string> _credential;

    public GoogleDriveServiceTests()
    {
        _credential = TestConfigurationHelper.GetJsonCredential();

        _googleDriveService = new GoogleDriveService(_credential);
    }

    [Fact]
    public async Task GivenGetAllData_ThenReturnInfo()
    {
        var result = await _googleDriveService.GetSheetFiles();
        result.Should().NotBeNull();
        
    }
}
