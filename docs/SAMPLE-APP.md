# Sample App

`RaptorSheets.Sample.Web` is a runnable Blazor Server app that gives you a browser UI over a real
spreadsheet, using whichever domain manager you point it at. It's the fastest way to see whether
the library does what you need: pick a sheet, browse the rows, add/edit/delete a few, and watch the
result land in the actual Google Sheet.

It's plain ASP.NET Core - `dotnet run` is the whole setup story, no Node/npm toolchain required.

Wired up for **Gig Work, Job Applications, Home Maintenance, and Stock Tracking** - pick any of them
from the nav and browse/edit its sheets the same way. Domain labels are deliberately more than the
bare domain name ("Home" alone reads as this app's own Home page, not a domain) - see
`ISheetOperations.DomainLabel`. Stock's entities only recently gained `[Column]` attributes (it
predated the `[Column]`/`GenericSheetMapper<T>` convention the other three domains are built on,
and used its own hand-rolled header/mapping code until then) -
`RaptorSheets.Sample.Web/Services/StockSheetOperations.cs` was already written and ready well
before that port landed.

There's also a domain-agnostic **[Sheet Inspector](#sheet-inspector)** at `/sheet-inspector`, reachable
from the nav regardless of which domains are wired up - point it at any live tab on any connection
(including one of RaptorSheets' own known domains, or a schema-less "Generic" connection) and see its
structure/raw data straight from a live read, with a best-effort C# class stub generated from what it
finds.

Styling is a small set of CSS custom properties defined once in `wwwroot/app.css` (`--color-*`,
`--radius-*`, `--shadow-*`) with a `prefers-color-scheme: dark` override alongside the light
defaults - every component stylesheet references the tokens rather than hardcoding colors, so
light/dark and any future palette tweak is a one-file change. `.btn-primary` marks the primary
action in each toolbar/form (Save changes, Add, Create demo data, Save); everything else is the
plain default button style.

## Running it

```bash
dotnet run --project RaptorSheets.Sample.Web
```

If no spreadsheet is configured yet, the Home page points you at **Settings** (also reachable from
the nav at any time, not just when disconnected) - a walkthrough for creating a Google Cloud service
account and sharing a spreadsheet with it, then individual fields for each credential (type, client
email, client ID, private key ID, private key) plus a spreadsheet ID per domain (Gig/Stock/Job/Home).
Only the private key is actually secret - the others are identifiers, safe to see in plain text - so
only the private key gets a Show/Hide toggle, and hiding it swaps the `<textarea>` out for a
character-count placeholder entirely rather than trying to mask live text in place: the value is
genuinely absent from the rendered DOM while hidden, not just visually obscured. There's also an
optional "paste the whole JSON key" box above the fields that parses on input and autofills them,
since copying five values out of the downloaded key file by hand is tedious - the individual fields
are what actually gets saved, the paste box is just a shortcut into them. The 5 credential fields are
independently optional on save - a blank one just means "leave the existing value alone" (the private
key is never sent back down to the browser once saved, so leaving it blank must not wipe it).
Submitting writes straight to the local `secrets.json` and reconnects immediately, no restart needed.
Credentials are replace-only: Settings shows which service account is currently active (`client_email`
- safe to display) but never re-displays the private key itself, the same "write, don't show" pattern
every password/API-key settings screen uses.

Each spreadsheet ID field accepts either a bare ID or the full Google Sheets URL - paste the URL
straight from the browser's address bar and it's trimmed down to just the ID as you type, no manual
extraction needed. A **Clear** button next to each field blanks it in one click; unlike the credential
fields, a blank spreadsheet ID field is not "leave unchanged" - it's always visibly prefilled with the
current value, so a blank one is a deliberate signal, and saving removes that domain's ID entirely
(disconnecting it) rather than leaving the old value in place. Once connected, a field shows
**"Connected to '{title}'"** using the spreadsheet's own title from Google Sheets (via
`GetSpreadsheetTitle()`), not anything typed into this app - confirms you actually pasted the ID you
meant to, especially useful right after pasting a URL. This check runs in the background per field
(so the page doesn't wait on 4 API calls before it's usable) and re-runs after every save.

Every domain - Gig/Job/Home/Stock - gets **"Create missing sheets"** and **"Insert demo data"** buttons
next to its ID field - deliberately two buttons, not one. Every domain's `GenerateDemoData()` always
assigns `RowId` starting fresh at 2, and the
underlying write path decides overwrite-vs-append purely by comparing `RowId` against the sheet's
total *grid* row count (usually 1000+), not its populated-row count - so `RowId 2` almost always lands
in the "overwrite this literal spreadsheet row" branch. **Calling insert-demo-data against a
spreadsheet that already has real rows would silently overwrite the first several of them, not just
add demo rows alongside them.** (Tracked as a library-level fix, since this affects any caller of
`GenerateDemoData`/`ChangeSheetData`, not just this UI - see the codebase's task backlog.)

Settings guards against this: alongside the "Connected to" title check, it fetches each domain's
writable/primary sheets (skipping read-only reference/rollup sheets, which are formula-derived and
show rows the moment primary data exists either way) and counts their rows. The summary shows
**"X of Y sheets found"** plus any header-check warnings from the read, and **"Insert demo data" stays
disabled the moment any existing row is found** - re-checked again immediately before the write
itself, not just at page load, since something could have changed in between. **"Create missing
sheets"** has no such risk (`CreateAllSheets()`/`CreateSheets()` never touches a sheet that already
exists as a tab) and stays available whenever fewer sheets exist than expected. This is a "just give
me something to look at" action with no options, distinct from the Home page's blank-
spreadsheet wizard (which is Gig-specific and reuses the same underlying call).

`RaptorSheets.Sample.Web` and `RaptorSheets.Test` (the integration test suite's shared infra)
deliberately declare the **same `<UserSecretsId>`**, so they read one `secrets.json`
(`%APPDATA%\Microsoft\UserSecrets\d3dcd413-.../secrets.json` on Windows,
`~/.microsoft/usersecrets/d3dcd413-.../secrets.json` on Linux/macOS) instead of each needing its own
copy kept in sync by hand. The service account credentials really are shared - one Google Cloud
service account works for both. User secrets holds **only** those credentials plus
`spreadsheets:test:{domain}` - the dedicated, disposable spreadsheets `RaptorSheets.Test`'s
integration suite points at, which it deletes and regenerates on every run (see the test suite's own
`CleanSlateSheetFixture`). Set those via the CLI or CI secrets, not the Settings UI.

**Your own spreadsheets are a separate, local-only "Connections" list**, not user secrets: a
`connections.json` file in this app's own local-data folder
(`%APPDATA%\RaptorSheets.Sample.Web\connections.json` on Windows,
`~/.config/RaptorSheets.Sample.Web/connections.json` on Linux/macOS) - deliberately not inside the
`UserSecrets` folder alongside `secrets.json`, since none of it is actually secret. Needs no
`.gitignore` entry either way, since it was never inside the repo. Fully managed from the Settings
page's "Connections" section. Each
connection has a type (`gig`/`stock`/`job`/`home`, or `generic` for a spreadsheet with no compiled
`[Column]` schema - usable only in the Sheet Inspector), a label, and a spreadsheet ID. Unlike the
old single `spreadsheets:live:{domain}` key, **you can add more than one connection of the same
type** - useful for comparing spreadsheets or keeping a backup alongside your main one; NavMenu nests
by connection label whenever a domain has more than one.

This split exists specifically so recording real data through this app can never land on the
spreadsheet the tests wipe. As a convenience, whenever a domain type has zero real connections,
`Sheet.razor`/`Home.razor` fall back to showing `spreadsheets:test:{domain}` instead (synthesized as a
connection on the fly, with a banner making clear that's what's happening) - so there's something to
look at before you've added your own connection, rather than just an empty/error state. Nothing is
read from `appsettings.json`, so there's nothing to accidentally commit either way.

Prefer the CLI for credentials/test IDs? Same store, either project's directory works:

```bash
cd RaptorSheets.Sample.Web

dotnet user-secrets set "google_credentials:type" "service_account"
dotnet user-secrets set "google_credentials:private_key_id" "your-key-id"
dotnet user-secrets set "google_credentials:private_key" "your-private-key"
dotnet user-secrets set "google_credentials:client_email" "service@project.iam.gserviceaccount.com"
dotnet user-secrets set "google_credentials:client_id" "your-client-id"
dotnet user-secrets set "spreadsheets:test:gig" "your-test-gig-spreadsheet-id"
```

There's no CLI equivalent for adding a real connection - `connections.json` isn't a
`dotnet user-secrets`-managed file, so use the Settings page's "Connections" section for those.

**If the Gig spreadsheet you connect is blank** (no Gig sheets on it yet - checked via
`GetAllSheetTabNames()` against the known Gig sheet names, not just "zero tabs", since a fresh
Google Sheet always starts with one default "Sheet1" tab), the Home page offers a one-click
"Create sheets + fill with demo data" button: `CreateAllSheets()` then `GenerateDemoData()` then
`ChangeSheetData(["Shifts", "Trips", "Expenses"], demoData)`, the exact sequence documented in
[RaptorSheets.Gig's README](../RaptorSheets.Gig/README.md#demo-setup). The other 14 sheets fill in
on their own from the sheet formulas once Shifts/Trips/Expenses have real rows. This particular
blank-spreadsheet wizard is Gig-only, since it's tied to the Home page's Gig connection status - the
equivalent for Job/Home/Stock is the "Create sheets + fill with demo data" button on each domain's
spreadsheet ID field in **Settings**, described above.

## Multiple domains, one generic layer

Gig/Job/Home each get their own `ISheetOperations` implementation
(`GigSheetOperations`/`JobSheetOperations`/`HomeSheetOperations` in `Services/`) - a small,
fully-typed adapter over that domain's own strongly-typed `ISheetManager`/`SheetEntity`, since
there's no single non-generic manager type shared across domains to inject instead (each domain
declares its own `ISheetManager : ISheetManager<TEntity>`, and `TEntity` differs). A
`DomainRegistry` collects whichever ones are registered in `Program.cs` and looks them up by route
segment ("gig", "job", "home"), so `NavMenu.razor` and `Sheet.razor` (route `/sheet/{Domain}/{SheetName}`)
never need to know which domain they're actually driving - the small amount of per-domain
duplication across the four adapter classes is deliberate, not an oversight: each is maybe 50 lines,
almost entirely boilerplate, and boring/explicit beats a reflection-heavy generic dispatcher for
something this size (there are 4 domains, not 40).

The nav is a one-level accordion, not a flat list of every sheet in every domain at once (Gig alone
has 17) - each domain label is a toggle; clicking it expands/collapses just that domain's sheet
list, and only one domain is expanded at a time. Landing directly on a sheet page (a fresh load, a
refresh, browser back/forward) auto-expands whichever domain that sheet belongs to, via
`NavigationManager.LocationChanged`, so you're never looking at a collapsed group with no visible
indication of where you are. `DomainRegistry` sorts domains alphabetically by `DomainLabel`, not by
whatever order `Program.cs` happens to register them in, so the nav (and Settings' spreadsheet ID
list) stay predictable regardless of registration order.

Every domain entity declares its own schema via `[Column(...)]` attributes - header name, whether
it's user-editable or a read-only formula/output column, validation rules, display format. Rather
than hand-building a page per entity (Gig alone has 17 sheet types), the sample reflects over that
metadata once, using the same reflection Core's own mapper relies on
(`RaptorSheets.Core.Utilities.TypedFieldUtils.GetColumnProperties`), and renders one generic
`EntityGrid<TRow>` component for whichever sheet you pick. Adding a new column to a domain entity
means it just shows up - no sample-app code changes.

A Sheets-container property's own name isn't always its real spreadsheet tab name - C# identifiers
can't hold spaces or "&", so Job's `InterviewTypes` property is really the "Interview Types" tab, and
Home's `DoorsWindows` property is really "Doors & Windows". `SheetMetadata.GetSheetDescriptors`
resolves this via each domain's own `Constants.SheetsConfig.SheetNames` class, whose field names
match the Sheets-container property names 1:1 by convention - its value is the real tab name (Gig's
own sheet names are all single words identical to their constants either way, so this was invisible
until Job/Home were wired up). Job's Sheets container also has three DTO placeholder properties
(`Weekly`/`Monthly`/`Summary`) with no backing sheet or formula yet - `ISheetOperations
.ExcludedSheetNames` filters these out of the nav and sheet discovery entirely.

New rows go through an "Add item" form rather than an inline blank row - fill in the fields, confirm,
and it's staged alongside any inline edits. Nothing is sent to Google until you click "Save changes",
which batches every pending add/edit/delete for the sheet into one `ChangeSheetData` call - never a
write per keystroke.

Editable inputs are sized by the underlying CLR type rather than left to the browser's defaults -
numeric columns (`decimal?`/`int?`/`double?`, e.g. Pay, Tips, Distance) render compact, text and
typeahead columns render wide. This matters because a bare `<input type="number">` renders far wider
than `<input type="text">` by default in most browsers regardless of what it holds, which is backwards
for this data (short numbers, longer text). One subtlety: these inputs are built via
`RenderTreeBuilder` in `EntityGrid.razor`'s `@code` block, not literal markup in the `.razor` file's
template - Blazor's CSS isolation only scopes elements the compiler sees directly in that markup
section, so a rule in `EntityGrid.razor.css` can never reach them. Their sizing (`.field-compact` /
`.field-wide`) lives in the global `wwwroot/app.css` instead for exactly that reason.

**Reference sheets are read-only - per sheet, not per domain.** A sheet Gig/Job marks
`ProtectSheet = true` (Gig's Addresses/Names/Places/Regions/Services/Types and its Daily/Weekly/
Monthly/Yearly/Weekday/Setup rollups; Job's Companies/Positions/Sites/Decisions/Interview Types/
Interview Outcomes/Schedules) is auto-generated from primary-sheet data - the grid detects this via
`ISheetOperations.GetSheetLayout(sheet)?.ProtectSheet` and drops the add/edit/delete affordances
entirely, leaving just a browsable, filterable table. This is genuinely per-sheet, not an assumption
baked in per domain: Home has zero `ProtectSheet = true` sheets at all - even Rooms/Contacts (the
sheets its own dropdowns validate against) are plain user-editable input sheets, and they render
editable here exactly because their own `GetSheetLayout` says so.

**Validated columns render as type-to-filter fields**, not free text or a plain dropdown - e.g. a
trip's `Service`/`Type`/`Place`/`Name`/`StartAddress`/`EndAddress`/`Region`, a job application's
`Company`/`JobTitle`/`Site`/`Decision`/`Schedule`, or a home appliance's `Location`. Each is validated
against a specific reference sheet (e.g. `Service` against Gig's Services sheet) - the same
relationship each domain uses to build the sheet's own Google Sheets data-validation dropdown, via
that domain's own `ISheetOperations.ValidationSheetMap` (column `ValidationPattern` -> reference
sheet name; empty for Stock, which has no validated columns). Each renders as a native
`<input list="...">` bound to a `<datalist>` of that column's reference values - one `<datalist>` per
column, rendered once and shared by every row, not duplicated per cell. That gives built-in browser
search-as-you-type with zero JavaScript, which matters for a column like Gig's `Name` that can carry
hundreds of reference values - a plain `<select>` would be an unusably long scroll. It also naturally
allows free text, matching how validation actually works here: the sheet's own Google Sheets data
validation is the real enforcement point, not this UI. The primary sheet loads first so a big sheet
like Trips (thousands of rows) isn't held up waiting on its lookup sheets; options for those columns
arrive a moment later from a background batched read of just the reference sheets, and the grid
re-renders once they're in.

Those reference-sheet reads are cached for 60 seconds in `ReferenceSheetCache` (a singleton, shared
across every page, every visitor, and every domain, not per-session) - since these sheets can't be
edited from the app, a short-lived read cache is safe and cuts out redundant API calls every time you
switch sheets. Cache keys are namespaced by domain name so two domains can't collide even if they
happened to reuse the same reference sheet name. Writes are never cached and always go straight to
the live spreadsheet.

**Table isn't the only view.** A "Cards" toggle next to the filter box switches to a read-only card
layout of the same rows - a demonstration that the data isn't inherently tabular, it's just what
`EntityGrid` happens to render by default. A card shows every input column plus the entity's first
column (its identity value - `TripEntity.Date`, `ServiceEntity.Service`, whichever it is) plus any
other read-only column that's an actual computed metric rather than a derived text label (`Total`
and `AmountPerDistance` show; `Key`/`Day`/`Month`/`Year`, which just restate the `Date` input, don't).
Both Table and Cards load 30 rows at a time with a "Load more" button rather than all at once -
rendering every row of a big sheet in one Blazor Server diff (Trips can run into the thousands) used
to hang the whole page, since a single-threaded circuit can't handle a nav click or even a scroll
reflow until that render finishes. Genuine scroll-triggered loading would need a JS interop call to
read scroll position that Blazor doesn't provide out of the box, so this trades automatic-on-scroll
for something that needs no JS at all.

## Sheet Inspector

**`/sheet-inspector` reverse-engineers a live tab instead of rendering a known entity.** Every other
page in this app already knows its shape from a domain's `[Column]` attributes; this one is for the
opposite situation - a hand-built tab RaptorSheets has never seen, or an entire **"Generic"**
connection with no compiled schema at all (see the `Type` dropdown in Settings' Connections section).
A Generic connection is only usable here - `GenericSheetOperations` deliberately isn't an
`ISheetOperations` (it carries none of the domain metadata - `SheetsType`, `ValidationSheetMap` - that
would mean anything for an arbitrary sheet), so it only ever hands back the plain `IConnectedSheet`
surface (structure/raw-data reads), never the typed CRUD `ITypedConnectedSheet` adds. Under the hood
it's built on Gig's compiled manager type purely as an invisible carrier - every method
`IConnectedSheet` exposes is implemented on `SheetManagerBase` itself without touching that domain's
registry, so any already-registered domain's manager would work identically; Gig is picked only
because it's always unconditionally registered.

**One "Inspect" click fires two independent reads concurrently** - `GetLiveSheetStructureAsync` and
`GetLiveSheetRawValuesAsync` (`Task.WhenAll`, not sequential) - rather than needing separate
"Inspect"/"Preview data" actions; a **Structure**/**Raw data** tab toggle then switches between the
two already-fetched results with no second round-trip. Typing a tab name that isn't in the
`<datalist>` (an unregistered sheet, or one that doesn't exist yet) still works - both reads accept
any live tab name.

**Structure** lists every detected header - Name, Column letter, `Format`, raw Google
`NumberFormat.Type`, format pattern, validation rule, and whether it's protected - the same
`SheetModel`/`SheetCellModel` shape `GetLiveSheetStructure` returns (see its own doc comment on
`ISheetManager`). A Note or Formula gets its own full-width detail row directly below the header it
belongs to (long text needs room to wrap, not a cramped column), sharing that header's zebra stripe
rather than a fixed color of its own, so it visually groups with its parent instead of risking fusion
with whichever neighbor happened to land on the same alternating color.

**Raw data** is `GetLiveSheetRawValues` - every cell by pure row/column position, capped at 200 rows
(`RawPreviewMaxRows`), with no assumption that row 1 is a header or that the sheet is a simple
one-row-per-record table. Rows are ragged (only as long as their own last populated cell), which
matters for a dashboard-style tab with fields scattered around rather than a plain grid. Whenever
Structure was also read for the same tab, row 1 highlights and the header row auto-freezes in the
grid - not a second guess, just reflecting what Structure already confirmed (format/validation live on
the *first data row*, never the header cell itself - see `GoogleRequestHelpers.GenerateRepeatCellRequest`
- so a structure read that came back clean already proves row 0 really is the header). Freezing column
A is a separate, always-available checkbox, since that has nothing to do with whether a header was
detected.

**"Generate class"** turns the current Structure result into a best-effort `[Column]`-decorated C#
class stub via `EntityClassGenerator` - a starting point for strongly-typing a hand-built tab, not a
finished mapping (the header comment on every generated class says so explicitly). Property type
resolution, in order: a `BOOLEAN` validation rule wins outright; then a recognized Google Sheets
`Format` (currency/number/percent/accounting/distance to `decimal?`, date/weekday/duration/time to
`string` - those round-trip as formatted strings via `GenericSheetMapper`, not `DateTime`, matching
how every shipped domain entity types them); then the raw `NumberFormat.Type` when no `Format` enum
matched (`NUMBER`/`CURRENCY`/`PERCENT` to `decimal?`, everything else - including `TEXT` - to
`string`). That last case is deliberate, not a fallback to guess past: a column explicitly formatted
as Plain Text (e.g. a version column holding `"1.10"`/`"1.9"`) must stay `string` even though sampled
values look numeric, since Sheets would otherwise silently collapse `"1.10"` to `1.1` - real row
samples are only consulted (`InferTypeFromSamples`) when a column has **no** format metadata
whatsoever, true for a plain hand-built sheet nobody ever explicitly formatted, and even then every
non-blank sample must agree on one type or it falls back to `string` rather than produce a class that
can't parse its own data.

**"Bulk generate classes"** is a two-stage confirm, not one click: it first opens a picker listing
every known tab name, all pre-checked (free - already loaded from `GetAllSheetTabNamesAsync`, no read
happens yet), so the selection can be trimmed before anything is fetched. Confirming runs exactly two
batched calls total - `GetLiveSheetStructuresAsync`/`GetLiveSheetsRawValuesAsync` across every selected
tab at once, not one round-trip per tab - then generates one class per tab that actually returned a
structure (a tab missing from the result, e.g. deleted mid-flight, is listed as couldn't-read rather
than silently dropped) and concatenates them under one shared `using` header. It also emits an
aggregating container - `{Base}Sheets`/`{Base}Entity`, mirroring e.g.
`RaptorSheets.Gig.Entities.GigSheets`/`SheetEntity` exactly - where `{Base}` is a derived placeholder
from the connection's own label; the
generated comment says outright that it's a placeholder, since there's no other prompt telling you to
rename it before using it for real.

**Saved sheet names** (the "Saved sheets" panel below the Tab field) lets a connection remember extra
tab names to offer later - a hand-built tab that isn't live yet, or one borrowed from a completely
different domain's schema (the suggestion list is sourced from every registered domain, not just the
selected connection's own type - useful for trying, say, a known Gig sheet name against a Generic
connection).

## Known limitations (first pass)

- "Discard changes" resets in-memory edits back to what was last loaded; it doesn't re-fetch from
  the spreadsheet. Use the sheet's nav link again (or a page refresh) to pull the latest data.
- The "connect a blank spreadsheet" demo-data flow follows the documented Gig README sequence
  exactly and the credential-write/parse/reconnect mechanics are verified against real (and
  deliberately malformed, to check error handling) input, but the create-sheets-then-fill-with-
  demo-data button itself hasn't been exercised against a genuinely empty spreadsheet.
