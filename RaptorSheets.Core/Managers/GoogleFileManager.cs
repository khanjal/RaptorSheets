using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Services;

namespace RaptorSheets.Core.Managers;

public interface IGoogleFileManager
{
    public Task<PropertyEntity> CreateFile(string name, CancellationToken cancellationToken = default);
    public Task<List<PropertyEntity>> GetFiles(CancellationToken cancellationToken = default);
}

public class GoogleFileManager : IGoogleFileManager
{
    private readonly IGoogleDriveService _googleDriveService;

    // Primary constructor for dependency injection
    public GoogleFileManager(IGoogleDriveService googleDriveService)
    {
        _googleDriveService = googleDriveService ?? throw new ArgumentNullException(nameof(googleDriveService));
    }

    // Convenience constructor for backward compatibility
    public GoogleFileManager(string accessToken)
    {
        ArgumentNullException.ThrowIfNull(accessToken);
        _googleDriveService = new GoogleDriveService(accessToken);
    }

    public async Task<PropertyEntity> CreateFile(string name, CancellationToken cancellationToken = default)
    {
        var file = await _googleDriveService.CreateSpreadsheet(name, cancellationToken);

        return file;
    }

    public async Task<List<PropertyEntity>> GetFiles(CancellationToken cancellationToken = default)
    {
        var files = await _googleDriveService.GetSpreadsheets(cancellationToken);
        return files?.ToList() ?? new List<PropertyEntity>();
    }
}
