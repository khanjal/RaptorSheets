using RaptorSheets.Core.Services;
using File = Google.Apis.Drive.v3.Data.File;

namespace RaptorSheets.Core.Managers;

public interface IGoogleSheetManager
{
    public Task<File> CreateFile(string name);
    public Task<List<File>> GetFiles();
}

public class GoogleFileManager : IGoogleSheetManager
{
    private readonly GoogleDriveService _googleDriveService;

    public GoogleFileManager(string accessToken)
    {
        _googleDriveService = new GoogleDriveService(accessToken);
    }

    public async Task<File> CreateFile(string name)
    {
        var file = await _googleDriveService.CreateSpreadsheet(name);

        return file;
    }

    public async Task<List<File>> GetFiles()
    {
        var files = await _googleDriveService.GetSpreadsheets();

        return files.ToList();
    }
}
