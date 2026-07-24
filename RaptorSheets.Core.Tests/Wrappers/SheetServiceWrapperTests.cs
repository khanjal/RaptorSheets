using RaptorSheets.Test.Common.Helpers;
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
        // Arrange - a real (throwaway) key, since the credential layer actually parses it
        var parameters = GoogleCredentialHelpers.CreateServiceAccountParameters();
        var spreadsheetId = "spreadsheet-id";

        // Act
        var wrapper = new SheetServiceWrapper(parameters, spreadsheetId);

        // Assert
        Assert.NotNull(wrapper);
    }

    [Fact]
    public void Wrapper_ShouldThrow_WhenPrivateKeyIsMalformed()
    {
        // Unusable key material fails every subsequent API call, so construction is where the caller
        // should hear about it - not a confusing 401 on the first request.
        var parameters = GoogleCredentialHelpers.CreateMalformedServiceAccountParameters();

        var exception = Assert.Throws<ArgumentException>(
            () => new SheetServiceWrapper(parameters, "spreadsheet-id"));

        Assert.Contains("private key", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("privateKey")]
    [InlineData("clientEmail")]
    public void Wrapper_ShouldThrow_WhenRequiredParameterIsMissing(string missingKey)
    {
        var parameters = GoogleCredentialHelpers.CreateServiceAccountParameters();
        parameters.Remove(missingKey);

        Assert.Throws<ArgumentException>(() => new SheetServiceWrapper(parameters, "spreadsheet-id"));
    }

    [Fact]
    public async Task GetValues_WithAlreadyCancelledToken_ShouldThrowRatherThanIssueTheRequest()
    {
        // Proves the token actually reaches the underlying Google client's ExecuteAsync rather than
        // being an unused decoration on the signature - an already-cancelled token must short-circuit
        // before any network call, so this needs no live credentials or network access to verify.
        var wrapper = new SheetServiceWrapper("ya29.mocked.token", "spreadsheet-id");
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => wrapper.GetValues("Sheet1!A1:A1", cts.Token));
    }
}
