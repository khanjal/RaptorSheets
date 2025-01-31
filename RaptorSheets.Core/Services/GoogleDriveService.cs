using RaptorSheets.Core.Wrappers;
using System.Diagnostics.CodeAnalysis;

namespace RaptorSheets.Core.Services;

public interface IGoogleDriveService
{
    public Task<IList<string>> GetSheetFiles();
}

[ExcludeFromCodeCoverage]
public class GoogleDriveService : IGoogleDriveService
{
    private readonly DriveServiceWrapper _driveService;

    public GoogleDriveService(string accessToken)
    {
        _driveService = new DriveServiceWrapper(accessToken);
    }

    public async Task<IList<string>> GetSheetFiles()
    {
        var sheets = await _driveService.GetSheetFiles();

        var sheetList = sheets.Select(s => s.Name.ToString()).ToList();

        return sheetList;
    }
}
