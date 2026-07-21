# Raptor Sheets — V3 Investigation & Core Consolidation

> **⚠️ DELETE BEFORE MERGE.** This is a working/scratch doc for the `refactor/generic-manager-base`
> branch — a running log of investigation and decisions, not permanent documentation. Remove this
> file before merging to `main`; fold anything still relevant (e.g. the `SheetEntity` breaking-change
> idea) into a real tracking issue or a permanent doc first if it shouldn't be lost.

**Status (2026-07-20): V3 decided against — not pursuing the generic `Sheet` model.**

V3 (proposed 2026-07-11, below) was originally a plan to replace each app's strongly-typed
`SheetEntity` with one generic runtime model shared across Gig/Stock/etc. It was revisited on
2026-07-19 as a possible fix for Gig Raptor's Lambda timeouts. The investigation found the real
causes were unrelated implementation defects (logging chatter, a redundant API call, an O(n)
lookup, double serialization, uncached reflection) — none of which required giving up static
typing. Those are fixed. The reuse/duplication goal V3 was chasing is instead being solved
incrementally by pulling domain-agnostic orchestration into `RaptorSheets.Core` — same strongly-typed
entities per app, shared plumbing underneath. No breaking change.

The original proposal is kept below, collapsed, for historical reference. It is not an active plan.

## What shipped

- **Perf fixes #1–5** (logging cleanup, redundant `GetSheetInfo(ranges)` call removed, O(1) header
  lookup, double-JSON-serialization removed in gig-logger, reflection caching) — see the investigation
  section below for detail. Shipped in `RaptorSheets.Gig` 2.0.14.
- **Shared manager base + sheet registry**
  ([RaptorSheets#50](https://github.com/khanjal/RaptorSheets/pull/50), merged to `main` 2026-07-19):
  added `GoogleSheetManagerBase` and `SheetRegistry<TEntity>` to `RaptorSheets.Core`, so
  `MapData`/`GetMissingSheets`/`CheckSheetHeaders`/`CheckUnknownSheets`/`GetSheetLayout(s)` and
  missing-column auto-insertion are implemented once and shared by Gig and Stock, instead of
  hand-duplicated per domain. Stock picked up working `CheckUnknownSheets`/`CheckSheetHeaders` it
  didn't have before. Shipped in `RaptorSheets.Gig` 2.0.15.
- **gig-logger**: bumped to `RaptorSheets.Gig` 2.0.15 (`GigRaptorService.csproj`); `ILogger` is now
  threaded from `SheetsController` through to `GoogleSheetManager` (the gap noted below is closed).

## Next: further Core consolidation

### Done — generic `GoogleSheetManagerBase<TEntity>` (2026-07-20)

The base manager is now **generic over `TEntity` and holds the domain's `SheetRegistry<TEntity>` +
canonical sheet-name list as fields** (`RaptorSheets.Core/Managers/GoogleSheetManagerBase.cs`).
Every domain-agnostic instance method moved up once and is now inherited rather than re-copied per
domain: `GetSheets`/`GetAllSheets` orchestration, `GetSheetProperties`/`GetAllSheetProperties`,
`GetAllSheetTabNames`, `GetSpreadsheetInfo`, `GetBatchData`, `GetSheetLayout(s)`,
`InsertMissingColumns`, and the missing-column auto-heal. `SheetPropertyHelper` moved from
`RaptorSheets.Gig/Helpers` to `RaptorSheets.Core/Helpers` (it's domain-agnostic property parsing).

Missing-sheet self-heal is the one per-domain hook: each manager implements the abstract
`CreateMissingSheetsAsync` (Gig's ordered/indexed `CreateSheets`; Stock's enum-based `CreateSheets`).
The only other things a domain manager now owns are its typed public API shape and its write
operations (`CreateSheets` ordering, `ChangeSheetData`, `DeleteSheets`). Static
`CheckUnknownSheets`/`CheckSheetHeaders` shims stay on each domain manager because tests call them
statically off the type. Stock picked up `GetSheetProperties`/tab-names/`GetSpreadsheetInfo`/
`GetBatchData` it didn't have before, purely by inheriting. A new domain manager is now: constructor
(hand the base a registry + canonical names) + `CreateMissingSheetsAsync` + its write ops — zero
re-copied metadata/layout/heal code. Verified end-to-end (Core/Gig/Stock unit + Gig/Stock
integration suites green).

### Done — `CreateSheets` ordering + `DeleteSheets` hoisted into the base (2026-07-20)

Gig's manager had shrunk to one 604-line file after the partial-class collapse, but two regions —
`CreateSheets` ordering (default-sheet relocation + index computation, ~182 lines) and `DeleteSheets`
(temp-sheet safety, ~106 lines) — were still ~85-90% domain-agnostic orchestration living only in
Gig. Both moved into `GoogleSheetManagerBase<TEntity>` behind one new hook:
`protected virtual BatchUpdateSpreadsheetRequest GenerateSheetsRequest(List<string> sheetNames)`,
which a domain overrides to supply its fully-configured AddSheet requests (headers, formatting,
validation, colors). `CreateSheets(List<string>, Dictionary<string,int>?)`, `CreateAllSheets`,
`DeleteSheets(List<string>)`, and `DeleteAllSheets` are now on the base; Gig's manager dropped to
**356 lines**, keeping only its 1-arg `CreateSheets` interface-arity shim, its
`CreateSheets(Dictionary<string,int>)` index-ordering convenience overload, the one-line
`GenerateSheetsRequest` override, `ChangeSheetData` (genuinely domain-specific), the static
`Check*` shims, and demo-data generation.

**Stock is untouched by this pass.** `GenerateSheetsRequest`'s default implementation throws
`NotSupportedException` — Stock doesn't override it, so it's inert (Stock never calls the base
`CreateSheets(List<string>,...)`/`DeleteSheets` overloads; it keeps its own enum-based `CreateSheets`
and still has no delete at all). Verified: Stock's 41 tests unchanged and still green.

### Done — Gig manager partials collapsed to one file, generic formulas moved to Core (2026-07-20)

`GoogleSheetManager.Crud/.Metadata/.Demo/.Helpers.cs` merged into one `GoogleSheetManager.cs`
(regions instead of partials) — matches Stock's single-file shape and is the template for
Job/Home. Separately, five formula-builder methods in `GigFormulaBuilder` were pure date/stat math
with zero gig logic (`WeekNumber`, `MonthNumber`, `WeekBeginDate`, `WeekEndDate`,
`RollingAverage`) — moved (with their constants) to `RaptorSheets.Core.Helpers.GoogleFormulaBuilder`
/ `GoogleFormulas`, next to the existing generic day/month/year/weekday extractors. Found and
removed two duplicate dead methods (`BuildArrayFormulaWeekBegin`/`WeekEnd`) and one fully-unused
file (`DateHelpers.cs`) along the way. **The Gig/Core formula split itself was already correct**
before this — `GoogleFormulaBuilder` (generic) vs `GigFormulaBuilder` (domain wiring: shift/trip
keys, pay+tips+bonus, address visit lookups) — most of Gig's formulas genuinely can't generalize
and stay put.

Net effect of this whole pass: Gig's manager went from ~950 lines across 5 partials to **356 lines**
in one file; `GoogleSheetManagerBase<TEntity>` now carries the entire read/metadata/heal/layout/
create/delete surface; Stock is unaffected throughout (still 41 tests, untouched).

### Still open / next possibilities

**Priority order (2026-07-21):** (1) Gig + gig-logger updated to the new wire format below,
(2) Stock brought to parity with Gig (surfaces the most overlap for a shared base), (3) Job
scaffold — deprioritized, will be a while, listed last on purpose.

1. **Gig + gig-logger on the new format (highest priority right now)** — the two breaking changes
   already shipped on the RaptorSheets side (`Sheets` nesting, Setup camelCase) still need the
   gig-logger Angular frontend ported: `trips`/`shifts`/etc. move under a nested `sheets` key
   (~49 non-spec `.ts` files, central `sheet.interface.ts`), and `SetupEntity`'s
   `RowId`/`Action`/`Saved` keys read as camelCase now.
2. **Stock parity with Gig (next)** — add `DeleteSheets`/ordered `CreateSheets` (blocked on
   Stock's `GenerateSheetHelpers.Generate` taking `List<Enums.SheetEnum>`, not `List<string>` — needs
   a `List<string>`-capable generator, or a minimal bare-`AddSheet` builder for non-domain sheets,
   before overriding `GenerateSheetsRequest`) and the still-missing `ChangeSheetData`. Doing this
   against Gig as the reference implementation is the point: it's what will show which parts of a
   base `SheetEntity`/manager genuinely generalize vs. which are Gig-specific.
3. **Scaffold `RaptorSheets.Job` as the proof-of-concept (deprioritized, last)** — the real test of
   whether this consolidation worked. Should be: entities + mappers + a `SheetRegistry<TEntity>` +
   a manager that's little more than a constructor + `CreateMissingSheetsAsync` +
   `GenerateSheetsRequest` + its own write ops. If that's not ~100-150 lines, something's still not
   generic enough.
3. **Done (2026-07-20):** `ChangeSheetData` dispatch scaffold extracted to Core. The accessor-map +
   separate request-building switch (which could drift out of sync) became one
   `GoogleRequestHelpers.SheetChangeAccessor<TEntity>` record (GetCount + GetData + BuildRequests) +
   `ResolveSheetsWithData`/`BuildChangeRequests`. Gig now declares one accessor dict as its single
   source of truth; the request-builders themselves stay per-domain (still need per-entity mappers),
   as expected. Job/Home get the same dispatch for free.
4. **Stock write ops**: still lacks `ChangeSheetData`/`DeleteSheets` entirely (in development,
   expected).
5. **Done (2026-07-20):** `GigRequestHelpers.cs` split the same way — `CreateDeleteRequests`,
   `ChangeSheetData<T>`, `CreateUpdateCellRequests<T>`, `GetEntityAction<T>`, `GetEntityRowId<T>`
   were already fully generic (reflection on `Action`/`RowId`, no Gig types) and moved to
   `RaptorSheets.Core.Helpers.GoogleRequestHelpers`. Gig keeps only its 8 thin per-entity wrappers.
6. **Done (2026-07-20):** `GigSheetConfigurationHelpers.cs` — `ConfigureSheet`, `CreateArrayFormula`,
   `CreateDatePartFormula` were dead code (never called by any mapper, only their own tests) that
   duplicated `ListExtensions.UpdateColumns()` / `GoogleFormulaBuilder.WrapWithArrayFormula`/
   `BuildArrayFormulaDay/Month/Year` — deleted along with their tests. Also fixed one live
   duplicate-logic bug found along the way: `DailyMapper`'s YEAR case hand-rolled the same
   ARRAYFORMULA Core's `BuildArrayFormulaYear` already builds — swapped to the builder call.
   `ApplyCommonFormats`/`ApplyFormatsByHeaderEnum` (genuinely Gig-specific) untouched.
7. **Done (2026-07-20):** Revisited `GoogleRequestHelpers.CreateUpdateCellRequests<T>`/
   `ChangeSheetData<T>`. The `GetEntityAction<T>`/`GetEntityRowId<T>` reflection-by-string-name
   (`typeof(T).GetProperty("Action"/"RowId")` on every call, in filter loops) was replaced by
   constraining `where T : SheetRowEntityBase` and reading `entity.Action`/`entity.RowId` directly —
   the two reflection helpers were deleted. `SheetRowEntityBase` (RowId/Action/Saved) moved from
   Gig to `RaptorSheets.Core.Entities` (it was fully generic). Side finding fixed: `SetupEntity`
   hand-rolled its own RowId/Action/Saved (missing the `[JsonPropertyName]` camelCase attributes the
   other 15 entities inherit) — now inherits the base. **BREAKING**: Setup's JSON keys change
   PascalCase→camelCase (see the breaking-changes section below — this is exactly the kind of thing
   worth doing before Job/Home). The append/update and save/delete split reads cleanly as-is; not
   changed.
8. **Done (2026-07-20):** Full scan of the remaining `RaptorSheets.Gig/Helpers` files plus
   `Mappers/` for dead code / Core-movable helpers. One finding:
   `CreateSheetsHelpers.OrderSheetTitlesByIndex` was fully generic — moved into Core's
   `SheetOrderingHelper` (`CreateSheetsHelpers.cs` deleted, was down to this one method).
   Everything else checked out as genuinely domain-specific and correctly placed:
   `MapperFormulaHelper` (keyed off Gig's `HeaderEnum` + `GigFormulaBuilder`),
   `GenerateSheetsHelpers` (dispatches to Gig's own mappers — this *is* Gig's
   `GenerateSheetsRequest` override, the same thing Job/Home will write their own version
   of), `DemoHelpers` (single method, fake-gig-entity generator), and all of `Mappers/`
   (one class per entity, no hidden generic utility or copy-pasted helpers found via
   cross-file scan). `Entities/` not scanned (plain `[Column]`-attributed POCOs — low
   expected value, no logic to find).

### Done (2026-07-20) — restructured `SheetEntity` to nest domain sheet collections ⚠️ BREAKING

**Shipped (RaptorSheets C# side).** Row collections moved off `SheetEntity` into a nested
`Sheets` container: Gig gained `RaptorSheets.Gig/Entities/GigSheets.cs` (18 collections),
Stock gained `StockSheets.cs` (3). `SheetEntity` is now `{ Properties, Sheets, Messages }` and
`ISheetEntity` is unchanged (`Sheets` is domain-typed, referenced directly by the registry
`assign` delegates, so Core needn't know about it). New wire shape:
`{"properties":{...}, "sheets":{"trips":[...], ...}, "messages":[...]}` — verified by serialization.

Blast radius handled compiler-driven (make the structural change → every stale `.Trips` becomes a
build error → fix): registry delegates in `GigSheetHelpers`/`StockSheetHelpers` (`se.X` →
`se.Sheets.X`), the manager accessor dict, `DemoHelpers`, and ~5 test files (Gig unit 558 / Stock 41
/ Core 898 all unchanged — purely structural; Gig integration re-run against the live sheet).

**⚠️ FRONTEND FOLLOW-UP REQUIRED (gig-logger, separate repo — NOT done):** the Angular frontend
reads `trips`/`shifts`/etc. as **top-level** JSON keys across ~49 non-spec `.ts` files (~85 with
specs), with a central `src/app/shared/interfaces/sheets/sheet.interface.ts`. They must move under a
`sheets` object. Until that lands, gig-logger is broken against the new RaptorSheets payload. This is
the accepted, pre-agreed cost of doing the break now (before Job/Home) rather than later. (Also still
pending from item 7 above: Setup's `RowId/Action/Saved` casing PascalCase→camelCase.)

<details><summary>Original proposal (superseded by the above)</summary>

Bigger and separate from the additive Core-consolidation work above, but worth
tracking here since it's a data-model question the original (rejected) V3 proposal also touched on.

**Current shape** (both `RaptorSheets.Gig/Entities/SheetEntity.cs` and
`RaptorSheets.Stock/Entities/SheetEntity.cs`): `Properties` and `Messages` (from `ISheetEntity`)
sit as flat siblings next to every domain sheet collection - Gig has 18
`List<XEntity>` properties (`Trips`, `Shifts`, `Expenses`, `Addresses`, ...) all flat on the same
object as `Properties`/`Messages`.

**The concern**: a domain could plausibly want (or already have, or a future domain like Home
might have) a sheet actually named "Properties" or "Messages," which would collide with the
`ISheetEntity` members at the same nesting level. Nesting every domain sheet collection under one
sub-object (e.g. `SheetEntity.Sheets.Trips`, `.Sheets.Shifts`, ...) instead of flattening them onto
`SheetEntity` itself would remove that class of collision entirely, at the cost of a breaking
change to the JSON wire shape (gig-logger and any frontend parse this JSON directly) and to
`SheetRegistry<TEntity>`'s processor delegates (currently `(se, rows) => se.Trips = rows`, would
become `(se, rows) => se.Sheets.Trips = rows` or similar).

**Not evaluated yet**: exact shape of the nested container, migration path for gig-logger's
existing consumers (Angular frontend + the Lambda's `SheetResponse`), whether `ISheetEntity` itself
needs to change, and whether this should land before or after `RaptorSheets.Job`/`Home` exist (doing
it before means only Gig/Stock need migrating; doing it after means a 3rd/4th domain too). Explicitly
open to a breaking change here if the resulting shape is cleaner - not constrained to
backwards-compatible steps the way the manager/formula consolidation work was.

</details>

**TODO — scan for other breaking-change candidates, not just this one.** `SheetEntity` nesting is
one example found by inspection, not the result of a systematic look. Do a deliberate pass over the
public shapes (entities, `ISheetEntity`, manager interfaces, wire/JSON contracts) for other things
that would be cleaner as a breaking change now. The point of doing this now, before `Job`/`Home`
exist: gig-logger's frontend + Lambda are the only consumers today, so breakage is fixable and
contained - the same changes get more expensive once two more domains are built on top of the
current shape. Breaking gig-logger is an accepted cost here, to be fixed after, not a reason to hold
back.

**Already shipped (2026-07-20) — first breaking change of this kind:** `SetupEntity`'s JSON keys
changed PascalCase→camelCase (`RowId`/`Action`/`Saved` → `rowId`/`action`/`saved`) when it was moved
onto `SheetRowEntityBase` (item 7 in the list above). Every other row entity already serialized
camelCase; Setup was the lone inconsistency. **gig-logger's frontend must be updated to read the new
Setup casing.** This is the pattern for the rest: when the scan finds more of these, fix them here
and patch gig-logger after.

### Done (2026-07-21) — `SheetEntityBase<TSheets>` added to Core

Gig's and Stock's `SheetEntity` had become byte-for-byte identical in structure after the `Sheets`
nesting above landed - `Properties`, a domain-typed `Sheets`, `Messages` - differing only in the
concrete `Sheets` type. Added `RaptorSheets.Core.Entities.SheetEntityBase<TSheets>` (abstract,
`where TSheets : new()`, implements `ISheetEntity`) holding that shared shape; Gig's `SheetEntity`
is now `: SheetEntityBase<GigSheets>` and Stock's is `: SheetEntityBase<StockSheets>`, both bodies
empty. `ISheetEntity` itself is untouched (still just `Properties`/`Messages`) - Core's generic
algorithms (`SheetRegistry<TEntity>`, `ColumnInsertionHelper`, `GoogleSheetManagerBase<TEntity>`)
still don't know about `Sheets`, so no second type parameter was needed there; `SheetEntityBase` is
purely a convenience base for domains, not a new constraint Core reasons about. Non-breaking (same
public shape/namespace/JSON per domain). Job/Home get this for free. Verified: Core 898 / Stock 41 /
Gig 558 unit tests unchanged and green.

### Done (2026-07-21) — Stock's row entities now inherit `SheetRowEntityBase`

Same class of finding as Setup (item 7 above), found while extending the base-class pattern: Stock's
`AccountEntity`/`StockEntity`/`TickerEntity` each hand-rolled their own bare `RowId` int instead of
inheriting `SheetRowEntityBase` (`RowId`/`Action`/`Saved`) - and had no `Action`/`Saved` at all.
This wasn't just duplication: it's a real blocker for **Stock write ops (item 4 below)**, since
`GoogleRequestHelpers.ChangeSheetData<T>`/`CreateUpdateCellRequests<T>` are constrained
`where T : SheetRowEntityBase` and read `entity.Action`/`entity.RowId` directly. Fixed by rooting
the hierarchy at the base: `CostEntity : SheetRowEntityBase` (the common ancestor of all three, and
not used anywhere else), then deleted the redundant manual `RowId` from `AccountEntity`/
`StockEntity`/`TickerEntity` - they now inherit `RowId`/`Action`/`Saved` transitively through
`CostEntity`/`PriceEntity`. Additive on the wire (`action`/`saved` are new JSON keys with defaults,
`rowId` unchanged), not a rename/removal. Verified: Core 898 / Stock 41 / Gig 558 unit tests
unchanged and green. Stock's mappers (`AccountMapper`/`StockMapper`/`TickerMapper`) still hand-roll
`MapFromRangeData` per entity (don't use `GenericSheetMapper<T>` the way Gig does) and were
untouched - `RowId = id` in each still compiles against the inherited property.

---

## Performance investigation (2026-07-19) — read this before the original plan below

Context: Gig Raptor requests are taking 20+ seconds against a hard 30s API Gateway timeout, and got worse after adding the Deliveries and Locations sheets (17 sheets total now). This was investigated to see whether V3 (relaxing strong typing) would help. Finding: it would not — the actual causes are implementation defects unrelated to the entity model, and none of them require abandoning strong typing. V3's stated goals (flexibility/reuse across apps) are orthogonal to this problem. Fix these first; decide on V3 separately, on its own merits.

Root causes found, in priority order (each independently verified against the code). Decisions below were made 2026-07-19 after walking through each item; implementation happens one at a time.

1. `Console.WriteLine` cleanup — split into "delete" vs "convert to real logging," plus a logging library decision
   - Delete outright, no replacement: `RaptorSheets.Core/Helpers/HeaderHelpers.cs` (`GetIntValueOrNull` ~L103-138, `GetDecimalValueOrNull` ~L154-180, up to 5-6 calls per numeric cell) and `RaptorSheets.Core/Mappers/GenericSheetMapper.cs` (~L270, ~L304, 2 per property per row). This is per-cell debug chatter with no production value — anything actually worth surfacing already flows through the existing `MessageEntity` mechanism. Across 17 sheets x many rows x many columns this is tens of thousands of CloudWatch log-writes per request (stdout is shipped synchronously per line in Lambda — real I/O cost) and scales directly with row count, which is why this got worse after adding Deliveries/Locations.
   - Convert to real logging (genuine error paths, not chatter): `RaptorSheets.Core/Services/GoogleSheetService.cs` catch blocks (L48, 63, 77, 103, 121, 140, 155) and `RaptorSheets.Core/Factories/SheetModelFactory.cs` (~L39, which literally comments "in a real implementation you'd use proper logging").
   - Library decision: `Microsoft.Extensions.Logging.Abstractions` (not a concrete provider like Serilog/NLog — neither is currently referenced anywhere in the solution). Standard pattern for a reusable library: depend only on `ILogger<T>`, let the consuming app own the real provider. gig-logger's Lambda already calls `AddLogging(b => { b.AddConsole(); b.AddDebug(); })` in `Startup.cs:143-147`, so once RaptorSheets accepts an injected logger it flows into the existing pipeline for free.
   - Mechanics: `GoogleSheetService` already has real (non-static) constructors — add an optional `ILogger<GoogleSheetService>? logger = null` (default `NullLogger.Instance`), non-breaking. `GoogleSheetManager`'s DI constructor can accept `ILogger<GoogleSheetManager>` the same way. Gap to close: gig-logger's `SheetsController` already has `ILogger<SheetsController>` injected (`SheetsController.cs:18,22`) but doesn't pass it to `SheetManager`/`GoogleSheetManager` (`SheetManager.cs:50` just does `new GoogleSheetManager(token, sheetId)`) — needs threading through. `HeaderHelpers`/`GenericSheetMapper<T>` are static with zero instance state and no injection point, which is fine since their Console.WriteLines are in the "just delete" bucket anyway.

2. A second, full Google API round trip on every successful request — confirmed redundant, not just slow
   - Two different `GetSheetInfo` calls exist and only one is expensive. The self-heal path (batchGet fails -> check for missing sheets, `GoogleSheetManager.Crud.cs` ~L240) already calls `GetSheetInfo()` with **no ranges** — confirmed in `SheetServiceWrapper.cs:169-178`, `IncludeGridData` only gets set `true` when `ranges` is non-empty, so this is already a cheap metadata/tab-names-only call. Nothing to change there.
   - The expensive one is on the **success** path: `GoogleSheetManager.Crud.cs` ~L297-299 calls `GetSheetInfo(ranges)` **with** ranges (`IncludeGridData=true` across all 17 sheet ranges), purely to run `GoogleSheetManager.CheckSheetHeaders(spreadsheetInfo)` (`GoogleSheetManager.Metadata.cs:163`) for header-mismatch messages.
   - This is redundant: `GigSheetHelpers.MapData` (`GigSheetHelpers.cs:65-166`), which runs right after using data already in hand from the one `batchGet` call, already does the identical per-sheet header-name check (`var headers = values[0]; ... HeaderHelpers.CheckSheetHeaders(headers, XMapper.GetSheet())`) for every sheet. Known-sheet header validation is being done twice — once free (from data already fetched), once via a dedicated second API call.
   - The only thing the second call adds beyond what `MapData` covers is detecting unknown/extra tabs in the spreadsheet (`GetUnknownSheetWarnings`, `GoogleSheetManager.Metadata.cs:45-51`) — which needs tab names only, not grid data.
   - Fix: drop `GetSheetInfo(ranges)` + the redundant known-sheet re-check on the success path entirely. Replace with the cheap `GetSheetInfo()` (no ranges) used only for unknown-tab detection (same call `GetAllSheetTabNames()` already uses). No loss of header-reorder detection — that's already fully covered by `MapData` — just removes a duplicated expensive round trip.
   - Separate, not-yet-decided item: after self-heal creates missing sheets, the code returns an "info: please retry" message rather than re-fetching inline (`GoogleSheetManager.Crud.cs` ~L265-276). Since Deliveries/Locations were just added, existing users are likely hitting this now (create tabs -> client must re-request). Worth checking whether the Angular side already retries on this message vs. adding a bounded inline retry (wait ~1-2s, retry batchGet once or twice within the existing 30s budget) — a UX/orchestration call, not a hot-path fix.

3. O(n) header lookup with string allocations, called per cell — already correct, just slow
   - `RaptorSheets.Core/Helpers/HeaderHelpers.cs` (`GetHeaderKey`, ~L184-195) already matches headers by **text**, not position, so reordered/swapped columns are already tolerated correctly today — no behavior change needed for that requirement. The only defect is `header.First(x => x.Value.Trim() == value.Trim())`: a linear scan plus `.Trim()` allocation on every single `GetStringValue`/`GetIntValueOrNull`/`GetDecimalValueOrNull`/etc. call.
   - Fix: right after parsing the header row (same place `ParserHeader` already runs, once per sheet), build a `Dictionary<string, int>` (trimmed header name -> column index) once, and have every `GetXValue` do an O(1) `TryGetValue` against it instead of a linear scan per cell. Same reorder-tolerant semantics, O(1) instead of O(n) — likely the single biggest CPU win given total cell count across 17 sheets.

4. Double JSON serialization in gig-logger's Lambda
   - `amplify/backend/function/GigRaptorService/src/GigRaptorService/Business/SheetManager.cs` (`ProcessResponseSize`, ~L89-107): serializes the full `SheetEntity` once just to measure byte size for the S3-offload decision (needed — that's how inline-vs-S3 is decided given the Lambda/API Gateway payload limit), then ASP.NET Core serializes the same object graph again on the way out.
   - Fix: the first serialize is unavoidable, but the second isn't. Reuse the already-serialized `jsonContent` string as the literal response body when under the S3 threshold — build the envelope (`{"sheetEntity": <existing json>, "s3Link": null, "isStoredInS3": false, "metadata": null}`, matching `SheetResponse`'s shape in `Models/SheetResponse.cs`) by embedding that string directly, and return it via `ContentResult`/`Content(json, "application/json")` instead of handing the `SheetResponse` object back for the framework to serialize a second time. Same wire shape and `_jsonOptions` formatting (same serializer call produces the fragment), one fewer full-graph pass. Only the inline path needs this — the S3-link branch's payload is tiny already. Touches the controller's return contract for the sheets endpoints, so slightly more invasive than #1-#3 but self-contained to `SheetManager.cs`/`SheetsController.cs`.

5. Minor: reflection property lookup by string name, per row, not cached
   - `RaptorSheets.Core/Mappers/GenericSheetMapper.cs` (`MapFromRangeData`, ~L75, ~L94): `typeof(T).GetProperty("RowId")` and `typeof(T).GetProperty("Saved")` are looked up every row instead of once per type (the column properties are already cached this way — these two were missed).

Separate lever, not required for the fixes above: decoupling wire format from internal typing
- The internal C# entities can stay strongly-typed (real validation/correctness value) while a slimmer DTO is defined for what actually crosses the wire to the Angular frontend — dropping per-cell noise, redundant messages, or summary sheets the current view doesn't need.
- This is a legitimate answer to "maybe we don't need strongly typed data for the frontend" that does NOT require the V3 generic-Sheet model — it's an orthogonal axis (internal representation vs. wire representation).
- Caution if V3 is pursued later for its stated reuse goals: a fully generic `Cell`/`Row` model with per-cell `meta` dictionaries can easily be slower than today's approach if it repeats the same traps (reflection/logging/allocation per cell) — design it to avoid #1 and #3 above from day one, don't treat V3 as a performance initiative.

Caching angle for the request-count vs. latency tension
- `batchGet` already consolidates all 17 sheets into one Google API call — splitting data into more granular fetches for the frontend does not have to mean more Google API calls, if a short-TTL cache (in-memory or DynamoDB) of the parsed `SheetEntity` sits between Google Sheets and the Lambda, invalidated on write. This decouples "how much data the frontend gets per view" from "how many Google API calls are made," which is the actual constraint in play.

Suggested order to work through these (smallest/highest-confidence first): #1 -> #2 -> #3 -> #5 -> #4 -> caching layer design -> wire-format/DTO split -> revisit V3 adoption decision on its own merits.

Status (2026-07-19): #1-#5 implemented and tested in this repo (RaptorSheets.Core/RaptorSheets.Gig), full solution suite green (833 Core, 32 Stock, 621 Gig, all passing, no regressions). New/updated tests added for reordered-column header lookup, `CheckUnknownSheets`, and `GetSheets`' no-longer-calling-the-expensive-overload behavior. #4 (gig-logger's double JSON serialization) is also implemented and tested on the gig-logger side (`SheetResponse.SheetEntity` now `object`-typed so `ProcessResponseSize` can hand over an already-parsed `JsonNode` instead of re-serializing; wire-shape equivalence verified by test).

Important gap found during implementation (closed 2026-07-19, see "What shipped" above): gig-logger's Lambda (`GigRaptorService.csproj`) consumed `RaptorSheets.Gig` as a **pinned NuGet package**, not a local project reference, so none of #1-#3/#5 reached production until the package was version-bumped and gig-logger's reference bumped to match. The `ILogger` threading from `SheetsController` down to `GoogleSheetManager` was reverted at the time for the same reason, with a `NOTE` left at the call site — both are done now.

---

<details>
<summary><strong>Original V3 proposal (2026-07-11) — superseded, kept for history</strong></summary>

Raptor Sheets — Version 3 Plan (breaking)

TL;DR
- Replace per-application, strongly-typed SheetEntity model with a single generic Sheet v3 model.
- New model: a top-level Sheet object containing metadata, an array of SheetModel (one per worksheet), and shared messages / metadata.
- SheetModel contains headers (attributes), rows; each row is a list of cells/columns with optional typed metadata.
- This is a breaking change for Gig Raptor and other applications that currently rely on app-specific sheet entity types. Plan includes phased migration, adapters, runtime feature flagging, tests, and a migration utility.

Goals
- Increase flexibility and reuse across applications (gigt, stock, etc.).
- Preserve ability to show and edit sheet data while relaxing compile-time coupling to exact domain shape.
- Provide a clear migration path from existing strongly-typed sheet entities to a generic runtime-friendly representation.
- Keep mapping and developer ergonomics reasonable (helpers, typing layers, and adapters).

Constraints & Non-goals
- Non-goal: Keep full property-level static typing for domain shapes inside the generic sheets — v3 intentionally trades some static typing for flexibility.
- Must preserve existing UX where possible and provide migration adapters.
- Backend storage (Lambda, DB) schema changes allowed; this is a major version bump.

High-level change summary
- Old: For each application, create a SheetEntity that mirrored the spreadsheet with app-specific typed nested entities for each sheet.
  - Pros: strong typing, direct mapping, easy to use in UI.
  - Cons: duplicated code, poor reuse of common functions, brittle to spreadsheet changes.
- New (v3): One generic Sheet object used by all apps.
  - Contains: metadata, messages, sheets: SheetModel[]
  - SheetModel structure: name, id, headers: ColumnHeader[], rows: Row[] and other options (frozenRows, protectedRanges...)
  - Rows are arrays of cells (Cell objects) plus optional row-level metadata (id, syncedAt, annotations).

Proposed Data Model (TypeScript examples)

// Top-level sheet container
export interface ISheetV3 {
  version: 3; // integer version
  id: string; // overall sheet collection id (spreadsheet id)
  title?: string; // spreadsheet title
  messages?: IMessage[]; // common messages, errors, sync status
  sheets: ISheetModel[]; // each worksheet
  metadata?: Record<string, any>; // extensible
}

export interface ISheetModel {
  sheetId?: string | number; // provider sheet id
  name: string; // worksheet name
  headers: IColumnHeader[]; // header definitions
  rows: IRow[]; // data rows
  properties?: ISheetProperties; // options like frozen, filters, ranges
}

export interface IColumnHeader {
  id?: string; // optional stable id for the column
  name: string; // displayed header text
  key?: string; // optional logical key used by mappers
  type?: 'string'|'number'|'date'|'boolean'|'json'|'any';
  meta?: Record<string, any>; // e.g., format, validations
}

export interface IRow {
  id?: string; // stable id (generated) for mapping
  values: ICell[]; // ordered cells matching headers
  meta?: Record<string, any>; // row-level annotations (source, status, notes)
}

export interface ICell {
  value: any; // raw value
  formatted?: string; // display value
  formula?: string; // formula text if applicable
  meta?: Record<string, any>; // e.g., originalColumnKey, confidence, error
}

export interface IMessage {
  level: 'info'|'warning'|'error';
  code?: string;
  message: string;
  details?: any;
}

Design decisions and rationale
- Use arrays for headers and rows so the object is compact and maps naturally to sheet providers (Google Sheets rows/columns are ordered).
- Provide an optional stable id on headers and rows to allow mapping to domain-specific keys while preserving order.
- Store cell-level metadata to allow downstream mapping to typed DTOs and to carry sync/formatting hints.
- Keep top-level messages and metadata for cross-sheet concerns (sync status, errors).

Mapping strategy from current typed entities to v3
- Adapter layer: implement a bi-directional adapter API per application.
  - toV3(appTypedSheetEntity) -> ISheetV3
  - fromV3(ISheetV3, targetDomain) -> typed entities (best-effort)
- Mapping rules:
  - If original entities had explicit property names (e.g., Trip.date, Trip.earnings), add header entries with key = property name and type when known.
  - If original shape included nested objects per sheet, create separate SheetModel entries (e.g., Trips sheet, Expenses sheet), and add a header for the original typed fields.
  - Preserve the original typed entity as a serialization of entire row in cell.meta.originalType to allow full-fidelity roundtrip if needed.
- Provide a standard algorithm:
  1. Produce header list: for each typed property, create a header with key and type.
  2. For each domain object -> produce row where values array matches header order. For missing values, place null.
  3. Save row.id as domain id if exists, or generate stable UUID.
  4. Save mapping metadata: which header.key maps to which property path.

Backward compatibility & progressive rollout
- Introduce ISheetV3 while keeping existing v2 sheet entity code intact behind a feature-flag.
- Implement an adapter service (SheetAdapter) used by API and front-end to load `v2` or `v3` depending on feature flag and convert to the internal view model expected by consuming components.
- Deployment phases:
  1. Library + types: add v3 interfaces and utilities, no behavior change.
  2. Adapters + migration CLI/tool (dry-run mode) — can generate v3 artifacts from stored v2 data.
  3. Consume v3 internally behind feature flag; add tests and telemetry.
  4. Flip feature flag; monitor.
  5. Remove v2 code after stable.

Storage & versioning
- Persist a version property on saved sheet documents: sheet.version = 2 | 3.
- Update DB schema (if using Dynamo/Sheets-backed storage) to accept the new structure. Keep v2 documents until migration.
- Provide a migration job (server-side) that reads v2 rows and writes v3 format; support streaming and batched conversions.

Migration plan (phased tasks)

Phase 0 — Discovery & alignment (1 week)
- Inventory all apps (gig-raptor, stock, others) that rely on current sheet entities and list exact consumers (UI components, services, lambdas).
- Identify fields / typed sheet entities that are fragile or duplicated.
- Decide minimum header metadata required for mapping (id, key, type, name).

Phase 1 — Types & adapters (2 weeks)
- Create v3 interfaces (ISheetV3, ISheetModel, IColumnHeader, IRow, ICell, IMessage).
- Implement SheetAdapter base class with helper functions to build headers/rows and to map back to domain objects.
- Add adapter for Gig Raptor as first consumer (top priority) and at least one other app as validation.
- Add unit tests for adapter mapping logic.

Phase 2 — Persistence & migration tooling (2 weeks)
- Add version flag to persisted documents.
- Create migration CLI tool: "migrate-sheets --dry-run" that scans DB/storage and produces v3 samples and metrics.
- Implement server-side migration job for bulk conversion (batched, idempotent).
- Add monitoring for conversion progress and error reporting.

Phase 3 — Runtime integration & UI (2 weeks)
- Integrate adapter into services used by UI; expose a compatibility layer that provides either typed or generic view models.
- Update key UI components to read rows as arrays or use helpers to present table data.
- Add utilities to convert a row array to an object by header.key for components that need keyed access.
- Add migration mode to admin/ops UI to preview conversions.

Phase 4 — Feature flag flip & cleanup (1-2 weeks)
- Flip feature flag for internal usage of v3 in one app (Gig Raptor) and monitor.
- Triage issues and complete conversion for safe parity.
- Gradually migrate remaining apps and remove v2 code after validation (cleanup branch, then PRs).

Testing & validation
- Unit tests for adapters (roundtrip fidelity), header/row conversions, and edge cases (missing columns, uneven rows, formulas).
- Integration tests: UI rendering for generic table view, mapping helpers.
- Migration dry-run verification outputs: sample counts, percentage loss of fidelity, unmapped fields.
- Runtime smoke tests: import/export roundtrip to ensure no data loss on small spreadsheets.

Rollout & monitoring
- Telemetry events: migration start/complete, adapter failures, mapping warnings (e.g., missing header.key), API errors.
- Feature flag controls per-app to allow staged rollout.
- Health dashboards for migration job and conversion error queue.

Risks & mitigations
- Risk: Loss of compile-time typing leads to runtime bugs in consumers.
  - Mitigation: Provide typed mappers that produce domain DTOs and maintain unit tests to ensure correct shape.
- Risk: Mapping complexity for highly nested domain types.
  - Mitigation: Provide explicit adapter templates and a "serializeOriginalObject" fallback in cell.meta to permit full-fidelity roundtrip.
- Risk: UI breakage due to changed data shape.
  - Mitigation: Provide compatibility wrappers that expose old shape until components are ported.

Developer ergonomics & helper utilities
- Build helpers:
  - rowToObject(headers, row): returns object keyed by header.key
  - objectToRow(headers, obj): emits values in header order
  - generateHeadersFromSchema(schema): from domain schema or example objects
  - findHeaderIndex(headers, key)
- Provide TypeScript "view types": small helpers that map ISheetModel to a Partial<T>[] when a T is known — this preserves typed usage surfaces while staying compatible.

Open questions (to decide before implementation)
- What minimal header metadata is required for all apps? (must we include type?)
- Do we want to support nested columns (e.g., address.street) as single header.key or multiple split headers?
- How to prefer column identification on sheet name vs sheetId when provider sheets are renamed frequently?
- Migration prioritization: which app should migrate first after Gig Raptor?

Deliverables
- plan.md (this document)
- v3 TypeScript interfaces in shared package
- SheetAdapter base class + Gig Raptor adapter
- Migration CLI (dry-run and run modes)
- Unit + integration tests
- Feature flag gating and admin preview UI

Next immediate steps (actionable)
1. Approve conceptual model and open a tracking ticket for "Raptor Sheets v3" (assign an owner).
2. Run inventory of all consumers and produce a short mapping spec per app (triples: app -> entity types -> consumer list).
3. Add v3 interfaces to the repository under src/lib/raptor-sheets/v3/ and implement SheetAdapter skeleton.

Appendix: Example mapping snippet (pseudo)

// mapping row -> typed object
function rowToTyped(headers: IColumnHeader[], row: IRow): Record<string, any> {
  const out: Record<string, any> = {};
  for (let i = 0; i < headers.length; i++) {
    const key = headers[i].key ?? headers[i].name;
    out[key] = row.values[i]?.value ?? null;
  }
  return out;
}

// mapping typed object -> row
function typedToRow(headers: IColumnHeader[], obj: Record<string, any>): IRow {
  const values = headers.map(h => ({ value: get(obj, h.key ?? h.name) }));
  return { id: obj.id ?? generateUuid(), values };
}

</details>
