using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Managers;
using RaptorSheets.Test.Common.Helpers;
using Xunit;

namespace RaptorSheets.Test.Common.Fixtures;

/// <summary>
/// Shared "clean slate" integration-test fixture: deletes every sheet in the target spreadsheet and
/// recreates the domain's canonical sheets - once per test collection - so every domain's integration
/// tests always start from the same known-good state instead of accumulating stale rows or masking
/// sheet-config regressions across runs. An optional seed step (demo data, etc.) runs after creation.
///
/// Each domain supplies a thin subclass with a public parameterless constructor (required for xUnit's
/// fixture activation) that wires in its own manager type, spreadsheet id, and optional seed step -
/// see StockCleanSlateFixture/GigCleanSlateFixture/JobCleanSlateFixture/HomeCleanSlateFixture.
/// </summary>
public class CleanSlateSheetFixture<TEntity, TManager> : IAsyncLifetime
    where TEntity : class, ISheetEntity, new()
    where TManager : SheetManagerBase<TEntity>
{
    private readonly Func<Dictionary<string, string>, string, TManager> _managerFactory;
    private readonly Func<TManager, Task>? _seedAsync;

    /// <summary>
    /// Public (not just protected) so a domain's own plumbing-test adapter (see
    /// SheetPlumbingTestsBase&lt;TEntity,TManager&gt; in RaptorSheets.Test.Common.Integration) can
    /// construct its own throwaway IGoogleSheetService for the raw-batch-update escape hatch those
    /// scenarios need, without requiring a domain to stand up a whole test-only manager subclass just
    /// to reach it.
    /// </summary>
    public string SpreadsheetId { get; }
    public Dictionary<string, string> Credential { get; private set; } = new();

    public TManager? Manager { get; private set; }
    public bool HasCredentials { get; private set; }

    protected CleanSlateSheetFixture(
        string spreadsheetId,
        Func<Dictionary<string, string>, string, TManager> managerFactory,
        Func<TManager, Task>? seedAsync = null)
    {
        SpreadsheetId = spreadsheetId;
        _managerFactory = managerFactory;
        _seedAsync = seedAsync;
    }

    public async Task InitializeAsync()
    {
        Credential = TestConfigurationHelpers.GetJsonCredential();
        HasCredentials = GoogleCredentialHelpers.IsCredentialFilled(Credential);

        if (!HasCredentials)
        {
            return;
        }

        Manager = _managerFactory(Credential, SpreadsheetId);

        await Manager.DeleteAllSheets();
        await Task.Delay(3000); // allow deletion to propagate

        await Manager.CreateAllSheets();
        await Task.Delay(3000); // allow creation + cross-sheet formulas to settle

        if (_seedAsync != null)
        {
            await _seedAsync(Manager);
            await Task.Delay(2000); // allow seeded data / formulas to recalc
        }

        await AfterSetupAsync();
    }

    /// <summary>
    /// One precondition check covering both failure modes: tabs an earlier test removed, and columns
    /// an earlier test changed. Missing tabs are recreated; drift is reported only.
    ///
    /// Deliberately a single API call. The first version made two - GetAllSheetTabNames plus
    /// GetSheetProperties - and adding two live reads before each of Core's fourteen plumbing tests
    /// destabilised that suite: it went from passing twice in a row to failing once per run, on a
    /// different test each time. The diagnostic was costing more reliability than it bought.
    /// GetSheetProperties already returns both an id (empty when the tab is absent) and the header
    /// row, so one call answers both questions.
    ///
    /// Drift is reported rather than repaired: fixing a column means regenerating the sheet and
    /// discarding its rows, and a false positive would then destroy data on every run.
    /// </summary>
    /// <returns>Sheets recreated, and a description of any that drifted.</returns>
    public async Task<(IReadOnlyList<string> Repaired, IReadOnlyList<string> Drifted)> VerifyPreconditionsAsync(
        IReadOnlyList<string> expectedSheets, CancellationToken cancellationToken = default)
    {
        if (Manager == null)
        {
            return ([], []);
        }

        var properties = await Manager.GetSheetProperties([.. expectedSheets], cancellationToken);

        var missing = properties.Where(p => string.IsNullOrEmpty(p.Id)).Select(p => p.Name).ToList();
        var drifted = new List<string>();

        foreach (var property in properties.Where(p => !string.IsNullOrEmpty(p.Id)))
        {
            var layout = Manager.GetSheetLayout(property.Name);
            if (layout == null)
            {
                continue;
            }

            var expected = layout.Headers.Select(h => h.Name).ToList();
            var actual = (property.Attributes.GetValueOrDefault(Property.HEADERS.GetDescription()) ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            if (expected.SequenceEqual(actual, StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            var unexpected = actual.Except(expected, StringComparer.OrdinalIgnoreCase).ToList();
            var absent = expected.Except(actual, StringComparer.OrdinalIgnoreCase).ToList();
            var detail = unexpected.Count == 0 && absent.Count == 0
                ? "columns reordered"
                : string.Join("; ", new[]
                {
                    unexpected.Count > 0 ? "unexpected: " + string.Join(", ", unexpected) : null,
                    absent.Count > 0 ? "missing: " + string.Join(", ", absent) : null
                }.Where(x => x != null));

            drifted.Add($"{property.Name} ({detail})");
        }

        if (missing.Count > 0)
        {
            await Manager.CreateSheets(missing, cancellationToken);
            await Task.Delay(2000, cancellationToken); // allow creation + cross-sheet formulas to settle
        }

        return (missing, drifted);
    }

    /// <summary>
    /// Extension point for domain-specific post-setup work (e.g. Stock captures a batch-data
    /// snapshot here for its MapFromRangeData tests to consume without an extra live read).
    /// </summary>
    protected virtual Task AfterSetupAsync() => Task.CompletedTask;

    public Task DisposeAsync() => Task.CompletedTask;
}
