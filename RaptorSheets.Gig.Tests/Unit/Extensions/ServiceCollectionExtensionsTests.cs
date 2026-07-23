using Microsoft.Extensions.DependencyInjection;
using RaptorSheets.Core.Factories;
using RaptorSheets.Gig.Extensions;
using RaptorSheets.Gig.Managers;
using Xunit;

namespace RaptorSheets.Gig.Tests.Unit.Extensions;

/// <summary>
/// End-to-end check that the Gig registration wires up the real manager. The generic wiring itself
/// is covered in Core's tests; this is the domain-level smoke test.
/// </summary>
public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddRaptorSheetsGig_WithOptions_ShouldResolveTheGigManager()
    {
        var services = new ServiceCollection();

        services.AddRaptorSheetsGig(options =>
        {
            options.SpreadsheetId = "spreadsheet-id";
            options.AccessToken = "ya29.mocked.token";
        });

        var manager = services.BuildServiceProvider().GetService<IGoogleSheetManager>();

        Assert.NotNull(manager);
        Assert.IsType<GoogleSheetManager>(manager, exactMatch: false);
    }

    [Fact]
    public void AddRaptorSheetsGig_WithoutOptions_ShouldResolveTheFactoryForPerRequestSpreadsheets()
    {
        var services = new ServiceCollection();

        services.AddRaptorSheetsGig();

        var factory = services.BuildServiceProvider()
            .GetService<ISheetManagerFactory<IGoogleSheetManager>>();

        Assert.NotNull(factory);

        var manager = factory.Create("ya29.mocked.token", "some-users-spreadsheet");

        Assert.NotNull(manager);
    }
}
