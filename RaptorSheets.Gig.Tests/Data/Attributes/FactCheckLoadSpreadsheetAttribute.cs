using RaptorSheets.Test.Common.Attributes;
using RaptorSheets.Test.Common.Helpers;

namespace RaptorSheets.Gig.Tests.Data.Attributes;

/// <summary>
/// Marks a load-tier test, which runs against its own spreadsheet (spreadsheets:test:gigload) rather
/// than the one the contract and workflow tiers share.
///
/// Skips when that setting is absent, so the load tier cannot quietly fall back to the shared
/// spreadsheet and reintroduce the coupling the split exists to remove. Integration tests silently
/// skipping for months is a failure this repo has already had (#124) - the skip reason names the
/// missing setting so an empty run is traceable rather than mysterious.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class FactCheckLoadSpreadsheetAttribute() : FactCheckUserSecretsBaseAttribute(TestConfigurationHelpers.GetGigLoadSpreadsheet());
