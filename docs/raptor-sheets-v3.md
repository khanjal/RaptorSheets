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

Notes
- Keep this plan alive and refine as engineering runs experiments. The biggest engineering work is the adapter and migration tooling; invest in easy-to-use adapter templates to reduce per-app friction.
