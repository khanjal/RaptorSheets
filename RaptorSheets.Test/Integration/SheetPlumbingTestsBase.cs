using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Managers;
using Xunit;

namespace RaptorSheets.Test.Common.Integration;

/// <summary>
/// Generic, live "does the library's own plumbing work" test bodies shared across every domain -
/// delete/recreate a sheet, the temp-sheet safety net, missing-column self-heal (on both a formula
/// sheet and a plain input sheet), multi-column self-heal, column-reorder tolerance, an unexpected
/// extra column, and reapply-formatting-preserves-values. First proven live as a bespoke, Core-only
/// suite (see git history for RaptorSheets.Core.Tests/Integration/CoreTest/CoreSheetsIntegrationTests.cs)
/// before being generalized here so Gig/Job/Home/Stock don't each have to re-author the same raw
/// DeleteDimension/MoveDimension/InsertDimension orchestration against their own real schemas.
///
/// Deliberately does NOT cover row-level/business-value assertions (specific computed totals,
/// workflow correctness) - those can't be generalized across domains and stay in each domain's own
/// test file. What IS generalized only cares that the mechanics work, not what a row means.
///
/// A concrete domain test class supplies <see cref="Manager"/> (from its own CleanSlateSheetFixture)
/// and <see cref="Config"/>, then re-declares each virtual method with `override` and its own
/// domain's FactCheckUserSecrets attribute (xUnit needs the attribute on the concrete method for
/// discovery/skip - it can't live here since the skip attribute type itself is domain-specific).
/// </summary>
public abstract class SheetPlumbingTestsBase<TEntity, TManager>
    where TEntity : class, ISheetEntity, new()
    where TManager : SheetManagerBase<TEntity>, ISheetManager<TEntity>
{
    /// <summary>RowId every scenario here writes its own scratch row(s) at/from - matches the
    /// RowId-2-is-scratch convention established by CoreSheetsIntegrationTests.</summary>
    protected const int TestRowId = 2;

    private const int ModestReseedRowCount = 3;

    protected abstract TManager? Manager { get; }
    protected abstract PlumbingTestConfig<TEntity> Config { get; }

    protected void SkipIfNoCredentials()
    {
        if (Manager == null)
        {
            Assert.Fail("Google Sheets credentials not available. Configure user secrets to run integration tests.");
        }
    }

    private static List<MessageEntity> CriticalErrors(TEntity result) =>
        result.Messages
            .Where(m => m.Level == MessageLevel.ERROR.GetDescription() && !IsExpectedError(m.Message))
            .ToList();

    private static bool IsExpectedError(string message) =>
        message.Contains("not supported") ||
        message.Contains("already exists") ||
        message.Contains("header issue") ||
        message.Contains("No data to change");

    private async Task<int> GetSheetIdAsync(string sheetName)
    {
        var property = (await Manager!.GetSheetProperties([sheetName]))[0];
        return int.Parse(property.Id);
    }

    private async Task TryDeleteNonCanonicalSheetAsync(string sheetName)
    {
        var properties = await Manager!.GetSheetProperties([sheetName]);
        var property = properties.FirstOrDefault(p => !string.IsNullOrEmpty(p.Id));

        if (property == null || !int.TryParse(property.Id, out var sheetId))
        {
            return;
        }

        var deleteRequest = new BatchUpdateSpreadsheetRequest
        {
            Requests = [new Request { DeleteSheet = new DeleteSheetRequest { SheetId = sheetId } }]
        };

        await Config.ExecuteRawBatchUpdateAsync(deleteRequest, default);
        await Task.Delay(Config.StructureSettleDelay);
    }

    private static Request DeleteColumnRequest(int sheetId, int columnIndex) => new()
    {
        DeleteDimension = new DeleteDimensionRequest
        {
            Range = new DimensionRange { SheetId = sheetId, Dimension = "COLUMNS", StartIndex = columnIndex, EndIndex = columnIndex + 1 }
        }
    };

    /// <summary>
    /// Leaves the input sheet non-trivially populated after a test that wipes it. Uses the domain's
    /// own richer seed step when supplied (<see cref="PlumbingTestConfig{TEntity}.BulkReseedAsync"/>),
    /// otherwise falls back to a handful of <see cref="PlumbingTestConfig{TEntity}.BuildTestRow"/> rows.
    /// </summary>
    private async Task ReseedAsync()
    {
        if (Config.BulkReseedAsync != null)
        {
            await Config.BulkReseedAsync(default);
            return;
        }

        for (var i = 0; i < ModestReseedRowCount; i++)
        {
            await Manager!.ChangeSheetData([Config.InputSheetName], Config.BuildTestRow(TestRowId + i));
        }
    }

    public virtual async Task CreateAllSheets_ThenReadStructure_HasExpectedHeaders()
    {
        SkipIfNoCredentials();

        var structures = await Manager!.GetAllLiveSheetStructures();

        Assert.True(structures.ContainsKey(Config.InputSheetName));
        if (Config.DependentSheetName != null)
        {
            Assert.True(structures.ContainsKey(Config.DependentSheetName));
        }
    }

    /// <summary>
    /// Direct regression test for the Copilot-caught field-mask bug on #99: GenerateColumnFormatRequest
    /// previously reused GenerateRepeatCellRequest's "*" mask, which would have blanked every value in
    /// the column since the request never set UserEnteredValue. Only a real round trip proves the fix.
    /// </summary>
    public virtual async Task ReapplyFormatting_OnPopulatedColumn_PreservesExistingValues()
    {
        SkipIfNoCredentials();

        var writeResult = await Manager!.ChangeSheetData([Config.InputSheetName], Config.BuildTestRow(TestRowId));
        Assert.Empty(CriticalErrors(writeResult));
        await Task.Delay(Config.SettleDelay);

        var reapplyResult = await Manager!.ReapplyFormatting(Config.InputSheetName);
        Assert.Empty(CriticalErrors(reapplyResult));
        await Task.Delay(Config.SettleDelay);

        var readResult = await Manager!.GetSheets([Config.InputSheetName]);
        Assert.True(Config.ContainsTestRow(readResult, TestRowId));
    }

    /// <summary>Deleting the dependent (formula) sheet must not disturb the input sheet it reads from.</summary>
    public virtual async Task DeleteDependentSheet_LeavesInputSheetIntact_ThenRecreatesIt()
    {
        SkipIfNoCredentials();
        if (Config.DependentSheetName == null)
        {
            return;
        }

        var dependentSheet = Config.DependentSheetName;

        try
        {
            await Manager!.DeleteSheets([dependentSheet]);

            var tabNamesAfterDelete = await Manager!.GetAllSheetTabNames();
            Assert.DoesNotContain(dependentSheet, tabNamesAfterDelete);
            Assert.Contains(Config.InputSheetName, tabNamesAfterDelete);
        }
        finally
        {
            var createResult = await Manager!.CreateSheets([dependentSheet]);
            Assert.Empty(CriticalErrors(createResult));
            await Task.Delay(Config.StructureSettleDelay);
        }

        await Task.Delay(Config.StructureSettleDelay);
        var tabNamesAfterRecreate = await Manager!.GetAllSheetTabNames();
        Assert.Contains(dependentSheet, tabNamesAfterRecreate);
    }

    /// <summary>
    /// Deletes the sheet a calculated/dependent sheet depends on, confirms self-heal recreates it AND
    /// that the dependent sheet's own formulas (SheetRegistry.GetDependents/RefreshDependentSheetsAsync)
    /// still exist and reference something live afterward, rather than staying silently broken against
    /// a stale sheet reference.
    /// </summary>
    public virtual async Task DeleteInputSheet_ThenRecreate_DependentFormulaStillComputes()
    {
        SkipIfNoCredentials();
        if (Config.DependentSheetName == null)
        {
            return;
        }

        var writeResult = await Manager!.ChangeSheetData([Config.InputSheetName], Config.BuildTestRow(TestRowId));
        Assert.Empty(CriticalErrors(writeResult));
        await Task.Delay(Config.SettleDelay);

        // Not asserting on this result: the recreation below is a side effect of the next GetSheets
        // call regardless of what DeleteSheets reports, and must always run - a blocking assertion
        // here would risk leaving the input sheet permanently deleted for every later test.
        await Manager!.DeleteSheets([Config.InputSheetName]);
        await Task.Delay(Config.StructureSettleDelay);

        try
        {
            var healResult = await Manager!.GetSheets([Config.InputSheetName, Config.DependentSheetName]);
            Assert.Contains(healResult.Messages, m => m.Message.Contains("Created missing sheets"));
            await Task.Delay(Config.SettleDelay);

            var refillResult = await Manager!.ChangeSheetData([Config.InputSheetName], Config.BuildTestRow(TestRowId));
            Assert.Empty(CriticalErrors(refillResult));
            await Task.Delay(Config.SettleDelay);

            var finalRead = await Manager!.GetSheets([Config.InputSheetName, Config.DependentSheetName]);
            Assert.True(Config.ContainsTestRow(finalRead, TestRowId));

            var dependentStructure = await Manager!.GetLiveSheetStructure(Config.DependentSheetName);
            Assert.NotNull(dependentStructure);
            Assert.Contains(dependentStructure!.Headers, h => !string.IsNullOrEmpty(h.Formula));
        }
        finally
        {
            await ReseedAsync();
        }
    }

    public virtual async Task DeleteAllSheets_ThenCreateAllSheets_UsesTempSheetSafetyNet()
    {
        SkipIfNoCredentials();

        // Google always starts a new spreadsheet with a default "Sheet1" tab, and any earlier run of
        // this very test leaves a "TempSheet" behind (both non-canonical, so DeleteAllSheets/
        // CreateAllSheets never touch either). Remove both here so deleting canonical sheets genuinely
        // leaves nothing behind and the safety net actually has to create a fresh TempSheet.
        await TryDeleteNonCanonicalSheetAsync("Sheet1");
        await TryDeleteNonCanonicalSheetAsync(SheetManagerBase.TempSheetName);

        try
        {
            var deleteResult = await Manager!.DeleteAllSheets();
            Assert.Contains(deleteResult.Messages, m => m.Message.Contains("safety sheet"));
            await Task.Delay(Config.StructureSettleDelay);
        }
        finally
        {
            var createResult = await Manager!.CreateAllSheets();
            Assert.Empty(CriticalErrors(createResult));
            await Task.Delay(Config.StructureSettleDelay);

            await ReseedAsync();
        }

        await Task.Delay(Config.StructureSettleDelay);

        var tabNames = await Manager!.GetAllSheetTabNames();
        Assert.Contains(Config.InputSheetName, tabNames);
        if (Config.DependentSheetName != null)
        {
            Assert.Contains(Config.DependentSheetName, tabNames);
        }
    }

    /// <summary>
    /// Simulates a column being manually deleted outside the library (via a raw DeleteDimension
    /// request), then confirms GetSheets' auto-heal restores it with its Formula intact - on the
    /// dependent/formula sheet's first non-key column (index 0 is always the key/category column by
    /// this codebase's own rollup convention - see Config.DependentSheetName's own doc comment).
    /// </summary>
    public virtual async Task MissingColumn_OnDependentSheet_SelfHealRestoresFormula()
    {
        SkipIfNoCredentials();
        if (Config.DependentSheetName == null)
        {
            return;
        }

        var writeResult = await Manager!.ChangeSheetData([Config.InputSheetName], Config.BuildTestRow(TestRowId));
        Assert.Empty(CriticalErrors(writeResult));
        await Task.Delay(Config.SettleDelay);

        var dependentSheetId = await GetSheetIdAsync(Config.DependentSheetName);
        var dependentModel = Manager!.GetSheetLayout(Config.DependentSheetName)!;
        dependentModel.Headers.UpdateColumns();
        var formulaHeader = dependentModel.Headers.OrderBy(h => h.Index).Skip(1).First();

        Assert.True(await Config.ExecuteRawBatchUpdateAsync(
            new BatchUpdateSpreadsheetRequest { Requests = [DeleteColumnRequest(dependentSheetId, formulaHeader.Index)] }, default));
        await Task.Delay(Config.StructureSettleDelay);

        var healingRead = await Manager!.GetSheets([Config.DependentSheetName]);
        Assert.Contains(healingRead.Messages, m => m.Message.Contains("Inserting column") && m.Message.Contains(formulaHeader.Name));
        await Task.Delay(Config.SettleDelay);

        var afterHealStructure = await Manager!.GetLiveSheetStructure(Config.DependentSheetName);
        Assert.Contains(afterHealStructure!.Headers, h => h.Name == formulaHeader.Name && !string.IsNullOrEmpty(h.Formula));
    }

    /// <summary>
    /// Same self-heal, but on the input sheet's <see cref="PlumbingTestConfig{TEntity}.TestColumnName"/> -
    /// a genuine user-INPUT column, not a formula column, and with real data already in it. Self-heal
    /// restores the column's structure (format) fully, but deleting a column deletes its cell content
    /// along with it - there is nothing left to recover the row's prior value from.
    /// </summary>
    public virtual async Task MissingColumn_OnInputSheet_RestoresStructureButNotData()
    {
        SkipIfNoCredentials();

        var writeResult = await Manager!.ChangeSheetData([Config.InputSheetName], Config.BuildTestRow(TestRowId));
        Assert.Empty(CriticalErrors(writeResult));
        await Task.Delay(Config.SettleDelay);

        var inputSheetId = await GetSheetIdAsync(Config.InputSheetName);
        var inputModel = Manager!.GetSheetLayout(Config.InputSheetName)!;
        inputModel.Headers.UpdateColumns();
        var testColumnHeader = inputModel.Headers.First(h => h.Name == Config.TestColumnName);

        Assert.True(await Config.ExecuteRawBatchUpdateAsync(
            new BatchUpdateSpreadsheetRequest { Requests = [DeleteColumnRequest(inputSheetId, testColumnHeader.Index)] }, default));
        await Task.Delay(Config.StructureSettleDelay);

        var healingRead = await Manager!.GetSheets([Config.InputSheetName]);
        Assert.Contains(healingRead.Messages, m => m.Message.Contains("Inserting column") && m.Message.Contains(Config.TestColumnName));
        await Task.Delay(Config.SettleDelay);

        var structure = await Manager!.GetLiveSheetStructure(Config.InputSheetName);
        var restoredHeader = structure!.Headers.First(h => h.Name == Config.TestColumnName);
        if (testColumnHeader.Format != null)
        {
            Assert.Equal(testColumnHeader.Format, restoredHeader.Format);
        }

        // The pre-existing row's value for that column is genuinely gone - the round-tripped row no
        // longer matches what BuildTestRow originally wrote.
        var afterHeal = await Manager!.GetSheets([Config.InputSheetName]);
        Assert.False(Config.ContainsTestRow(afterHeal, TestRowId));
    }

    /// <summary>
    /// Deletes every formula column on the dependent sheet at once (right-to-left, so later deletes
    /// aren't shifted by earlier ones) and confirms self-heal restores all of them at their canonical
    /// positions - not just that the data is findable by name somewhere.
    /// </summary>
    public virtual async Task MultipleMissingColumns_OnDependentSheet_RestoresAllAtCorrectPositions()
    {
        SkipIfNoCredentials();
        if (Config.DependentSheetName == null)
        {
            return;
        }

        var writeResult = await Manager!.ChangeSheetData([Config.InputSheetName], Config.BuildTestRow(TestRowId));
        Assert.Empty(CriticalErrors(writeResult));
        await Task.Delay(Config.SettleDelay);

        var dependentSheetId = await GetSheetIdAsync(Config.DependentSheetName);
        var dependentModel = Manager!.GetSheetLayout(Config.DependentSheetName)!;
        dependentModel.Headers.UpdateColumns();
        var canonicalOrder = dependentModel.Headers.OrderBy(h => h.Index).Select(h => h.Name).ToList();
        var formulaHeaders = dependentModel.Headers.Where(h => h.Index > 0).OrderByDescending(h => h.Index).ToList();
        Assert.True(formulaHeaders.Count >= 2, $"{Config.DependentSheetName} needs at least 2 formula columns for this scenario.");

        var deleteRequests = formulaHeaders.Select(h => DeleteColumnRequest(dependentSheetId, h.Index)).ToList();
        Assert.True(await Config.ExecuteRawBatchUpdateAsync(new BatchUpdateSpreadsheetRequest { Requests = deleteRequests }, default));
        await Task.Delay(Config.StructureSettleDelay);

        var healingRead = await Manager!.GetSheets([Config.DependentSheetName]);
        foreach (var header in formulaHeaders)
        {
            Assert.Contains(healingRead.Messages, m => m.Message.Contains("Inserting column") && m.Message.Contains(header.Name));
        }
        await Task.Delay(Config.SettleDelay);

        var structure = await Manager!.GetLiveSheetStructure(Config.DependentSheetName);
        var liveOrder = structure!.Headers.OrderBy(h => h.Index).Select(h => h.Name).ToList();
        Assert.Equal(canonicalOrder, liveOrder);
    }

    /// <summary>
    /// Simulates a user manually dragging <see cref="PlumbingTestConfig{TEntity}.TestColumnName"/> to
    /// swap places with its next neighbor (no deletion, nothing missing). Confirms reads stay correct
    /// (header matching is name-based, not positional), the mismatch is reported as a warning rather
    /// than silently ignored, the library does NOT auto-correct the live sheet's order on its own, and
    /// a subsequent write still lands in the correct columns despite the reorder. Requires
    /// TestColumnName not be the input sheet's last configured header.
    /// </summary>
    public virtual async Task ColumnsReordered_ReadsAndWritesStayCorrect_LibraryDoesNotAutoCorrectOrder()
    {
        SkipIfNoCredentials();

        var writeResult = await Manager!.ChangeSheetData([Config.InputSheetName], Config.BuildTestRow(TestRowId));
        Assert.Empty(CriticalErrors(writeResult));
        await Task.Delay(Config.SettleDelay);

        var inputSheetId = await GetSheetIdAsync(Config.InputSheetName);
        var inputModel = Manager!.GetSheetLayout(Config.InputSheetName)!;
        inputModel.Headers.UpdateColumns();
        var canonicalOrder = inputModel.Headers.OrderBy(h => h.Index).Select(h => h.Name).ToList();
        var testColumnHeader = inputModel.Headers.First(h => h.Name == Config.TestColumnName);
        var nextHeader = inputModel.Headers.First(h => h.Index == testColumnHeader.Index + 1);

        try
        {
            var moveRequest = new BatchUpdateSpreadsheetRequest
            {
                Requests =
                [
                    new Request
                    {
                        MoveDimension = new MoveDimensionRequest
                        {
                            Source = new DimensionRange { SheetId = inputSheetId, Dimension = "COLUMNS", StartIndex = testColumnHeader.Index, EndIndex = testColumnHeader.Index + 1 },
                            DestinationIndex = nextHeader.Index + 1
                        }
                    }
                ]
            };
            Assert.True(await Config.ExecuteRawBatchUpdateAsync(moveRequest, default));
            await Task.Delay(Config.StructureSettleDelay);

            var readResult = await Manager!.GetSheets([Config.InputSheetName]);
            Assert.True(Config.ContainsTestRow(readResult, TestRowId));
            Assert.Contains(readResult.Messages, m => m.Message.Contains("should be"));

            var structure = await Manager!.GetLiveSheetStructure(Config.InputSheetName);
            var liveOrder = structure!.Headers.OrderBy(h => h.Index).Select(h => h.Name).ToList();
            Assert.NotEqual(canonicalOrder, liveOrder);

            var rewriteResult = await Manager!.ChangeSheetData([Config.InputSheetName], Config.BuildTestRow(TestRowId));
            Assert.Empty(CriticalErrors(rewriteResult));
            await Task.Delay(Config.SettleDelay);

            var finalRead = await Manager!.GetSheets([Config.InputSheetName]);
            Assert.True(Config.ContainsTestRow(finalRead, TestRowId));
        }
        finally
        {
            // Restore canonical order by delete + self-heal recreate rather than reverse-engineering
            // MoveDimensionRequest's exact before/after-removal index semantics (genuinely easy to get
            // subtly wrong) - reuses the delete/self-heal path already proven reliable above.
            await Manager!.DeleteSheets([Config.InputSheetName]);
            await Task.Delay(Config.StructureSettleDelay);
            await Manager!.GetSheets([Config.InputSheetName]); // triggers self-heal recreation
            await Task.Delay(Config.StructureSettleDelay);

            var restoredStructure = await Manager!.GetLiveSheetStructure(Config.InputSheetName);
            var restoredOrder = restoredStructure!.Headers.OrderBy(h => h.Index).Select(h => h.Name).ToList();
            Assert.Equal(canonicalOrder, restoredOrder);

            await ReseedAsync();
        }
    }

    /// <summary>
    /// Simulates a user manually inserting their own extra column (with their own header text and a
    /// value on the existing row). Confirms known columns keep reading/writing correctly despite the
    /// shift, the unknown column is flagged rather than silently dropped or deleted, and - the Core-
    /// level field-mask finding tracked as issue #101 - that a subsequent ordinary write DOES clear
    /// the unrecognized column's value (GenerateUpdateCellsRequest's field mask has no way to say
    /// "leave this cell alone" for a column it doesn't know about).
    /// </summary>
    public virtual async Task ExtraUnexpectedColumn_IsFlaggedNotRemoved_KnownColumnsUnaffected()
    {
        SkipIfNoCredentials();

        var writeResult = await Manager!.ChangeSheetData([Config.InputSheetName], Config.BuildTestRow(TestRowId));
        Assert.Empty(CriticalErrors(writeResult));
        await Task.Delay(Config.SettleDelay);

        var inputSheetId = await GetSheetIdAsync(Config.InputSheetName);
        const int extraColumnIndex = 1; // right after the first column - always a valid insertion point.
        const string extraColumnHeaderName = "PlumbingTestExtraColumn";
        const string extraColumnValue = "plumbing test value";

        var insertRequest = new BatchUpdateSpreadsheetRequest
        {
            Requests = [new Request { InsertDimension = new InsertDimensionRequest { Range = new DimensionRange { SheetId = inputSheetId, Dimension = "COLUMNS", StartIndex = extraColumnIndex, EndIndex = extraColumnIndex + 1 }, InheritFromBefore = false } }]
        };
        Assert.True(await Config.ExecuteRawBatchUpdateAsync(insertRequest, default));
        await Task.Delay(Config.StructureSettleDelay);

        var writeExtraColumnRequest = new BatchUpdateSpreadsheetRequest
        {
            Requests =
            [
                new Request
                {
                    UpdateCells = new UpdateCellsRequest
                    {
                        Fields = "userEnteredValue",
                        Range = new GridRange { SheetId = inputSheetId, StartRowIndex = 0, EndRowIndex = 2, StartColumnIndex = extraColumnIndex, EndColumnIndex = extraColumnIndex + 1 },
                        Rows =
                        [
                            new RowData { Values = [new CellData { UserEnteredValue = new ExtendedValue { StringValue = extraColumnHeaderName } }] },
                            new RowData { Values = [new CellData { UserEnteredValue = new ExtendedValue { StringValue = extraColumnValue } }] }
                        ]
                    }
                }
            ]
        };
        Assert.True(await Config.ExecuteRawBatchUpdateAsync(writeExtraColumnRequest, default));
        await Task.Delay(Config.SettleDelay);

        try
        {
            var readResult = await Manager!.GetSheets([Config.InputSheetName]);
            Assert.True(Config.ContainsTestRow(readResult, TestRowId));
            Assert.Contains(readResult.Messages, m => m.Message.Contains("Extra column") && m.Message.Contains(extraColumnHeaderName));

            var beforeWrite = await Manager!.GetLiveSheetRawValues(Config.InputSheetName);
            Assert.Contains(beforeWrite[1], v => v?.Trim() == extraColumnValue);

            var rewriteResult = await Manager!.ChangeSheetData([Config.InputSheetName], Config.BuildTestRow(TestRowId));
            Assert.Empty(CriticalErrors(rewriteResult));
            await Task.Delay(Config.SettleDelay);

            var afterWrite = await Manager!.GetLiveSheetRawValues(Config.InputSheetName);
            Assert.DoesNotContain(afterWrite[1], v => v?.Trim() == extraColumnValue);
        }
        finally
        {
            var deleteExtraColumn = new BatchUpdateSpreadsheetRequest
            {
                Requests = [DeleteColumnRequest(inputSheetId, extraColumnIndex)]
            };
            await Config.ExecuteRawBatchUpdateAsync(deleteExtraColumn, default);
            await Task.Delay(Config.StructureSettleDelay);
        }
    }

    public virtual async Task GetLiveSheetStructure_ReturnsConfiguredHeaders()
    {
        SkipIfNoCredentials();

        var structure = await Manager!.GetLiveSheetStructure(Config.InputSheetName);
        Assert.NotNull(structure);
        Assert.Contains(structure!.Headers, h => h.Name == Config.TestColumnName);
    }

    public virtual async Task GetLiveSheetRawValues_ReturnsPositionalRows()
    {
        SkipIfNoCredentials();

        var rawValues = await Manager!.GetLiveSheetRawValues(Config.InputSheetName);
        Assert.NotEmpty(rawValues);
    }

    public virtual async Task GetSheetProperties_And_GetAllSheetTabNames_ReturnCurrentMetadata()
    {
        SkipIfNoCredentials();

        var tabNames = await Manager!.GetAllSheetTabNames();
        Assert.Contains(Config.InputSheetName, tabNames);

        var properties = await Manager!.GetSheetProperties([Config.InputSheetName]);
        var property = Assert.Single(properties);
        Assert.False(string.IsNullOrEmpty(property.Id));
    }

    public virtual async Task GetSpreadsheetTitle_ReturnsConfiguredTitle()
    {
        SkipIfNoCredentials();

        var title = await Manager!.GetSpreadsheetTitle();
        Assert.False(string.IsNullOrWhiteSpace(title));
    }
}
