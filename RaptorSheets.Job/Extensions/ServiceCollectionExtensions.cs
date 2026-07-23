using Microsoft.Extensions.DependencyInjection;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Options;
using RaptorSheets.Job.Managers;

namespace RaptorSheets.Job.Extensions;

/// <summary>
/// Dependency-injection registration for the Job domain.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Job domain's services.
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
    /// services.AddRaptorSheetsJob(options =>
    /// {
    ///     options.SpreadsheetId = configuration["Sheets:SpreadsheetId"];
    ///     options.AccessToken = configuration["Sheets:AccessToken"];
    /// });
    ///
    /// // Spreadsheet chosen per request
    /// services.AddRaptorSheetsJob();
    /// // ... then, from the factory:
    /// var manager = factory.Create(userAccessToken, userSpreadsheetId);
    /// ]]>
    /// </example>
    public static IServiceCollection AddRaptorSheetsJob(
        this IServiceCollection services,
        Action<RaptorSheetsOptions>? configureOptions = null)
    {
        return services.AddSheetManager<IGoogleSheetManager>(
            "Job",
            (sheetService, logger) => new GoogleSheetManager(sheetService, logger),
            configureOptions);
    }
}
