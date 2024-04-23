using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace GigRaptorLib.Utilities;

public static class ConfigurationHelper
{
    public static IConfigurationRoot GetConfiguration()
    {
        var configuration = new ConfigurationBuilder()
                            .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
                            .Build();

        return configuration;
    }
}
