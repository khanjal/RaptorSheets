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

Performance investigation (2026-07-19) — read this before continuing V3 planning

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

Important gap found during implementation: gig-logger's Lambda (`GigRaptorService.csproj`) consumes `RaptorSheets.Gig` as a **pinned NuGet package** (currently `2.0.13`), not a local project reference. That means none of #1-#3/#5 (the RaptorSheets-side fixes - logging cleanup, the redundant `GetSheetInfo(ranges)` removal, the O(1) header lookup, the cached reflection lookups) reach production until:
1. RaptorSheets.Core/RaptorSheets.Gig are version-bumped and republished as NuGet packages.
2. gig-logger's `GigRaptorService.csproj` `PackageReference` is bumped to that new version.

The `ILogger` threading from `SheetsController` down to `GoogleSheetManager` was reverted in gig-logger for this reason (the constructor overload isn't in the published package yet) — a `NOTE` comment was left at the call site (`SheetManager.cs`) to wire it up once the package is bumped. Only gig-logger's own double-serialization fix (#4) is independent of the package version and is live as soon as this branch merges.

Notes
- Keep this plan alive and refine as engineering runs experiments. The biggest engineering work is the adapter and migration tooling; invest in easy-to-use adapter templates to reduce per-app friction.
