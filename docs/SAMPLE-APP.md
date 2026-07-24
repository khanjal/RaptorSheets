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

Edits are tracked locally (add / edit / delete) and only sent on "Save changes", as one batched
`ChangeSheetData` call per sheet - never one write per keystroke.

## Known limitations (first pass)

- Columns with `EnableValidation` (e.g. `Service` on a trip, which is validated against a named
  range in the sheet) render as a plain text input, not a dropdown sourced from that range. Typing
  an invalid value saves fine but the sheet's own validation will flag it - the same as typing it
  directly into Google Sheets.
- "Discard changes" resets in-memory edits back to what was last loaded; it doesn't re-fetch from
  the spreadsheet. Use the sheet's nav link again (or a page refresh) to pull the latest data.
