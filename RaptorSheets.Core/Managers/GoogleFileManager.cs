using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Services;

namespace RaptorSheets.Core.Managers;

public interface IGoogleFileManager
{
    public Task<PropertyEntity> CreateFile(string name);
    public Task<List<PropertyEntity>> GetFiles();
}

public class GoogleFileManager : IGoogleFileManager
{
    private readonly GoogleDriveService _googleDriveService;

    public GoogleFileManager(string accessToken)
    {
        _googleDriveService = new GoogleDriveService(accessToken);
    }

    public async Task<PropertyEntity> CreateFile(string name)
    {
        var file = await _googleDriveService.CreateSpreadsheet(name);

        return file;
    }

    public async Task<List<PropertyEntity>> GetFiles()
    {
        var files = await _googleDriveService.GetSpreadsheets();

        return [.. files];
    }
}
