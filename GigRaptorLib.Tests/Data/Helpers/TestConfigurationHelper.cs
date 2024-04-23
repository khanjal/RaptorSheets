using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace GigRaptorLib.Tests.Data.Helpers;

public static class TestConfigurationHelper
{
    public static IConfigurationRoot GetConfiguration()
    {
        var configuration = new ConfigurationBuilder()
                            .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
                            .Build();

        return configuration;
    }
}
