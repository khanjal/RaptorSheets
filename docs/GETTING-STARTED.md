# Getting Started with RaptorSheets

This guide will help you get up and running with RaptorSheets quickly, whether you need custom Google Sheets integration or specialized gig work tracking.

## Table of Contents
1. [Choose Your Package](#choose-your-package)
2. [Prerequisites](#prerequisites)
3. [Authentication Setup](#authentication-setup)
4. [Quick Start Examples](#quick-start-examples)
5. [Next Steps](#next-steps)

## Choose Your Package

| Package | Best For | When to Use |
|---------|----------|-------------|
| **RaptorSheets.Core** | Custom integrations | Building your own sheet management solutions |
| **RaptorSheets.Gig** | Gig work tracking | Ready-made solution for freelancers and gig workers |

## Prerequisites

- **.NET 8.0 SDK** or later
- **Google Cloud Project** with Sheets API enabled
- **Google credentials** (Service Account recommended)

### Enable Google Sheets API

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Select or create a project
3. Navigate to **APIs & Services** > **Library**
4. Search for "Google Sheets API" and enable it
5. If using RaptorSheets.Gig, also enable "Google Drive API"

## Authentication Setup

RaptorSheets supports multiple authentication methods. Choose the one that fits your use case:

### Option 1: Service Account (Recommended)
Best for server applications and automated processes.

```csharp
var credentials = new Dictionary<string, string>
{
    ["type"] = "service_account",
    ["private_key_id"] = Environment.GetEnvironmentVariable("GOOGLE_PRIVATE_KEY_ID"),
    ["private_key"] = Environment.GetEnvironmentVariable("GOOGLE_PRIVATE_KEY"),
    ["client_email"] = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_EMAIL"),
    ["client_id"] = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID")
};
```

### Option 2: OAuth2 Access Token
Best for client applications where users authenticate themselves.

```csharp
string accessToken = "your-oauth2-access-token";
```

**📖 [Complete Authentication Guide](AUTHENTICATION.md)**

## Quick Start Examples

### RaptorSheets.Core - Custom Integration

#### 1. Installation
```bash
dotnet add package RaptorSheets.Core
```

#### 2. Basic Usage
```csharp
using RaptorSheets.Core.Services;
using RaptorSheets.Core.Models.Google;

// Initialize service
var service = new GoogleSheetService(credentials, spreadsheetId);

// Get sheet information
var sheetInfo = await service.GetSheetInfo();
Console.WriteLine($"Spreadsheet: {sheetInfo.Properties.Title}");

// Read data
var data = await service.GetSheetData("Sheet1");
foreach (var row in data.Values)
{
    Console.WriteLine(string.Join(", ", row));
}

// Write data
var valueRange = new ValueRange
{
    Values = new List<IList<object>>
    {
        new List<object> { "Header1", "Header2" },
        new List<object> { "Value1", "Value2" }
    }
};
await service.UpdateData(valueRange, "Sheet1!A1:B2");
```

#### 3. Create Custom Sheets
```csharp
using RaptorSheets.Core.Models.Sheet;
using RaptorSheets.Core.Enums;

// Define sheet structure
var sheetModel = new SheetModel
{
    Name = "CustomSheet",
    TabColor = ColorEnum.BLUE,
    CellColor = ColorEnum.LIGHT_GRAY,
    Headers = new List<SheetCellModel>
    {
        new() { Name = "ID", Format = FormatEnum.NUMBER },
        new() { Name = "Name", Format = FormatEnum.TEXT },
        new() { Name = "Date", Format = FormatEnum.DATE }
    }
};

// Generate and execute sheet creation requests
var requests = sheetModel.GenerateRequests();
await service.ExecuteBatchUpdate(requests);
```

### RaptorSheets.Gig - Gig Work Tracking

#### 1. Installation
```bash
dotnet add package RaptorSheets.Gig
```

#### 2. Initialize and Create Sheets
```csharp
using RaptorSheets.Gig.Managers;
using RaptorSheets.Gig.Entities;

// Initialize manager
var manager = new GoogleSheetManager(credentials, spreadsheetId);

// Create all gig tracking sheets with formatting
await manager.CreateSheets();
```

#### 3. Add Trip Data
```csharp
var trips = new List<TripsEntity>
{
    new()
    {
        Date = DateTime.Now.ToString("yyyy-MM-dd"),
        Service = "Uber",
        Type = "Delivery",
        Pay = 15.50m,
        Tips = 3.00m,
        AddressStart = "123 Main St",
        AddressEnd = "456 Oak Ave"
    }
};

await manager.AddTrips(trips);
```

#### 4. Get Analytics
```csharp
using RaptorSheets.Gig.Enums;

// Get specific sheets
var sheetTypes = new List<SheetEnum> { SheetEnum.Trips, SheetEnum.Daily };
var response = await manager.GetSheets(sheetTypes);

foreach (var sheet in response.Sheets)
{
    Console.WriteLine($"Sheet: {sheet.Name}");
    Console.WriteLine($"Rows: {sheet.Data.Count}");
}

// Get all sheets
var allData = await manager.GetSheets();
```

## Next Steps

### For RaptorSheets.Core Users
- **[📖 Core Documentation](CORE.md)** - Detailed API reference
- **[🏗️ Advanced Usage](CORE.md#advanced-features)** - Simplified mappers with automated field mapping
- **[🧪 Testing](CORE.md#testing)** - Unit testing your integrations

### For RaptorSheets.Gig Users
- **[📖 Gig Documentation](../RaptorSheets.Gig/README.md)** - Complete feature guide
- **[📊 Sheet Types](../RaptorSheets.Gig/README.md#sheet-types)** - Understanding all available sheets
- **[💡 Examples](../RaptorSheets.Gig/README.md#examples)** - Real-world usage scenarios

### For All Users
- **[🔐 Authentication Details](AUTHENTICATION.md)** - Complete setup guide
- **[🚦 Performance Tips](../README.md#performance--api-limits)** - Optimization strategies
- **[🤝 Contributing](../README.md#contributing)** - How to contribute to the project

## Common Issues

### Authentication Errors
- Ensure your service account has access to the spreadsheet
- Check that required APIs are enabled in Google Cloud Console
- Verify environment variables are set correctly

### Quota Limits
- Google Sheets API has rate limits (100 requests per 100 seconds)
- Use batch operations for bulk data operations
- RaptorSheets automatically handles retries

### Permission Errors
- Share your spreadsheet with the service account email
- Grant "Editor" permissions for write operations
- For read-only access, "Viewer" permissions are sufficient

## Support

- 🐞 [Report Issues](https://github.com/khanjal/RaptorSheets/issues)
- 💬 [Ask Questions](https://github.com/khanjal/RaptorSheets/discussions)
- 📖 [API Reference](https://googleapis.dev/dotnet/Google.Apis.Sheets.v4/latest/)