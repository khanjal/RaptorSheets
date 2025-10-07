# RaptorSheets.Core

A comprehensive .NET 8 library that simplifies Google Sheets API interactions for developers who need powerful sheet integration without the complexity. Build custom Google Sheets solutions or use our specialized packages for common use cases.

**[📋 Gig Package](RaptorSheets.Gig/README.md)** — Complete gig work tracking guide.

| Badge Name | Status | Site |
| ---------- | :------------: | :------------: |
| Latest Build Status | [![build status](https://github.com/khanjal/RaptorSheets/actions/workflows/dotnet.yml/badge.svg)](https://github.com/khanjal/RaptorSheets/actions) | [GitHub Repo](https://github.com/khanjal/RaptorSheets/) |
| RaptorSheets.Core NuGet | [![Nuget](https://img.shields.io/nuget/v/RaptorSheets.Core)](https://www.nuget.org/packages/RaptorSheets.Core/) | [RaptorSheets.Core](https://www.raptorsheets.com) |
| RaptorSheets.Gig NuGet | [![Nuget](https://img.shields.io/nuget/v/RaptorSheets.Gig)](https://www.nuget.org/packages/RaptorSheets.Gig/) | [RaptorSheets.Gig](https://www.nuget.org/packages/RaptorSheets.Gig/) |
| Test Coverage | [![Coverage](https://sonarcloud.io/api/project_badges/measure?project=khanjal_RaptorSheets&metric=coverage)](https://sonarcloud.io/summary/new_code?id=khanjal_RaptorSheets) | [SonarCloud](https://sonarcloud.io/project/overview?id=khanjal_RaptorSheets) |
| Code Quality | [![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=khanjal_RaptorSheets&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=khanjal_RaptorSheets) | [SonarCloud](https://sonarcloud.io/project/overview?id=khanjal_RaptorSheets) |
| License | [![License](https://img.shields.io/github/license/khanjal/RaptorSheets)](LICENSE) | - |

## 📦 Installation

```bash
# Core library for custom implementations
dotnet add package RaptorSheets.Core

# Or choose a specialized package
dotnet add package RaptorSheets.Gig    # For gig work tracking
```

## 📖 Project Documentation

| Project | Description | Documentation |
|---------|-------------|---------------|
| **RaptorSheets.Core** | Core library with Google Sheets integration | [View Docs](docs/CORE.md) |
| **RaptorSheets.Gig** | Specialized library for gig work tracking | [View Docs](RaptorSheets.Gig/README.md) |

## 🚀 Quick Start

```csharp
using RaptorSheets.Core.Services;
using RaptorSheets.Core.Models.Google;

// Set up authentication
var credentials = new Dictionary<string, string>
{
    ["type"] = "service_account",
    ["client_email"] = "service@project.iam.gserviceaccount.com",
    ["private_key"] = "-----BEGIN PRIVATE KEY-----\n...",
    // ... other credentials
};

var service = new GoogleSheetService(credentials, spreadsheetId);

// Read data from existing sheet
var sheetData = await service.GetSheetData("MySheet");
Console.WriteLine($"Found {sheetData.Values.Count} rows");

// Create custom sheet with formatting
var sheetModel = new SheetModel
{
    Name = "CustomSheet",
    TabColor = ColorEnum.BLUE,
    Headers = new List<SheetCellModel>
    {
        new() { Name = "ID", Format = FormatEnum.NUMBER },
        new() { Name = "Name", Format = FormatEnum.TEXT },
        new() { Name = "Amount", Format = FormatEnum.CURRENCY }
    }
};

// Generate and execute requests
var requests = sheetModel.GenerateRequests();
await service.ExecuteBatchUpdate(requests);

// Update data
var valueRange = new ValueRange
{
    Values = new List<IList<object>>
    {
        new List<object> { "ID", "Name", "Amount" },
        new List<object> { 1, "John Doe", 150.75 }
    }
};
await service.UpdateData(valueRange, "CustomSheet!A1:C2");
```

## 📚 Specialized Packages

Built on RaptorSheets.Core, these packages provide domain-specific functionality:

| Package | Version | Purpose | Documentation |
|---------|---------|---------|---------------|
| **[RaptorSheets.Gig](https://www.nuget.org/packages/RaptorSheets.Gig/)** | [![Nuget](https://img.shields.io/nuget/v/RaptorSheets.Gig)](https://www.nuget.org/packages/RaptorSheets.Gig/) | Complete gig work tracking with automated analytics | **[📖 Gig Guide](RaptorSheets.Gig/README.md)** |

> **Looking for gig work tracking?** Check out **[RaptorSheets.Gig](RaptorSheets.Gig/README.md)** - a complete solution for freelancers and gig workers with pre-built sheets for trips, shifts, earnings, and comprehensive analytics.

## 📚 Core Library Overview

RaptorSheets.Core provides the foundational infrastructure for Google Sheets integration, designed to handle complex spreadsheets with automated formulas, cross-sheet references, and strict column ordering.

### ✨ Key Features

- **📋 Header Management**: Extension methods for column and index assignments with automatic processing
- **🎨 Column Formatting**: Apply data formatting, configure drop-downs, and set cell protection
- **🎯 Sheet Styling**: Alternating row colors, full sheet protection, and custom tab colors
- **⚡ Batch Operations**: Efficient bulk operations for large datasets with automatic batching
- **🔒 Type Safety**: Strongly typed entities and enums for all operations
- **✅ Auto Validation**: Automatic header validation with detailed error reporting
- **🛠️ Error Handling**: Comprehensive message system for operation feedback
- **🧪 Well Tested**: Extensive unit and integration test coverage

### 🏗️ Architecture

```
Your Custom Application
       ↓
RaptorSheets.Core
  ├── GoogleSheetService (High-level operations)
  ├── SheetServiceWrapper (API abstraction)  
  ├── Models & Entities (Type safety)
  └── Extensions & Helpers (Utilities)
       ↓
Google Sheets API v4
```

### 💼 Use Cases

- **Custom Business Solutions**: Build domain-specific Google Sheets integrations for any industry
- **Data Pipeline Integration**: Automate data sync between your applications and collaborative spreadsheets  
- **Advanced Report Generation**: Create complex reports with formulas, cross-sheet references, and automated calculations
- **Workflow Automation**: Streamline business processes that rely on Google Sheets data
- **Foundation for Specialized Packages**: Use as a base to create domain-specific managers (like RaptorSheets.Gig)

## 📖 Documentation

| Documentation | Purpose | Audience |
|---------------|---------|----------|
| **[🚀 Getting Started](docs/GETTING-STARTED.md)** | Quick setup and first steps | All users |
| **[🔐 Authentication](docs/AUTHENTICATION.md)** | Google API setup guide | All users |
| **[🔧 Core Library](docs/CORE.md)** | Complete Core functionality reference | Core developers |
| **[� API Reference](docs/API-REFERENCE.md)** | Complete API documentation | All developers |
| **[📊 Generic Sheet Manager](docs/GENERIC-SHEET-MANAGER.md)** | Schema-less operations | Flexible use cases |

### Specialized Package Documentation
| Package | Documentation |
|---------|---------------|
| **[� Gig Package](docs/GIG.md)** | Complete gig work tracking guide |

## 🔐 Authentication Quick Start

RaptorSheets supports multiple authentication methods:

### Service Account (Recommended)
```csharp
var credentials = new Dictionary<string, string>
{
    ["type"] = "service_account",
    ["private_key_id"] = "your-key-id",
    ["private_key"] = "your-private-key", 
    ["client_email"] = "service@project.iam.gserviceaccount.com",
    ["client_id"] = "your-client-id"
};
```

### OAuth2 Access Token
```csharp
var manager = new GoogleSheetManager(accessToken, spreadsheetId);
```

**📖 [Complete Authentication Guide](docs/AUTHENTICATION.md)**

## 🏗️ Building Custom Packages

RaptorSheets.Core is designed to be the foundation for domain-specific packages. Here's how to create your own:

```csharp
// 1. Define your domain entities
public class CustomEntity
{
    public string Name { get; set; }
    public decimal Value { get; set; }
    public DateTime Date { get; set; }
}

// 2. Create a domain-specific manager
public class CustomManager
{
    private readonly GoogleSheetService _service;
    
    public CustomManager(Dictionary<string, string> credentials, string spreadsheetId)
    {
        _service = new GoogleSheetService(credentials, spreadsheetId);
    }
    
    public async Task<List<CustomEntity>> GetCustomData()
    {
        var data = await _service.GetSheetData("CustomSheet");
        return CustomMapper.MapFromRangeData(data.Values);
    }
    
    public async Task AddCustomData(List<CustomEntity> entities)
    {
        var rangeData = CustomMapper.MapToRangeData(entities);
        var valueRange = new ValueRange { Values = rangeData };
        await _service.UpdateData(valueRange, "CustomSheet!A:Z");
    }
}

// 3. Build specialized functionality on top of Core's foundation
```

**See [RaptorSheets.Gig](RaptorSheets.Gig/README.md) as a complete example of a specialized package built on Core.**

## 🛠️ Development Setup

### Prerequisites
- .NET 8.0 SDK or later
- Google Cloud Project with Sheets API enabled
- Service Account credentials (recommended) or OAuth2 setup

### Quick Setup
```bash
git clone https://github.com/khanjal/RaptorSheets.git
cd RaptorSheets
dotnet restore
dotnet build
dotnet test
```

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run Core library tests specifically  
dotnet test RaptorSheets.Core.Tests/

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## 🚦 Performance & API Limits

### Google Sheets API Quotas
- **Read/Write requests**: 100 requests per 100 seconds per user
- **Daily requests**: 50,000 requests per day

### Library Optimizations
- 📦 Automatic request batching
- ⚡ Efficient data retrieval strategies
- 🧠 Smart caching mechanisms
- 🔁 Rate limit handling with retries

## 🤝 Contributing

We welcome contributions to RaptorSheets.Core and the broader ecosystem!

### Development Workflow
1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Focus on Core library enhancements or create new specialized packages
4. Write comprehensive tests
5. Update relevant documentation
6. Submit a Pull Request

### Areas for Contribution
- **Core Library**: Enhance base functionality, performance, or new Google Sheets features
- **New Packages**: Create domain-specific packages (Stock, Real Estate, etc.)
- **Documentation**: Improve guides, examples, and API documentation
- **Testing**: Add test coverage or performance benchmarks

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 💬 Support & Resources

- 🐞 [Report Issues](https://github.com/khanjal/RaptorSheets/issues)
- 💬 [Discussions](https://github.com/khanjal/RaptorSheets/discussions)
- 📖 [Google Sheets API Reference](https://googleapis.dev/dotnet/Google.Apis.Sheets.v4/latest/api/Google.Apis.Sheets.v4.html)
- 🌐 [Project Homepage](https://www.raptorsheets.com)

---

**Made with ❤️ by Iron Raptor Digital**