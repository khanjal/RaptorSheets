using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RaptorSheets.Core.Managers;
using RaptorSheets.Core.Services;
using Xunit;

namespace RaptorSheets.Core.Tests.Unit.Managers;

public class GoogleSheetManagerBaseTests
{
    // Minimal concrete subclass to exercise the abstract base's constructors/fields, mirroring
    // how RaptorSheets.Gig/Stock's GoogleSheetManager classes now inherit from it.
    private class TestGoogleSheetManager : GoogleSheetManagerBase
    {
        public TestGoogleSheetManager(IGoogleSheetService service, ILogger? logger = null) : base(service, logger) { }
        public TestGoogleSheetManager(string accessToken, string spreadsheetId, ILogger? logger = null) : base(accessToken, spreadsheetId, logger) { }
        public TestGoogleSheetManager(Dictionary<string, string> parameters, string spreadsheetId, ILogger? logger = null) : base(parameters, spreadsheetId, logger) { }

        public IGoogleSheetService ServiceForTest => _googleSheetService;
        public ILogger LoggerForTest => _logger;
    }

    [Fact]
    public void Constructor_WithService_ShouldAssignService()
    {
        var mockService = new Mock<IGoogleSheetService>();

        var manager = new TestGoogleSheetManager(mockService.Object);

        Assert.Same(mockService.Object, manager.ServiceForTest);
    }

    [Fact]
    public void Constructor_WithNullService_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new TestGoogleSheetManager((IGoogleSheetService)null!));
    }

    [Fact]
    public void Constructor_WithoutLogger_ShouldDefaultToNullLogger()
    {
        var mockService = new Mock<IGoogleSheetService>();

        var manager = new TestGoogleSheetManager(mockService.Object);

        Assert.NotNull(manager.LoggerForTest);
        Assert.IsType<NullLogger>(manager.LoggerForTest);
    }

    [Fact]
    public void Constructor_WithLogger_ShouldUseProvidedLogger()
    {
        var mockService = new Mock<IGoogleSheetService>();
        var mockLogger = new Mock<ILogger>();

        var manager = new TestGoogleSheetManager(mockService.Object, mockLogger.Object);

        Assert.Same(mockLogger.Object, manager.LoggerForTest);
    }

    [Fact]
    public void Constructor_WithAccessTokenAndSpreadsheetId_ShouldInitializeWithoutThrowing()
    {
        var manager = new TestGoogleSheetManager("test-token", "test-spreadsheet-id");

        Assert.NotNull(manager.ServiceForTest);
        Assert.NotNull(manager.LoggerForTest);
    }

    [Fact]
    public void Constructor_WithParametersDictionaryAndSpreadsheetId_ShouldInitializeWithoutThrowing()
    {
        // Minimal fake service-account parameters; we only verify construction, not a live call
        var parameters = new Dictionary<string, string>
        {
            { "type", "service_account" },
            { "privateKeyId", "fake-key-id" },
            { "privateKey", "-----BEGIN PRIVATE KEY-----\\nMIIB...fake...\\n-----END PRIVATE KEY-----\\n" },
            { "clientEmail", "test@example.com" },
            { "clientId", "123" }
        };

        var manager = new TestGoogleSheetManager(parameters, "test-spreadsheet-id");

        Assert.NotNull(manager.ServiceForTest);
        Assert.NotNull(manager.LoggerForTest);
    }
}
