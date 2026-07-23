using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Mappers;
using RaptorSheets.Core.Models;
using RaptorSheets.Core.Wrappers;
using System.Diagnostics.CodeAnalysis;

namespace RaptorSheets.Core.Services;

public interface IGoogleDriveService
{
    public Task<PropertyEntity> CreateSpreadsheet(string name);
    public Task<IList<PropertyEntity>> GetSpreadsheets();
}

[ExcludeFromCodeCoverage]
public class GoogleDriveService : IGoogleDriveService
{
    private readonly IDriveServiceWrapper _driveService;

    public GoogleDriveService(string accessToken, GoogleRetryOptions? retryOptions = null)
    {
        _driveService = new DriveServiceWrapper(accessToken, retryOptions);
    }

    public async Task<PropertyEntity> CreateSpreadsheet(string name)
    {
        var sheet = await _driveService.CreateSpreadsheet(name);
        return PropertyEntityMapper.MapFromDriveFile(sheet);
    }

    public async Task<IList<PropertyEntity>> GetSpreadsheets()
    {
        var sheets = await _driveService.ListSpreadsheets();
        return PropertyEntityMapper.MapFromDriveFiles(sheets);
    }
}
