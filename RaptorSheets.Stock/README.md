# RaptorSheets.Stock

> **Status: in development.** The read path (sheets, mapping, self-heal, header validation) and sheet
> creation/deletion work and are tested. `ChangeSheetData` is wired for the Stocks sheet's `Shares`
> column only - Accounts and Tickers are fully formula/`GOOGLEFINANCE`-driven rollups with nothing
> else for a user to change. APIs may still change.

## Overview

RaptorSheets.Stock is a specialized implementation of [RaptorSheets.Core](../README.md) for tracking
investments and portfolios in Google Sheets. It provides pre-configured sheet types and
strongly-typed entities for accounts, stocks, and tickers.

Like every RaptorSheets domain package, it is a thin layer over the shared
`GoogleSheetManagerBase<TEntity>` in Core — see the [Architecture](../README.md#️-architecture)
section of the Core README. The Stock package only supplies its entities/mappers, its
`SheetRegistry<SheetEntity>` (`StockSheetHelpers.Registry`), and its write operations; all read,
metadata, layout, and self-heal orchestration is inherited from Core.

## Sheets

| Sheet | Entity | Purpose |
|-------|--------|---------|
| **Accounts** | `AccountEntity` | Brokerage/holding accounts |
| **Stocks** | `StockEntity` | Individual holdings/positions |
| **Tickers** | `TickerEntity` | Ticker reference data |

## Installation

```bash
dotnet add package RaptorSheets.Stock
```

## Quick Start

```csharp
using RaptorSheets.Core.Extensions;
using RaptorSheets.Stock.Managers;
using RaptorSheets.Stock.Enums;

// Initialize (access token or service-account credentials, same as other packages)
var manager = new GoogleSheetManager("your-access-token", "spreadsheet-id");

// Create the stock sheets
await manager.CreateAllSheets();

// Read everything (batch read → self-heal missing sheets → map → auto-heal missing columns)
var data = await manager.GetAllSheets();
Console.WriteLine($"Accounts: {data.Sheets.Accounts.Count}, Stocks: {data.Sheets.Stocks.Count}");

// Read a subset by name
var subset = await manager.GetSheets(new List<string> { SheetEnum.ACCOUNTS.GetDescription() });
```

### Dependency injection

```csharp
using RaptorSheets.Stock.Extensions;

// One spreadsheet, bound from configuration
builder.Services.AddRaptorSheetsStock(options =>
{
    options.SpreadsheetId = builder.Configuration["Sheets:SpreadsheetId"];
    options.AccessToken = builder.Configuration["Sheets:AccessToken"];
});

// Or, when the spreadsheet varies per request or per signed-in user
builder.Services.AddRaptorSheetsStock();
// ... then: factory.Create(userToken, userSpreadsheetId)
```

See [Getting Started](https://github.com/khanjal/RaptorSheets/blob/main/docs/GETTING-STARTED.md#dependency-injection) for details.

## Data Operations

### Creating and Deleting Sheets
```csharp
// Create all predefined sheets
await manager.CreateAllSheets();

// Create specific sheets
await manager.CreateSheets(new List<string> { "Accounts", "Stocks" });

// Delete everything (creates a temporary safety sheet first if it would otherwise leave zero
// sheets in the spreadsheet, since Google Sheets requires at least one to remain)
await manager.DeleteAllSheets();

// Delete specific sheets
await manager.DeleteSheets(new List<string> { "Tickers" });
```

### Updating Data
`Shares` on the Stocks sheet is the only field a user can actually edit - every other column on
Accounts/Stocks/Tickers is a cross-sheet formula or a live `GOOGLEFINANCE` pull, so there's nothing
else for `ChangeSheetData` to change:

```csharp
var sheetEntity = new SheetEntity
{
    Sheets =
    {
        Stocks = new List<StockEntity>
        {
            new() { RowId = 2, Shares = 10 } // RowId identifies the existing row to update
        }
    }
};

var result = await manager.ChangeSheetData(new List<string> { "Stocks" }, sheetEntity);
```

## Inherited from Core

Because `GoogleSheetManager` inherits `GoogleSheetManagerBase<SheetEntity>`, these come from Core with
no Stock-specific code:

- `GetSheets` / `GetAllSheets` — orchestration incl. missing-sheet self-heal and missing-column auto-heal
- `GetSheetProperties` / `GetAllSheetProperties`, `GetAllSheetTabNames`
- `GetSheetLayout` / `GetSheetLayouts`
- `InsertMissingColumns`, `GetSpreadsheetInfo`, `GetBatchData`
- `CreateSheets` / `CreateAllSheets`, `DeleteSheets` / `DeleteAllSheets` — ordered creation and
  temp-sheet-safe deletion, same as Gig; Stock supplies its own `GenerateSheetsRequest` override
  with its fully-configured `AddSheet` requests

Stock-specific: `ChangeSheetData` (Shares only, see above) and the static `CheckSheetHeaders` /
`CheckUnknownSheets` helpers.

## Related

- **[RaptorSheets.Core](../README.md)** — shared foundation and the manager base described above
- **[RaptorSheets.Gig](../RaptorSheets.Gig/README.md)** — a complete, production domain package built the same way
