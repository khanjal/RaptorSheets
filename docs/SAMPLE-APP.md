# Sample App

`RaptorSheets.Sample.Web` is a runnable Blazor Server app that gives you a browser UI over a real
spreadsheet, using whichever domain manager you point it at. It's the fastest way to see whether
the library does what you need: pick a sheet, browse the rows, add/edit/delete a few, and watch the
result land in the actual Google Sheet.

It's plain ASP.NET Core - `dotnet run` is the whole setup story, no Node/npm toolchain required.

Currently wired up for the **Gig** domain; Stock, Job, and Home are on the nav as "coming soon" and
will be added the same way, one at a time.

## Running it

The sample reads credentials from [user secrets](AUTHENTICATION.md#console-application-with-user-secrets)
the same way the integration test suite does - nothing is read from `appsettings.json`, so there's
nothing to accidentally commit.

```bash
cd RaptorSheets.Sample.Web
dotnet user-secrets init

dotnet user-secrets set "GoogleCredentials:type" "service_account"
dotnet user-secrets set "GoogleCredentials:private_key_id" "your-key-id"
dotnet user-secrets set "GoogleCredentials:private_key" "your-private-key"
dotnet user-secrets set "GoogleCredentials:client_email" "service@project.iam.gserviceaccount.com"
dotnet user-secrets set "GoogleCredentials:client_id" "your-client-id"
dotnet user-secrets set "Spreadsheets:Gig" "your-gig-spreadsheet-id"

dotnet run
```

If a secret is missing, the app shows a setup message in the browser instead of crashing - it only
connects on first use, not at startup.

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

## Known limitations (first pass)

- "Discard changes" resets in-memory edits back to what was last loaded; it doesn't re-fetch from
  the spreadsheet. Use the sheet's nav link again (or a page refresh) to pull the latest data.
- A reference sheet with a lot of rows (e.g. Names) produces a long native `<select>` - there's no
  search/filter inside the dropdown itself yet.
