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

## 🧭 Project Conventions
- Shared-first: place reusable entities, helpers, enums, request/response models, and formatting options in Core; keep domain-specific managers/mappers/config in domain projects.
- Constants as single source: use sheet/header/format names from SheetsConfig (or domain constants when truly domain-only); avoid hard-coded strings.
- Entity-driven config: generate headers with `EntitySheetConfigHelper.GenerateHeadersFromEntity<T>()` and call `sheet.Headers.UpdateColumns()` before applying formulas/formatting.
- Request builders: extend `RaptorSheets.Core/Helpers/GoogleRequestHelpers.cs` for new Sheets API operations so all domains share the same builders.
- Formatting: use the shared `FormattingOptionsEntity` (Core) for formatting/metadata operations; orchestration stays in domain managers.
- Ordering: default to declaration order; use `ColumnOrder`/`SheetOrder` only when necessary; keep tab order in explicit arrays and validate in tests.
- Testing: default to deterministic unit tests under `*.Tests/Unit`; seed randomness, avoid time/network flakiness, and prefer validation helpers.

## 📦 Installation

```bash
# Core library for custom implementations
dotnet add package RaptorSheets.Core

# Or choose a specialized package
dotnet add package RaptorSheets.Gig    # For gig work tracking
```

## 🚀 Quick Start

### Basic Google Sheets Operations
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
```

### TypedField System (Header + Metadata Attributes)
```csharp
using RaptorSheets.Core.Attributes;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Repositories;

// Define entities using Header and metadata attributes
public class ContactEntity
{
    public int RowId { get; set; }

    // Basic string field with header mapping
    [Header(SheetsConfig.HeaderNames.Name)]
    [Input]
    public string Name { get; set; } = "";

    // Automatic currency formatting
    [Header(SheetsConfig.HeaderNames.Salary)]
    [Input]
    [Format(FormatEnum.CURRENCY)]
    public decimal? Salary { get; set; }

    // Custom format when different from default
    [Header(SheetsConfig.HeaderNames.Score)]
    [Input]
    [Format(FormatEnum.PERCENT, pattern: "0.0%")]
    public decimal? Score { get; set; }

    // Override JSON name when needed
    [Header(SheetsConfig.HeaderNames.EmailAddress)]
    [Input]
    [JsonPropertyName("email")]
    public string EmailAddress { get; set; } = "";
}

// Repository with automatic CRUD operations (unchanged)
public class ContactRepository : BaseEntityRepository<ContactEntity>
{
    public ContactRepository(IGoogleSheetService sheetService) 
        : base(sheetService, "Contacts", hasHeaderRow: true) { }
}

// Usage
var repository = new ContactRepository(service);
var contacts = await repository.GetAllAsync();
```

### Create Advanced Sheets with Formatting
```csharp
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
```

## ✨ Key Features

### TypedField System
- **Header + Metadata Attributes**: Use `HeaderAttribute` plus `Input`, `Format`, `Validation`, and `Note` to express column intent. Internally these are merged into runtime `ColumnAttribute` objects for processing.
- **Automatic Type Conversion**: Currency, dates, percentages, phone numbers, emails
- **Default Format Patterns**: Specify only when different from sensible defaults using `Format` attribute
- **Header-Driven Configuration**: Use header names as primary source of truth
- **Repository Pattern**: Automatic CRUD operations with type conversion

### Core Infrastructure
- **📋 Header Management**: Extension methods for column and index assignments with automatic processing
- **🎨 Column Formatting**: Apply data formatting, configure drop-downs, and set cell protection
- **🎯 Sheet Styling**: Alternating row colors, full sheet protection, and custom tab colors
- **⚡ Batch Operations**: Efficient bulk operations for large datasets with automatic batching
- **🔒 Type Safety**: Strongly typed entities and enums for all operations
- **✅ Auto Validation**: Automatic header validation with detailed error reporting
- **🛠️ Error Handling**: Comprehensive message system for operation feedback
- **🧪 Well Tested**: Extensive unit and integration test coverage

## 🏗️ Architecture

```
Your Custom Application
       ↓
TypedField System
    ├── HeaderAttribute + Input/Format/Validation/Note (Configuration)
  ├── BaseEntityRepository<T> (CRUD Operations)
  ├── TypedEntityMapper<T> (Conversion)
  └── Schema Validation (Type Safety)
       ↓
RaptorSheets.Core
  ├── GoogleSheetService (High-level operations)
  ├── SheetServiceWrapper (API abstraction)  
  ├── Models & Entities (Type safety)
  └── Extensions & Helpers (Utilities)
       ↓
Google Sheets API v4
```

## 💼 TypedField System Benefits

### Simplified Configuration
```csharp
// Use Header + metadata attributes for clearer intent
[Header(SheetsConfig.HeaderNames.Pay)]
[Input]
[Format(FormatEnum.ACCOUNTING)]
public decimal? Pay { get; set; }

// Automated conversion in mappers
entity.Pay = MapperHelper.MapField<decimal?>("Pay", row, headers);
```

### Supported Field Types with Auto-Conversion

| Field Type | Auto-Converts | Example Input → Output |
|------------|---------------|------------------------|
| `Currency` | Dollar amounts | `"$1,234.56"` → `decimal 1234.56` |
| `PhoneNumber` | Phone formats | `"(555) 123-4567"` → `long 5551234567` |
| `DateTime` | Date/time values | Google serial → `DateTime` |
| `Percentage` | Percentage values | `0.85` → `"85.00%"` |
| `Email` | Email addresses | Validation + text format |
| `Number` | Numeric values | `"1,234.56"` → `decimal 1234.56` |
| `Integer` | Whole numbers | `"1,234"` → `int 1234` |
| `Boolean` | True/false | `"TRUE"` → `bool true` |

## 📚 Specialized Packages

Built on RaptorSheets.Core, these packages provide domain-specific functionality:

| Package | Version | Purpose | Documentation |
|---------|---------|---------|---------------|
| **[RaptorSheets.Gig](https://www.nuget.org/packages/RaptorSheets.Gig/)** | [![Nuget](https://img.shields.io/nuget/v/RaptorSheets.Gig)](https://www.nuget.org/packages/RaptorSheets.Gig/) | Complete gig work tracking with automated analytics | **[📖 Gig Guide](RaptorSheets.Gig/README.md)** |

> **Looking for gig work tracking?** Check out **[RaptorSheets.Gig](RaptorSheets.Gig/README.md)** - a complete solution for freelancers and gig workers with pre-built sheets for trips, shifts, earnings, and comprehensive analytics.

## 💼 Use Cases

- **Custom Business Solutions**: Build domain-specific Google Sheets integrations for any industry
- **Data Pipeline Integration**: Automate data sync between your applications and collaborative spreadsheets  
- **Advanced Report Generation**: Create complex reports with formulas, cross-sheet references, and automated calculations
- **Workflow Automation**: Streamline business processes that rely on Google Sheets data
- **Foundation for Specialized Packages**: Use as a base to create domain-specific managers (like RaptorSheets.Gig)

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

## 🏗️ Building Custom Packages

RaptorSheets.Core with TypedField system is designed to be the foundation for domain-specific packages:

```csharp
// 1. Define your domain entities using Header + metadata attributes
public class ProductEntity
{
    public int RowId { get; set; }
    
    [Header(SheetsConfig.HeaderNames.ProductName)]
    [Input]
    public string Name { get; set; } = "";
    
    [Header(SheetsConfig.HeaderNames.Price)]
    [Input]
    [Format(FormatEnum.CURRENCY)]
    public decimal Price { get; set; }
    
    [Header(SheetsConfig.HeaderNames.LaunchDate)]
    [Format(FormatEnum.DATE)]
    public DateTime? LaunchDate { get; set; }
}

// 2. Create repository with automatic CRUD
public class ProductRepository : BaseEntityRepository<ProductEntity>
{
    public ProductRepository(IGoogleSheetService service) 
        : base(service, "Products", hasHeaderRow: true) { }
    
    public async Task<List<ProductEntity>> GetExpensiveProductsAsync()
    {
        var products = await GetAllAsync(); // Automatic conversion
        return products.Where(p => p.Price > 100m).ToList();
    }
}

// 3. Domain-specific manager
public class ProductManager
{
    private readonly ProductRepository _repository;
    
    public ProductManager(Dictionary<string, string> credentials, string spreadsheetId)
    {
        var service = new GoogleSheetService(credentials, spreadsheetId);
        _repository = new ProductRepository(service);
    }
    
    public async Task<List<ProductEntity>> GetProductCatalogAsync()
    {
        return await _repository.GetAllAsync(); // Full type conversion automatically
    }
}
```

**See [RaptorSheets.Gig](RaptorSheets.Gig/README.md) as a complete example of a specialized package built on the TypedField system.**

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
- 🆕 **TypedField Performance**: Cached reflection, efficient type conversion

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
- **TypedField System**: Field types, conversion improvements, validation enhancements
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