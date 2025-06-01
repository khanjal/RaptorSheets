using RaptorSheets.Core.Wrappers;
using System.Diagnostics.CodeAnalysis;
using File = Google.Apis.Drive.v3.Data.File;

namespace RaptorSheets.Core.Services;

public interface IGoogleDriveService
{
    public Task<File> CreateSpreadsheet(string name);
    public Task<IList<File>> GetSpreadsheets();
}

[ExcludeFromCodeCoverage]
public class GoogleDriveService : IGoogleDriveService
{
    private readonly DriveServiceWrapper _driveService;

    public GoogleDriveService(string accessToken)
    {
        _driveService = new DriveServiceWrapper(accessToken);
    }

    public async Task<File> CreateSpreadsheet(string name)
    {
        var sheet = await _driveService.CreateSpreadsheet(name);

        return sheet;
    }

    public async Task<IList<File>> GetSpreadsheets()
    {
        var sheets = await _driveService.ListSpreadsheets();

        return [.. sheets];
    }
}
