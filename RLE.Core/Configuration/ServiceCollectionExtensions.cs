using Microsoft.Extensions.DependencyInjection;
using RLE.Core.Services;
using RLE.Core.Wrappers;

namespace RLE.Core.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services) =>
        services
            .AddScoped<ISheetServiceWrapper, SheetServiceWrapper>()
            .AddScoped<IGoogleSheetService, GoogleSheetService>();
}
