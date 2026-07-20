# RaptorSheets.Stock

> **Status: in development.** The read path (sheets, mapping, self-heal, header validation) works and
> is tested; write operations beyond sheet creation (`ChangeSheetData`, `DeleteSheets`) are not
> implemented yet. APIs may change.

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
using RaptorSheets.Stock.Managers;
using RaptorSheets.Stock.Enums;

// Initialize (access token or service-account credentials, same as other packages)
var manager = new GoogleSheetManager("your-access-token", "spreadsheet-id");

// Create the stock sheets
await manager.CreateSheets();

// Read everything (batch read → self-heal missing sheets → map → auto-heal missing columns)
var data = await manager.GetSheets();
Console.WriteLine($"Accounts: {data.Accounts.Count}, Stocks: {data.Stocks.Count}");

// Read a subset by enum
var subset = await manager.GetSheets(new List<SheetEnum> { SheetEnum.ACCOUNTS });
```

## Inherited from Core

Because `GoogleSheetManager` inherits `GoogleSheetManagerBase<SheetEntity>`, these come from Core with
no Stock-specific code:

- `GetSheets` / `GetAllSheets` — orchestration incl. missing-sheet self-heal and missing-column auto-heal
- `GetSheetProperties` / `GetAllSheetProperties`, `GetAllSheetTabNames`
- `GetSheetLayout` / `GetSheetLayouts`
- `InsertMissingColumns`, `GetSpreadsheetInfo`, `GetBatchData`

Stock-specific: `CreateSheets` (enum-based), `AddSheetData`, and the static `CheckSheetHeaders` /
`CheckUnknownSheets` helpers.

## Related

- **[RaptorSheets.Core](../README.md)** — shared foundation and the manager base described above
- **[RaptorSheets.Gig](../RaptorSheets.Gig/README.md)** — a complete, production domain package built the same way
