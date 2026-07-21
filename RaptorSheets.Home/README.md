# RaptorSheets.Home

A specialized library built on [RaptorSheets.Core](https://www.nuget.org/packages/RaptorSheets.Core) for
tracking **home maintenance and inventory** in Google Sheets.

## Sheets

| Sheet | Purpose |
| --- | --- |
| **Appliances & Electronics** | Inventory of appliances/electronics, including filter tracking (with an auto-calculated *Next Filter* date). |
| **Projects** | Home improvement / project tracking. |
| **Maintenance Log** | Record of problems, who fixed them, and cost. |
| **Doors** | Door inventory and specifications. |
| **Paints** | Paint colors used per location (for touch-ups). |
| **Power** | Electrical outlets / breakers. |
| **Rooms** | Room dimensions (with an auto-calculated *Sq. Ft* from L × W). Feeds *Location* dropdowns. |
| **Contacts** | Service providers and people. Feeds the *Company/Person* dropdown on the Maintenance Log. |
| **Stats** | Property facts (beds, baths, year built, parcel #, etc.) as name/value pairs. |

## Usage

```csharp
using RaptorSheets.Home.Managers;

var manager = new GoogleSheetManager(accessToken, spreadsheetId);

// Create every Home sheet
await manager.CreateAllSheets();

// Read everything back
var data = await manager.GetAllSheets();
```

See [RaptorSheets](https://github.com/khanjal/RaptorSheets) for authentication and full documentation.
