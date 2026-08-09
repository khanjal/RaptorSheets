using RaptorSheets.Test.Common.Attributes;
using RaptorSheets.Test.Common.Helpers;

namespace RaptorSheets.Core.Tests.Integration.CoreTest;

[AttributeUsage(AttributeTargets.Method)]
public sealed class FactCheckUserSecretsAttribute() : FactCheckUserSecretsBaseAttribute(TestConfigurationHelpers.GetCoreSpreadsheet());
