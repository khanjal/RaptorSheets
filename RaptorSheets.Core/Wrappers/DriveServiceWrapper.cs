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
        // This query is safe to use with restricted scopes like `drive.file`, but the
        // response is still constrained by the caller's OAuth grant. Under `drive.file`,
        // Google typically returns only files the current user created with or explicitly
        // opened through the app. Shared files from other accounts usually require a broader
        // scope such as `drive.readonly` or `drive` to appear automatically in list results.
        listRequest.Q = "mimeType='application/vnd.google-apps.spreadsheet' and trashed = false and ('me' in owners or sharedWithMe = true)";
        // These flags allow callers with broader Drive scopes to include shared-drive content.
        // They do not expand visibility on their own and do not override restricted scopes.
        listRequest.SupportsAllDrives = true;
        listRequest.IncludeItemsFromAllDrives = true;
        listRequest.Fields = "files(id, name, mimeType)";
        var result = await listRequest.ExecuteAsync();
        return result.Files;
    }
}
