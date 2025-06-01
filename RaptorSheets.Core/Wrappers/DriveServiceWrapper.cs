using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using RaptorSheets.Core.Constants;
using System.Diagnostics.CodeAnalysis;
using File = Google.Apis.Drive.v3.Data.File;

namespace RaptorSheets.Core.Wrappers;

public interface IDriveServiceWrapper
{
    Task<File> CreateSpreadsheet(string name);
    Task<IList<File>> ListSpreadsheets();
}

[ExcludeFromCodeCoverage]
public class DriveServiceWrapper : DriveService, IDriveServiceWrapper
{
    private DriveService _driveService = new();

    public DriveServiceWrapper(string accessToken)
    {
        var credential = GoogleCredential.FromAccessToken(accessToken.Trim());

        InitializeService(credential);
    }

    private DriveService InitializeService(GoogleCredential credential)
    {
        _driveService = new DriveService(new Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = GoogleConfig.AppName
        });

        return _driveService;
    }

    public async Task<File> CreateSpreadsheet(string name)
    {
        var fileMetadata = new File
        {
            Name = name,
            MimeType = "application/vnd.google-apps.spreadsheet"
        };

        var request = _driveService.Files.Create(fileMetadata);
        request.Fields = "id, name, mimeType";
        return await request.ExecuteAsync();
    }

    public async Task<IList<File>> ListSpreadsheets()
    {
        var listRequest = _driveService.Files.List();
        listRequest.Q = "mimeType='application/vnd.google-apps.spreadsheet' and trashed = false";
        listRequest.Fields = "files(id, name, mimeType)";
        var result = await listRequest.ExecuteAsync();
        return result.Files;
    }
}
