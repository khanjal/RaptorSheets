using RaptorSheets.Test.Common.Attributes;
using RaptorSheets.Test.Common.Helpers;

namespace RaptorSheets.Stock.Tests.Data.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public sealed class TheoryCheckUserSecretsAttribute() : TheoryCheckUserSecretsBaseAttribute(TestConfigurationHelpers.GetStockSpreadsheet());
