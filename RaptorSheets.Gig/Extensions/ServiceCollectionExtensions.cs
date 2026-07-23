using Microsoft.Extensions.DependencyInjection;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Options;
using RaptorSheets.Gig.Managers;

namespace RaptorSheets.Gig.Extensions;

/// <summary>
/// Dependency-injection registration for the Gig domain.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Gig domain's services.
    /// <para>
    /// Always registers <c>ISheetManagerFactory&lt;IGoogleSheetManager&gt;</c>, for hosts where the
    /// spreadsheet and credentials vary per request or per signed-in user.
    /// </para>
    /// <para>
    /// When <paramref name="configureOptions"/> is supplied, also registers a singleton
    /// <c>IGoogleSheetManager</c> bound to that one spreadsheet - the shape a worker or CLI wants,
    /// with credentials bound from configuration once.
    /// </para>
    /// </summary>
    /// <example>
    /// <![CDATA[
    /// // Fixed spreadsheet, bound from configuration
    /// services.AddRaptorSheetsGig(options =>
    /// {
    ///     options.SpreadsheetId = configuration["Sheets:SpreadsheetId"];
    ///     options.AccessToken = configuration["Sheets:AccessToken"];
    /// });
    ///
    /// // Spreadsheet chosen per request
    /// services.AddRaptorSheetsGig();
    /// // ... then, from the factory:
    /// var manager = factory.Create(userAccessToken, userSpreadsheetId);
    /// ]]>
    /// </example>
    public static IServiceCollection AddRaptorSheetsGig(
        this IServiceCollection services,
        Action<RaptorSheetsOptions>? configureOptions = null)
    {
        return services.AddSheetManager<IGoogleSheetManager>(
            "Gig",
            (sheetService, logger) => new GoogleSheetManager(sheetService, logger),
            configureOptions);
    }
}
