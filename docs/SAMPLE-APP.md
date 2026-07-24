# Sample App

`RaptorSheets.Sample.Web` is a runnable Blazor Server app that gives you a browser UI over a real
spreadsheet, using whichever domain manager you point it at. It's the fastest way to see whether
the library does what you need: pick a sheet, browse the rows, add/edit/delete a few, and watch the
result land in the actual Google Sheet.

It's plain ASP.NET Core - `dotnet run` is the whole setup story, no Node/npm toolchain required.

Currently wired up for the **Gig** domain; Stock, Job, and Home are on the nav as "coming soon" and
will be added the same way, one at a time.

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
are what actually gets saved, the paste box is just a shortcut into them. Every field is
independently optional on save - change just one domain's spreadsheet ID without touching
credentials, or vice versa. Submitting writes straight to the local `secrets.json` and reconnects
immediately, no restart needed. Credentials are replace-only: Settings shows which service account
is currently active (`client_email` - safe to display) but never re-displays the private key itself,
the same "write, don't show" pattern every password/API-key settings screen uses.

`RaptorSheets.Sample.Web` and `RaptorSheets.Test` (the integration test suite's shared infra)
deliberately declare the **same `<UserSecretsId>`**, so they read one `secrets.json`
(`%APPDATA%\Microsoft\UserSecrets\d3dcd413-.../secrets.json` on Windows,
`~/.microsoft/usersecrets/d3dcd413-.../secrets.json` on Linux/macOS) instead of each needing its own
copy kept in sync by hand - **Settings configures both projects at once**. Stock/Job/Home spreadsheet
IDs live there for exactly that reason, even though the sample app's nav only browses Gig for now.
Nothing is read from `appsettings.json`, so there's nothing to accidentally commit either way.

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

**Validated columns render as type-to-filter fields**, not free text or a plain dropdown. `Service`,
`Type`, `Place`, `Name`, `StartAddress`/`EndAddress`, and `Region` on a trip are each validated
against a specific reference sheet (e.g. `Service` against the Services sheet) - the same
relationship Gig uses to build the sheet's own Google Sheets data-validation dropdown. Each of these
renders as a native `<input list="...">` bound to a `<datalist>` of that column's reference values -
one `<datalist>` per column, rendered once and shared by every row, not duplicated per cell. That
gives built-in browser search-as-you-type with zero JavaScript, which matters for a column like
`Name` that can carry hundreds of reference values - a plain `<select>` would be an unusably long
scroll. It also naturally allows free text, matching how validation actually works here: the sheet's
own Google Sheets data validation is the real enforcement point, not this UI. The primary sheet loads
first so a big sheet like Trips (thousands of rows) isn't held up waiting on six lookup sheets;
options for those columns arrive a moment later from a background batched read of just the reference
sheets, and the grid re-renders once they're in.

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
Both Table and Cards load 30 rows at a time with a "Load more" button rather than all at once -
rendering every row of a big sheet in one Blazor Server diff (Trips can run into the thousands) used
to hang the whole page, since a single-threaded circuit can't handle a nav click or even a scroll
reflow until that render finishes. Genuine scroll-triggered loading would need a JS interop call to
read scroll position that Blazor doesn't provide out of the box, so this trades automatic-on-scroll
for something that needs no JS at all.

## Known limitations (first pass)

- "Discard changes" resets in-memory edits back to what was last loaded; it doesn't re-fetch from
  the spreadsheet. Use the sheet's nav link again (or a page refresh) to pull the latest data.
- The "connect a blank spreadsheet" demo-data flow follows the documented Gig README sequence
  exactly and the credential-write/parse/reconnect mechanics are verified against real (and
  deliberately malformed, to check error handling) input, but the create-sheets-then-fill-with-
  demo-data button itself hasn't been exercised against a genuinely empty spreadsheet.
