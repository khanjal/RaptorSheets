using RaptorSheets.Test.Common.Attributes;
using RaptorSheets.Test.Common.Helpers;

namespace RaptorSheets.Job.Tests.Data.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public sealed class TheoryCheckUserSecretsAttribute() : TheoryCheckUserSecretsBaseAttribute(TestConfigurationHelpers.GetJobSpreadsheet());
