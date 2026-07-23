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
    where TManager : GoogleSheetManagerBase<TEntity>
{
    private readonly Func<Dictionary<string, string>, string, TManager> _managerFactory;
    private readonly Func<TManager, Task>? _seedAsync;

    protected string SpreadsheetId { get; }
    protected Dictionary<string, string> Credential { get; private set; } = new();

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
    /// Extension point for domain-specific post-setup work (e.g. Stock captures a batch-data
    /// snapshot here for its MapFromRangeData tests to consume without an extra live read).
    /// </summary>
    protected virtual Task AfterSetupAsync() => Task.CompletedTask;

    public Task DisposeAsync() => Task.CompletedTask;
}
