using RaptorSheets.Test.Common.Attributes;
using RaptorSheets.Test.Common.Helpers;

namespace RaptorSheets.Home.Tests.Data.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public sealed class FactCheckUserSecrets() : FactCheckUserSecretsBase(TestConfigurationHelpers.GetHomeSpreadsheet());
