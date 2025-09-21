
# RaptorSheets.Core

| Badge Name | Status | Site |
| ---------- | :------------: | :------------: |
| Latest Build Status | [![build status](https://github.com/khanjal/RaptorSheets/actions/workflows/dotnet.yml/badge.svg)](https://github.com/khanjal/RaptorSheets/actions) | [GitHub Repo](https://github.com/khanjal/RaptorSheets/) |
| RaptorSheets.Core NuGet | [![Nuget](https://img.shields.io/nuget/v/RaptorSheets.Core)](https://www.nuget.org/packages/RaptorSheets.Core/) | [Raptor Sheets - Core](https://www.raptorsheets.com) |
| Test Coverage | [![Coverage](https://sonarcloud.io/api/project_badges/measure?project=khanjal_RaptorSheets&metric=coverage)](https://sonarcloud.io/summary/new_code?id=khanjal_RaptorSheets) | [SonarCloud](https://sonarcloud.io/project/overview?id=khanjal_RaptorSheets) |
| Code Quality | [![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=khanjal_RaptorSheets&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=khanjal_RaptorSheets) | [SonarCloud](https://sonarcloud.io/project/overview?id=khanjal_RaptorSheets) |
| License | [![License](https://img.shields.io/github/license/khanjal/RaptorSheets)](LICENSE) | - |

## 🚀 Quick Start

```bash
# Install the core package
dotnet add package RaptorSheets.Core
```



## 📚 Project Description

RaptorSheets is a comprehensive .NET 8 library suite that simplifies interactions between custom API services and the Google Sheets API. Built for developers who need powerful spreadsheet integration without the complexity, featuring extensive test coverage and production-ready reliability.

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
Your Application
    ↓
Package-Specific Manager (Gig)
       ↓
RaptorSheets.Core (GoogleSheetService)
       ↓
SheetServiceWrapper (API abstraction)
       ↓
Google Sheets API v4
```

### 💼 Use Cases

- **Business Operations**: Handle addresses, contacts, regions, and services
- **Data Analytics**: Generate daily, weekly, monthly, and yearly reports
- **Custom Integrations**: Build your own sheet types using the Core library

## 📖 Documentation

Choose the documentation that matches your needs:

| Documentation | Purpose | Audience |
|---------------|---------|----------|
| **[📚 Complete Guide](DOCUMENTATION.md)** | Comprehensive overview and getting started | All users |
| **[🔧 Core Library](docs/CORE.md)** | Core functionality and custom implementations | Library developers |
| [RaptorSheets.Gig Package](https://www.nuget.org/packages/RaptorSheets.Gig/) | Gig work and freelance tracking | Gig workers, freelancers |
| **[ Authentication](docs/AUTHENTICATION.md)** | Setup guide for Google APIs | All users |

## 📦 Available Packages

| Package | Version | Purpose | Dependencies | Documentation |
|---------|---------|---------|--------------|---------------|
| **RaptorSheets.Core** | ![NuGet](https://img.shields.io/nuget/v/RaptorSheets.Core) | Core functionality for custom implementations | Google.Apis.Sheets.v4 | [🔧 Core Docs](docs/CORE.md) |
| **RaptorSheets.Gig** | ![NuGet](https://img.shields.io/nuget/v/RaptorSheets.Gig) | Gig work and freelance tracking | Google.Apis.Sheets.v4, Google.Apis.Drive.v3 | [💼 Gig Docs](docs/GIG.md) |
| **RaptorSheets.Common** | - | Shared utilities (included in packages) | - | - |

## 🔐 Authentication

RaptorSheets supports multiple authentication methods. Here's a quick example:

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

var manager = new GoogleSheetManager(credentials, spreadsheetId);
```

**🔐 [Complete Authentication Guide](docs/AUTHENTICATION.md)**

## 💡 Usage Example

```csharp
using RaptorSheets.Core.Services;
using RaptorSheets.Core.Models.Google;

var service = new GoogleSheetService(credentials, spreadsheetId);

// Create custom sheet structure
var sheetModel = new SheetModel
{
    Name = "CustomSheet",
    Headers = new List<SheetCellModel>
    {
        new() { Name = "ID", Format = FormatEnum.NUMBER },
        new() { Name = "Description", Format = FormatEnum.TEXT }
    }
};

// Generate and execute requests (see Core docs for details)
```

---

## Looking for Gig Work Tracking?

RaptorSheets.Gig is a package built on top of Core for gig work and freelance tracking. [Learn more &rarr;](https://www.nuget.org/packages/RaptorSheets.Gig/)

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

**🛠️ [Complete Development Guide](DOCUMENTATION.md#development-setup)**

## 🧪 Testing

The library includes comprehensive test coverage across all packages:

```bash
# Run all tests
dotnet test

# Run package-specific tests
dotnet test RaptorSheets.Core.Tests/
dotnet test RaptorSheets.Gig.Tests/

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

**Test Coverage Areas:**
- 🏗️ Core functionality and services
- 📦 Package-specific implementations  
- 🔐 Authentication methods
- ⚠️ Error handling and validation
- 🧩 Extension methods and utilities
- 🔗 Real Google Sheets API integration

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

We welcome contributions to any package in the RaptorSheets suite!

### Development Workflow
1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Choose your focus area:
   - **Core Library**: Enhance base functionality
   - **Gig Package**: Add gig work features
   - **New Package**: Create a new domain-specific package
4. Write comprehensive tests
5. Update relevant documentation
6. Ensure all tests pass (`dotnet test`)
7. Submit a Pull Request

### Code Standards
- Follow existing patterns within each package
- Maintain backward compatibility for Core library
- Add package-specific tests for new features
- Update package-specific documentation
- Use appropriate XML documentation

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 💬 Support & Resources

### Documentation
- 📚 [Complete Guide](DOCUMENTATION.md) - Overview and getting started
- 🛠️ [Core Library](docs/CORE.md) - Core functionality reference  
- 💼 [Gig Package](docs/GIG.md) - Gig work tracking guide
- 🔐 [Authentication](docs/AUTHENTICATION.md) - Setup instructions

### Community & Support
- 🐞 [Report Issues](https://github.com/khanjal/RaptorSheets/issues) - Bug reports and feature requests
- 💬 [Discussions](https://github.com/khanjal/RaptorSheets/discussions) - Community support and questions
- 📖 [Google Sheets API Reference](https://googleapis.dev/dotnet/Google.Apis.Sheets.v4/latest/api/Google.Apis.Sheets.v4.html) - Official API documentation
- 🌐 [Project Homepage](https://gig.raptorsheets.com) - Additional resources and examples

## 🗺️ Roadmap

### Core Library
- 📦 Independent NuGet package release
- 🔐 Enhanced authentication flows
- 🧩 Plugin architecture for custom packages

### Package Ecosystem
- 📊 Advanced analytics across all packages
- 🌍 Multi-language localization support
- 📱 Mobile-optimized implementations
- 🏢 Enterprise features and compliance

### New Packages
- 💸 Business expense tracking
- 📅 Project management and time tracking
- 🏠 Real estate portfolio management
- 📦 Inventory management systems

---

**Made with ❤️ by Iron Raptor Digital**

## API Documentation

For details on the underlying Google Sheets API concepts used in this project, see the official documentation:

- [Google Sheets API Concepts Guide](https://developers.google.com/workspace/sheets/api/guides/concepts)
