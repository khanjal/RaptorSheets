# RaptorSheets.Job

A specialized library built on [RaptorSheets.Core](https://www.nuget.org/packages/RaptorSheets.Core) for
tracking **job applications and interviews** in Google Sheets.

## Sheets

| Sheet | Purpose |
| --- | --- |
| **Applications** | Job applications you've submitted (date, company, title, site, decision, pay, …). Auto-calculates interview count, days active, pay average, and a duplicate counter. |
| **Interviews** | Interviews linked to applications (times, type, round, recruiter, outcome). Auto-calculates round number. |
| **Company Details** / **Position Details** | Optional user-entered detail sheets. |
| **Companies** / **Positions** | Reference sheets auto-derived from Applications, with application/interview counts. |
| **Sites** / **Decisions** / **Interview Types** / **Interview Outcomes** / **Schedules** | Dropdown reference lists auto-derived from your data, with counts. |
| **Setup** | Configuration (name/value). |

## Usage

```csharp
using RaptorSheets.Job.Managers;

var manager = new GoogleSheetManager(accessToken, spreadsheetId);

// Create every Job sheet
await manager.CreateAllSheets();

// Create sheets and fill them with realistic demo data
await manager.SetupDemo();

// Read everything back
var data = await manager.GetAllSheets();
```

See [RaptorSheets](https://github.com/khanjal/RaptorSheets) for authentication and full documentation.
