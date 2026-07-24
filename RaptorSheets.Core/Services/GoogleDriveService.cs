using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Mappers;
using RaptorSheets.Core.Models;
using RaptorSheets.Core.Wrappers;
using System.Diagnostics.CodeAnalysis;

namespace RaptorSheets.Core.Services;

public interface IGoogleDriveService
{
    public Task<PropertyEntity> CreateSpreadsheet(string name, CancellationToken cancellationToken = default);
    public Task<IList<PropertyEntity>> GetSpreadsheets(CancellationToken cancellationToken = default);
}

[ExcludeFromCodeCoverage]
public class GoogleDriveService : IGoogleDriveService
{
    private readonly IDriveServiceWrapper _driveService;

    public GoogleDriveService(string accessToken, GoogleRetryOptions? retryOptions = null)
    {
        _driveService = new DriveServiceWrapper(accessToken, retryOptions);
    }

    public async Task<PropertyEntity> CreateSpreadsheet(string name, CancellationToken cancellationToken = default)
    {
        var sheet = await _driveService.CreateSpreadsheet(name, cancellationToken);
        return PropertyEntityMapper.MapFromDriveFile(sheet);
    }

    public async Task<IList<PropertyEntity>> GetSpreadsheets(CancellationToken cancellationToken = default)
    {
        var sheets = await _driveService.ListSpreadsheets(cancellationToken);
        return PropertyEntityMapper.MapFromDriveFiles(sheets);
    }
}
