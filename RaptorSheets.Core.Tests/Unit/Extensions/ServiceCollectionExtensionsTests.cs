using Microsoft.Extensions.DependencyInjection;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Factories;
using RaptorSheets.Core.Services;
using RaptorSheets.Test.Common.Helpers;
using Xunit;

namespace RaptorSheets.Core.Tests.Unit.Extensions;

public class ServiceCollectionExtensionsTests
{
    // Stand-in for a domain manager; Core has no concrete manager of its own to register.
    public interface ITestManager
    {
        IGoogleSheetService Service { get; }
    }

    private sealed class TestManager(IGoogleSheetService service) : ITestManager
    {
        public IGoogleSheetService Service { get; } = service;
    }

    private static IServiceCollection AddTestManager(
        IServiceCollection services,
        string domainName = "Test",
        Action<Options.RaptorSheetsOptions>? configure = null)
    {
        return services.AddSheetManager<ITestManager>(
            domainName,
            (service, _) => new TestManager(service),
            configure);
    }

    [Fact]
    public void AddSheetManager_WithoutOptions_ShouldStillRegisterTheFactory()
    {
        // A multi-tenant host supplies no fixed spreadsheet; the factory is the whole point for it.
        var provider = AddTestManager(new ServiceCollection()).BuildServiceProvider();

        var factory = provider.GetService<ISheetManagerFactory<ITestManager>>();

        Assert.NotNull(factory);
    }

    [Fact]
    public void AddSheetManager_WithoutOptions_ShouldNotRegisterTheManagerItself()
    {
        // There is no spreadsheet to bind it to, so resolving one directly must not silently succeed.
        var provider = AddTestManager(new ServiceCollection()).BuildServiceProvider();

        Assert.Null(provider.GetService<ITestManager>());
    }

    [Fact]
    public void AddSheetManager_WithOptions_ShouldResolveTheManager()
    {
        var provider = AddTestManager(new ServiceCollection(), configure: options =>
        {
            options.SpreadsheetId = "spreadsheet-id";
            options.AccessToken = "ya29.mocked.token";
        }).BuildServiceProvider();

        var manager = provider.GetService<ITestManager>();

        Assert.NotNull(manager);
        Assert.NotNull(manager.Service);
    }

    [Fact]
    public void Factory_ShouldCreateManagersForASpreadsheetChosenAtRuntime()
    {
        var provider = AddTestManager(new ServiceCollection()).BuildServiceProvider();
        var factory = provider.GetRequiredService<ISheetManagerFactory<ITestManager>>();

        var first = factory.Create("ya29.mocked.token", "spreadsheet-one");
        var second = factory.Create("ya29.mocked.token", "spreadsheet-two");

        Assert.NotNull(first);
        Assert.NotNull(second);
        Assert.NotSame(first, second);
    }

    [Fact]
    public void TwoDomains_ShouldKeepTheirOwnOptionsRatherThanOverwritingEachOther()
    {
        // Named options are what make this work; a plain Configure<T> would have the second
        // registration clobber the first, silently pointing one domain at the other's spreadsheet.
        var services = new ServiceCollection();

        services.AddSheetManager<ITestManager>("DomainA", (service, _) => new TestManager(service), options =>
        {
            options.SpreadsheetId = "spreadsheet-a";
            options.AccessToken = "token-a";
        });

        services.AddSheetManager<IOtherTestManager>("DomainB", (service, _) => new OtherTestManager(service), options =>
        {
            options.SpreadsheetId = "spreadsheet-b";
            options.AccessToken = "token-b";
        });

        var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetService<ITestManager>());
        Assert.NotNull(provider.GetService<IOtherTestManager>());
    }

    [Fact]
    public void MissingSpreadsheetId_ShouldThrowAtResolutionWithAClearMessage()
    {
        var provider = AddTestManager(new ServiceCollection(), "Gig", options =>
        {
            options.AccessToken = "ya29.mocked.token";
        }).BuildServiceProvider();

        var exception = Assert.Throws<InvalidOperationException>(() => provider.GetService<ITestManager>());

        Assert.Contains("SpreadsheetId", exception.Message, StringComparison.Ordinal);
        Assert.Contains("Gig", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void NoCredential_ShouldThrowAtResolution()
    {
        var provider = AddTestManager(new ServiceCollection(), configure: options =>
        {
            options.SpreadsheetId = "spreadsheet-id";
        }).BuildServiceProvider();

        var exception = Assert.Throws<InvalidOperationException>(() => provider.GetService<ITestManager>());

        Assert.Contains("neither was supplied", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void BothCredentialKinds_ShouldThrowRatherThanPickOneSilently()
    {
        var provider = AddTestManager(new ServiceCollection(), configure: options =>
        {
            options.SpreadsheetId = "spreadsheet-id";
            options.AccessToken = "ya29.mocked.token";
            options.ServiceAccountCredentials = GoogleCredentialHelpers.CreateServiceAccountParameters();
        }).BuildServiceProvider();

        var exception = Assert.Throws<InvalidOperationException>(() => provider.GetService<ITestManager>());

        Assert.Contains("both were supplied", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ServiceAccountCredentials_ShouldResolveTheManager()
    {
        var provider = AddTestManager(new ServiceCollection(), configure: options =>
        {
            options.SpreadsheetId = "spreadsheet-id";
            options.ServiceAccountCredentials = GoogleCredentialHelpers.CreateServiceAccountParameters();
        }).BuildServiceProvider();

        Assert.NotNull(provider.GetService<ITestManager>());
    }

    public interface IOtherTestManager;

    private sealed class OtherTestManager(IGoogleSheetService service) : IOtherTestManager
    {
        public IGoogleSheetService Service { get; } = service;
    }
}
