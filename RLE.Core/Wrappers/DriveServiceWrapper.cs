using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using RaptorSheets.Core.Constants;
using File = Google.Apis.Drive.v3.Data.File;

namespace RaptorSheets.Core.Wrappers;

public interface IDriveServiceWrapper
{
    Task<IList<File>> GetSheetFiles();
    Task<IList<File>> SearchSheetFiles(string name);
}
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

    public async Task<IList<File>> GetSheetFiles()
    {
        // Define parameters of request.
        FilesResource.ListRequest listRequest = _driveService.Files.List();
        //listRequest.PageSize = 10;
        listRequest.Q = "mimeType='application/vnd.google-apps.spreadsheet'";

        // List files.
        return (await listRequest.ExecuteAsync()).Files;
    }

    public async Task<IList<File>> SearchSheetFiles(string name)
    {
        // Define parameters of request.
        FilesResource.ListRequest listRequest = _driveService.Files.List();
        //listRequest.PageSize = 10;
        listRequest.Q = $"mimeType='application/vnd.google-apps.spreadsheet' and name contains '{name}'";

        // List files.
        return (await listRequest.ExecuteAsync()).Files;
    }
}
