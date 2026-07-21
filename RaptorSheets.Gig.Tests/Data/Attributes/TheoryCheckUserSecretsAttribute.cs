using RaptorSheets.Test.Common.Attributes;
using RaptorSheets.Test.Common.Helpers;

namespace RaptorSheets.Gig.Tests.Data.Attributes;

public sealed class TheoryCheckUserSecrets() : TheoryCheckUserSecretsBase(TestConfigurationHelpers.GetGigSpreadsheet());
