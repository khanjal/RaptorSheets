using Google.Apis.Auth.OAuth2;
using RaptorSheets.Core.Wrappers;
using Xunit;

namespace RaptorSheets.Core.Tests.Wrappers;

public class SheetServiceWrapperTests
{
    [Fact]
    public void Wrapper_CanBeConstructed_WithAccessToken()
    {
        // Arrange
        var token = "ya29.mocked.token";
        var spreadsheetId = "spreadsheet-id";

        // Act
        var wrapper = new SheetServiceWrapper(token, spreadsheetId);

        // Assert
        Assert.NotNull(wrapper);
    }

    [Fact]
    public void Wrapper_CanBeConstructed_WithServiceAccountParameters()
    {
        // Arrange - minimal fake parameters; we only verify construction, not a live call
        var parameters = new Dictionary<string, string>
        {
            { "type", "service_account" },
            { "privateKeyId", "fake-key-id" },
            { "privateKey", "-----BEGIN PRIVATE KEY-----\\nMIIB...fake...\\n-----END PRIVATE KEY-----\\n" },
            { "clientEmail", "test@example.com" },
            { "clientId", "123" }
        };

        var spreadsheetId = "spreadsheet-id";

        // Act
        var wrapper = new SheetServiceWrapper(parameters, spreadsheetId);

        // Assert
        Assert.NotNull(wrapper);
    }
}
