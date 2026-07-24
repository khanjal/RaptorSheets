# Sample App

`RaptorSheets.Sample.Web` is a runnable Blazor Server app that gives you a browser UI over a real
spreadsheet, using whichever domain manager you point it at. It's the fastest way to see whether
the library does what you need: pick a sheet, browse the rows, add/edit/delete a few, and watch the
result land in the actual Google Sheet.

It's plain ASP.NET Core - `dotnet run` is the whole setup story, no Node/npm toolchain required.

Currently wired up for the **Gig** domain; Stock, Job, and Home are on the nav as "coming soon" and
will be added the same way, one at a time.

## Running it

```bash
dotnet run --project RaptorSheets.Sample.Web
```

If no spreadsheet is configured yet, the Home page itself is a setup wizard rather than just an
error message: it walks through creating a Google Cloud service account and sharing a spreadsheet
with it, then a form to paste the service account's JSON key and the spreadsheet ID. The JSON
textarea is masked by default (a "Show" toggle reveals it - CSS-only, degrades to plaintext on
Firefox, an acceptable tradeoff for a localhost-only dev tool). Submitting writes straight to the
local `secrets.json` and reconnects immediately, no restart needed.

`RaptorSheets.Sample.Web` and `RaptorSheets.Test` (the integration test suite's shared infra)
deliberately declare the **same `<UserSecretsId>`**, so they read one `secrets.json`
(`%APPDATA%\Microsoft\UserSecrets\d3dcd413-.../secrets.json` on Windows,
`~/.microsoft/usersecrets/d3dcd413-.../secrets.json` on Linux/macOS) instead of each needing its
own copy kept in sync by hand - **the setup wizard configures both projects**, and has an optional
"also set up Stock / Job / Home spreadsheets" section for exactly that, even though the sample app
itself only uses Gig for now. Nothing is read from `appsettings.json`, so there's nothing to
accidentally commit either way.

Prefer the CLI? Same keys, same store, either project's directory works:

```bash
cd RaptorSheets.Sample.Web

dotnet user-secrets set "google_credentials:type" "service_account"
dotnet user-secrets set "google_credentials:private_key_id" "your-key-id"
dotnet user-secrets set "google_credentials:private_key" "your-private-key"
dotnet user-secrets set "google_credentials:client_email" "service@project.iam.gserviceaccount.com"
dotnet user-secrets set "google_credentials:client_id" "your-client-id"
dotnet user-secrets set "spreadsheets:gig" "your-gig-spreadsheet-id"
```

**If the spreadsheet you connect is blank** (no Gig sheets on it yet - checked via
`GetAllSheetTabNames()` against the known Gig sheet names, not just "zero tabs", since a fresh
Google Sheet always starts with one default "Sheet1" tab), the Home page offers a one-click
"Create sheets + fill with demo data" button: `CreateAllSheets()` then `GenerateDemoData()` then
`ChangeSheetData(["Shifts", "Trips", "Expenses"], demoData)`, the exact sequence documented in
[RaptorSheets.Gig's README](../RaptorSheets.Gig/README.md#demo-setup). The other 14 sheets fill in
on their own from the sheet formulas once Shifts/Trips/Expenses have real rows.

## How the grid works

Every domain entity already declares its own schema via `[Column(...)]` attributes - header name,
whether it's user-editable or a read-only formula/output column, validation rules, display format.
Rather than hand-building a page per entity (Gig alone has 17 sheet types), the sample reflects over
that metadata once, using the same reflection Core's own mapper relies on
(`RaptorSheets.Core.Utilities.TypedFieldUtils.GetColumnProperties`), and renders one generic
`EntityGrid<TRow>` component for whichever sheet you pick. Adding a new column to a domain entity
means it just shows up - no sample-app code changes.

New rows go through an "Add item" form rather than an inline blank row - fill in the fields, confirm,
and it's staged alongside any inline edits. Nothing is sent to Google until you click "Save changes",
which batches every pending add/edit/delete for the sheet into one `ChangeSheetData` call - never a
write per keystroke.

**Reference sheets are read-only.** Sheets Gig marks `ProtectSheet = true` (Addresses, Deliveries,
Locations, Names, Places, Regions, Services, Types, and the Daily/Weekly/Monthly/Yearly/Weekday/Setup
rollups) are auto-generated from Trips/Shifts/Expenses data - the grid detects this via
`IGoogleSheetManager.GetSheetLayout(sheet)?.ProtectSheet` and drops the add/edit/delete affordances
entirely, leaving just a browsable, filterable table.

**Validated columns render as dropdowns**, not free text. `Service`, `Type`, `Place`, `Name`,
`StartAddress`/`EndAddress`, and `Region` on a trip are each validated against a specific reference
sheet (e.g. `Service` against the Services sheet) - the same relationship Gig uses to build the
sheet's own Google Sheets data-validation dropdown. The primary sheet loads first so a big sheet like
Trips (thousands of rows) isn't held up waiting on six lookup sheets; dropdown options for those
columns arrive a moment later from a background batched read of just the reference sheets, and the
grid re-renders once they're in.

Those reference-sheet reads are cached for 60 seconds in `ReferenceSheetCache` (a singleton, shared
across every page and every visitor, not per-session) - since these sheets can't be edited from the
app, a short-lived read cache is safe and cuts out redundant API calls every time you switch sheets.
Writes are never cached and always go straight to the live spreadsheet.

**Table isn't the only view.** A "Cards" toggle next to the filter box switches to a read-only card
layout of the same rows - a demonstration that the data isn't inherently tabular, it's just what
`EntityGrid` happens to render by default. A card shows every input column plus the entity's first
column (its identity value - `TripEntity.Date`, `ServiceEntity.Service`, whichever it is) plus any
other read-only column that's an actual computed metric rather than a derived text label (`Total`
and `AmountPerDistance` show; `Key`/`Day`/`Month`/`Year`, which just restate the `Date` input, don't).
Cards load 30 at a time with a "Load more" button rather than all at once - genuine scroll-triggered
loading would need a JS interop call to read scroll position that Blazor doesn't provide out of the
box, so this trades automatic-on-scroll for something that needs no JS at all.

## Known limitations (first pass)

- "Discard changes" resets in-memory edits back to what was last loaded; it doesn't re-fetch from
  the spreadsheet. Use the sheet's nav link again (or a page refresh) to pull the latest data.
- A reference sheet with a lot of rows (e.g. Names) produces a long native `<select>` - there's no
  search/filter inside the dropdown itself yet.
- The "connect a blank spreadsheet" demo-data flow follows the documented Gig README sequence
  exactly and the credential-write/parse/reconnect mechanics are verified against real (and
  deliberately malformed, to check error handling) input, but the create-sheets-then-fill-with-
  demo-data button itself hasn't been exercised against a genuinely empty spreadsheet.
