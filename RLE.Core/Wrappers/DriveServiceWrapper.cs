using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using RLE.Core.Constants;
using File = Google.Apis.Drive.v3.Data.File;

namespace RLE.Core.Wrappers;

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

    public DriveServiceWrapper(Dictionary<string, string> parameters)
    {
        var jsonCredential = new JsonCredentialParameters
        {
            Type = parameters["type"].Trim(),
            PrivateKeyId = parameters["privateKeyId"].Trim(),
            PrivateKey = parameters["privateKey"].Trim(),
            ClientEmail = parameters["clientEmail"].Trim(),
            ClientId = parameters["clientId"].Trim(),
        };

        var credential = GoogleCredential.FromJsonParameters(jsonCredential);

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
