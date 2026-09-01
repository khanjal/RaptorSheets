using RaptorSheets.Core.Entities;
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
    /// Checks that the spreadsheet still has the tabs the domain expects, and recreates any that are
    /// missing. Opt-in: a test class calls it before its own work.
    ///
    /// The reset above runs once per collection, so a test that deletes a sheet and fails before
    /// restoring it leaves every later test reading a spreadsheet that no longer matches the
    /// canonical layout - which is why a failure here usually surfaced two or three tests away from
    /// its cause (#130). The common case costs one metadata read and repairs nothing.
    ///
    /// The point is as much diagnostic as corrective: it turns "an unrelated test fails later" into
    /// a named warning at the moment the damage is found. It checks tab presence only, not column
    /// drift, which needs grid data and is the more expensive half.
    /// </summary>
    /// <returns>The sheets it had to recreate - empty when the spreadsheet was already intact.</returns>
    public async Task<IReadOnlyList<string>> VerifyAndRepairAsync(IReadOnlyList<string> expectedSheets, CancellationToken cancellationToken = default)
    {
        if (Manager == null)
        {
            return [];
        }

        var present = await Manager.GetAllSheetTabNames(cancellationToken);
        var missing = expectedSheets
            .Where(name => !present.Any(tab => string.Equals(tab, name, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        if (missing.Count == 0)
        {
            return [];
        }

        await Manager.CreateSheets(missing, cancellationToken);
        await Task.Delay(2000, cancellationToken); // allow creation + cross-sheet formulas to settle

        return missing;
    }

    /// <summary>
    /// Extension point for domain-specific post-setup work (e.g. Stock captures a batch-data
    /// snapshot here for its MapFromRangeData tests to consume without an extra live read).
    /// </summary>
    protected virtual Task AfterSetupAsync() => Task.CompletedTask;

    public Task DisposeAsync() => Task.CompletedTask;
}
