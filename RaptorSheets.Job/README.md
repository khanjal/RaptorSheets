# RaptorSheets.Job

[![Nuget](https://img.shields.io/nuget/v/RaptorSheets.Job)](https://www.nuget.org/packages/RaptorSheets.Job/) [![Build Status](https://github.com/khanjal/RaptorSheets/actions/workflows/dotnet.yml/badge.svg)](https://github.com/khanjal/RaptorSheets/actions)

## Overview

RaptorSheets.Job is a specialized implementation of RaptorSheets.Core designed for job application tracking. It provides pre-configured sheet types, entities, and workflows optimized for managing job applications, interviews, companies, and analytics for job seekers.

## Table of Contents
1. [Quick Start](#quick-start)
2. [Demo Setup](#demo-setup)
3. [Sheet Types](#sheet-types)
4. [Entities](#entities)
5. [Manager Usage](#manager-usage)
6. [Data Operations](#data-operations)
7. [Advanced Features](#advanced-features)
8. [Examples](#examples)

## Quick Start

### Installation
```bash
dotnet add package RaptorSheets.Job
```

### Basic Setup
```csharp
using RaptorSheets.Job.Managers;

// Initialize manager
var manager = new GoogleSheetManager(credentials, spreadsheetId);

// Create all job tracking sheets
await manager.CreateAllSheets();

// Get your data
var data = await manager.GetSheets();
```

## Demo Setup

Create a demo spreadsheet with realistic sample data to explore RaptorSheets.Job capabilities or set up test environments.

### Option 1: Create a Complete Demo Spreadsheet

Perfect for new users or demos. Creates all sheets and populates with realistic job application data:

```csharp
var manager = new GoogleSheetManager(credentials, spreadsheetId);

// Creates all sheets and adds sample data
var result = await manager.SetupDemo();

Console.WriteLine("? Demo spreadsheet ready!");
```

### Option 2: Populate an Existing Spreadsheet

If you already have sheets created, just add sample data:

```csharp
// Populate existing sheets with demo data
var result = await manager.PopulateDemoData();

Console.WriteLine($"Added demo data: {result.Applications.Count} applications, {result.Interviews.Count} interviews");
```

### Custom Date Ranges

Specify custom date ranges for your demo data:

```csharp
// Last 90 days
await manager.SetupDemo(
    startDate: DateTime.Today.AddDays(-90),
    endDate: DateTime.Today
);

// Specific job search period
await manager.SetupDemo(
    startDate: new DateTime(2024, 1, 1),
    endDate: new DateTime(2024, 6, 30)
);

// Full year
await manager.PopulateDemoData(
    startDate: new DateTime(2024, 1, 1),
    endDate: new DateTime(2024, 12, 31)
);
```

### What Demo Data Includes

The demo system generates realistic job search data:

**Applications** - 2-5 per week
- Companies: Mix of tech, finance, healthcare, retail, and other industries
- Job Sites: LinkedIn, Indeed, Glassdoor, ZipRecruiter, Company Website
- Positions: Engineering, Design, Marketing, Sales, Product Management, etc.
- Salary ranges based on position level
- Decision outcomes: Pending, Accepted, Rejected, Withdrew
- Realistic timelines and response rates

**Interviews** - 30-40% of applications lead to interviews
- Interview Types: Phone Screen, Video Call, On-Site, Technical, Panel
- Multiple interview rounds for progressing applications
- Realistic scheduling (1-2 weeks after application)
- Attendees with roles (Hiring Manager, Team Lead, HR, etc.)
- Interview outcomes and notes

**Companies** - Automatically extracted from applications
- Industry categorization
- Location information
- Notes about company culture

**Positions** - Automatically extracted from applications
- Job title normalization
- Seniority levels (Entry, Mid, Senior, Lead, Manager)
- Categories (Engineering, Design, Marketing, etc.)

### Complete Demo Creation Example

```csharp
using Google.Apis.Sheets.v4;
using RaptorSheets.Job.Managers;

public async Task CreateDemoSpreadsheet()
{
    // 1. Create a new Google Spreadsheet
    var sheetsService = new SheetsService(/* credentials */);
    var spreadsheet = await sheetsService.Spreadsheets.Create(new Spreadsheet
    {
        Properties = new SpreadsheetProperties { Title = "Job Search Tracker" }
    }).ExecuteAsync();

    var spreadsheetId = spreadsheet.SpreadsheetId;

    // 2. Set up demo with sample data
    var manager = new GoogleSheetManager(credentials, spreadsheetId);
    var result = await manager.SetupDemo();

    // 3. View your demo
    Console.WriteLine($"? Demo ready at: https://docs.google.com/spreadsheets/d/{spreadsheetId}");
}
```

### Demo Troubleshooting

**"Unable to save data" Error**
- Ensure spreadsheet exists and has write permissions
- Use `SetupDemo()` for new spreadsheets (creates sheets first)
- Use `PopulateDemoData()` only when sheets already exist

**No Data Appearing**
- Verify date range is valid (startDate < endDate)
- Check Google Sheets API permissions

## Sheet Types

### Input Sheets

#### Applications
Primary tracking sheet for job applications. Fields include:
- **Date** - Application submission date
- **Company** - Company/organization name (validated against Companies sheet, allows input)
- **Job Title** - Position title (validated against Positions sheet, allows input)
- **Posting** - URL to job posting
- **Site** - Job board (LinkedIn, Indeed, Glassdoor, ZipRecruiter, Company)
- **Key** - Unique identifier: `Company-JobTitle-Number` (e.g., "Microsoft-Software Engineer-1")
- **Interview Count** - Calculated count of interviews (linked by Key)
- **Decision** - Application outcome (Pending, Accepted, Rejected, Withdrew)
- **Decision Date** - Date of final decision
- **Days Active** - Calculated: Decision Date - Application Date
- **Notes** - Free text for additional information
- **Pay Low** - Minimum salary/compensation
- **Pay High** - Maximum salary/compensation
- **Pay Avg** - Calculated average: (Pay Low + Pay High) / 2
- **Location** - Work location (Remote, Hybrid, or specific city)
- **Schedule** - Work schedule (Full-time, Part-time, Contract)

#### Interviews
Detailed interview tracking linked to applications. Fields include:
- **Date** - Interview date
- **Start Time** - Scheduled start time
- **End Time** - Scheduled end time
- **Duration** - Actual interview length (can be manually entered if different from scheduled)
- **Company** - Company name (dropdown from Companies sheet)
- **Job Title** - Position title (dropdown from Positions sheet)
- **Key** - Links to Application: `Company-JobTitle-Number`
- **Interview Type** - Format (Phone Screen, Video Call, On-Site, Technical, Panel)
- **Interview Round** - Calculated based on Key (Round 1, Round 2, etc.)
- **Recruiter Name** - Recruiter or coordinator name
- **Recruiter Contact** - Email or phone
- **Attendees** - Comma-separated list in format "Name (Position)"
- **Outcome** - Interview result (Passed, Failed, Pending, Awaiting Feedback)
- **Notes** - Interview feedback and observations

#### CompanyDetails (Optional)
Input sheet for additional company information:
- **Company** - Company name
- **Industry** - Industry/sector
- **Location** - Headquarters location
- **Website** - Company website
- **Notes** - Company culture, benefits, etc.

#### PositionDetails (Optional)
Input sheet for additional position information:
- **Position** - Job title
- **Category** - Job category (Engineering, Marketing, Sales, etc.)
- **Seniority** - Level (Entry, Mid, Senior, Lead, Manager)
- **Notes** - Common requirements, skills, etc.

### Reference Sheets (Calculated)

These sheets are automatically populated from input data:

#### Companies
Unique companies extracted from Applications, with optional enrichment from CompanyDetails:
- **Company** - Unique company name
- **Application Count** - Number of applications
- **Interview Count** - Total interviews
- **Industry** - From CompanyDetails (if provided)
- **Location** - From CompanyDetails (if provided)

#### Positions
Unique job titles extracted from Applications, with optional enrichment from PositionDetails:
- **Position** - Unique job title
- **Application Count** - Number of applications
- **Category** - From PositionDetails (if provided)
- **Seniority** - From PositionDetails (if provided)

#### Sites
Job boards used for applications:
- LinkedIn
- Indeed
- Glassdoor
- ZipRecruiter
- Company

#### Decisions
Application outcome options:
- Pending
- Accepted
- Rejected
- Withdrew

#### InterviewTypes
Interview format options:
- Phone Screen
- Video Call
- On-Site
- Technical
- Panel

#### InterviewOutcomes
Interview result options:
- Passed
- Failed
- Pending
- Awaiting Feedback

### Analytics Sheets

#### Weekly
Weekly application and interview metrics:
- **Week** - Week starting date
- **Applications** - Number of applications submitted
- **Interviews** - Number of interviews conducted
- **Interview Rate** - Percentage of applications leading to interviews
- **Decisions** - Number of decisions received
- **Response Rate** - Percentage of applications with decisions

#### Monthly
Monthly application and interview metrics:
- **Month** - Month and year
- **Applications** - Number of applications submitted
- **Interviews** - Number of interviews conducted
- **Interview Rate** - Percentage of applications leading to interviews
- **Decisions** - Number of decisions received
- **Response Rate** - Percentage of applications with decisions
- **Accepted** - Number of offers accepted
- **Rejected** - Number of rejections

#### Summary
Overall job search statistics:
- **Total Applications** - Total number of applications
- **Total Interviews** - Total number of interviews
- **Interview Rate** - Overall percentage of applications leading to interviews
- **Avg Days to Decision** - Average time from application to decision
- **Avg Days to Interview** - Average time from application to first interview
- **Response Rate** - Percentage of applications with decisions
- **Acceptance Rate** - Percentage of applications accepted
- **Top Sites** - Most effective job boards
- **Top Companies** - Companies with most applications

## Entities

### ApplicationEntity
Represents a single job application with validation and formatting.

```csharp
public class ApplicationEntity
{
    public int RowId { get; set; }
    public string Date { get; set; }           // DATE format
    public string Company { get; set; }        // Validated dropdown
    public string JobTitle { get; set; }       // Validated dropdown
    public string Posting { get; set; }        // URL
    public string Site { get; set; }           // Validated dropdown
    public string Key { get; set; }            // Formula: Company-JobTitle-Number
    public int InterviewCount { get; set; }    // Formula: COUNTIF from Interviews
    public string Decision { get; set; }       // Validated dropdown
    public string DecisionDate { get; set; }   // DATE format
    public int? DaysActive { get; set; }       // Formula: DecisionDate - Date
    public string Notes { get; set; }
    public decimal? PayLow { get; set; }       // Currency format
    public decimal? PayHigh { get; set; }      // Currency format
    public decimal? PayAvg { get; set; }       // Formula: (PayLow + PayHigh) / 2
    public string Location { get; set; }
    public string Schedule { get; set; }       // Validated dropdown
}
```

### InterviewEntity
Represents a single interview with detailed tracking.

```csharp
public class InterviewEntity
{
    public int RowId { get; set; }
    public string Date { get; set; }           // DATE format
    public string StartTime { get; set; }      // TIME format
    public string EndTime { get; set; }        // TIME format
    public string Duration { get; set; }       // DURATION format or manual entry
    public string Company { get; set; }        // Validated dropdown
    public string JobTitle { get; set; }       // Validated dropdown
    public string Key { get; set; }            // Links to Application
    public string InterviewType { get; set; }  // Validated dropdown
    public int InterviewRound { get; set; }    // Formula: COUNTIF for this Key
    public string RecruiterName { get; set; }
    public string RecruiterContact { get; set; }
    public string Attendees { get; set; }      // Format: "Name (Position), Name (Position)"
    public string Outcome { get; set; }        // Validated dropdown
    public string Notes { get; set; }
}
```

### SheetEntity
Container for all sheet data:

```csharp
public class SheetEntity
{
    public List<ApplicationEntity> Applications { get; set; }
    public List<InterviewEntity> Interviews { get; set; }
    public List<CompanyEntity> Companies { get; set; }
    public List<PositionEntity> Positions { get; set; }
    public List<WeeklyEntity> Weekly { get; set; }
    public List<MonthlyEntity> Monthly { get; set; }
    public List<SummaryEntity> Summary { get; set; }
    // ... reference sheets
}
```

## Manager Usage

### Creating Sheets

```csharp
// Create all sheets at once
await manager.CreateAllSheets();

// Create specific sheets
await manager.CreateSheet(SheetEnum.APPLICATIONS);
await manager.CreateSheet(SheetEnum.INTERVIEWS);
```

### Reading Data

```csharp
// Get all data
var allData = await manager.GetSheets();

// Get specific sheet data
var applications = await manager.GetApplications();
var interviews = await manager.GetInterviews();
var companies = await manager.GetCompanies();
```

### Writing Data

```csharp
// Add single application
var application = new ApplicationEntity
{
    Date = DateTime.Today.ToString("yyyy-MM-dd"),
    Company = "Microsoft",
    JobTitle = "Software Engineer",
    Site = "LinkedIn",
    Posting = "https://linkedin.com/jobs/123456",
    PayLow = 120000,
    PayHigh = 160000,
    Location = "Remote",
    Schedule = "Full-time",
    Decision = "Pending"
};
await manager.AddApplication(application);

// Add single interview
var interview = new InterviewEntity
{
    Date = DateTime.Today.AddDays(7).ToString("yyyy-MM-dd"),
    StartTime = "10:00 AM",
    EndTime = "11:00 AM",
    Company = "Microsoft",
    JobTitle = "Software Engineer",
    InterviewType = "Phone Screen",
    RecruiterName = "Jane Smith",
    RecruiterContact = "jane.smith@microsoft.com",
    Outcome = "Pending"
};
await manager.AddInterview(interview);

// Bulk add applications
var applications = new List<ApplicationEntity> { /* ... */ };
await manager.AddApplications(applications);
```

### Updating Data

```csharp
// Update application
application.Decision = "Accepted";
application.DecisionDate = DateTime.Today.ToString("yyyy-MM-dd");
await manager.UpdateApplication(application);

// Update interview outcome
interview.Outcome = "Passed";
interview.Notes = "Great conversation, moving to next round";
await manager.UpdateInterview(interview);
```

## Data Operations

### Key Generation

Application keys follow the pattern: `Company-JobTitle-Number`

The number is auto-incremented for multiple applications to the same company/position:
- First application: `Microsoft-Software Engineer-0`
- Second application: `Microsoft-Software Engineer-1`
- Third application: `Microsoft-Software Engineer-2`

This key is used to link interviews to applications.

### Calculated Fields

#### Applications Sheet
- **Key** - Formula generates unique identifier
- **Interview Count** - COUNTIF formula counts matching interviews
- **Days Active** - Calculates days from application to decision
- **Pay Avg** - Averages low and high salary

#### Interviews Sheet
- **Interview Round** - COUNTIF calculates round number for each Key
- **Duration** - Can be calculated from Start/End time or manually entered

### Data Validation

The following fields use dropdown validation from reference sheets:
- **Applications**: Company, JobTitle, Site, Decision, Schedule
- **Interviews**: Company, JobTitle, InterviewType, Outcome

This ensures data consistency and enables powerful filtering and analytics.

### Parsing Attendees

The `Attendees` field uses the format: `Name (Position), Name (Position)`

Example: `John Doe (Hiring Manager), Jane Smith (Team Lead), Bob Johnson (HR)`

Front-end applications can parse this format to extract individual attendees and their roles.

## Advanced Features

### Analytics and Reporting

```csharp
// Get weekly metrics
var weekly = await manager.GetWeeklyMetrics();
foreach (var week in weekly)
{
    Console.WriteLine($"Week of {week.Week}: {week.Applications} applications, {week.InterviewRate}% interview rate");
}

// Get monthly metrics
var monthly = await manager.GetMonthlyMetrics();

// Get overall summary
var summary = await manager.GetSummary();
Console.WriteLine($"Total Applications: {summary.TotalApplications}");
Console.WriteLine($"Interview Rate: {summary.InterviewRate}%");
Console.WriteLine($"Avg Days to Interview: {summary.AvgDaysToInterview}");
```

### Filtering and Searching

```csharp
// Filter applications by company
var microsoftApps = applications.Where(a => a.Company == "Microsoft").ToList();

// Filter by date range
var recentApps = applications.Where(a => 
    DateTime.Parse(a.Date) >= DateTime.Today.AddDays(-30)).ToList();

// Filter by decision
var pendingApps = applications.Where(a => a.Decision == "Pending").ToList();

// Filter interviews by type
var phoneScreens = interviews.Where(i => i.InterviewType == "Phone Screen").ToList();
```

### Exporting Data

```csharp
// Export to JSON
var json = JsonSerializer.Serialize(allData, new JsonSerializerOptions 
{ 
    WriteIndented = true 
});
File.WriteAllText("job-search-data.json", json);

// Export specific sheet
var applicationsJson = JsonSerializer.Serialize(applications);
```

## Examples

### Complete Job Application Workflow

```csharp
// 1. Submit application
var application = new ApplicationEntity
{
    Date = DateTime.Today.ToString("yyyy-MM-dd"),
    Company = "Google",
    JobTitle = "Senior Software Engineer",
    Site = "LinkedIn",
    Posting = "https://linkedin.com/jobs/789012",
    PayLow = 150000,
    PayHigh = 200000,
    Location = "Mountain View, CA",
    Schedule = "Full-time",
    Decision = "Pending"
};
await manager.AddApplication(application);

// 2. Schedule phone screen
var phoneScreen = new InterviewEntity
{
    Date = DateTime.Today.AddDays(5).ToString("yyyy-MM-dd"),
    StartTime = "2:00 PM",
    EndTime = "2:45 PM",
    Company = "Google",
    JobTitle = "Senior Software Engineer",
    InterviewType = "Phone Screen",
    RecruiterName = "Sarah Johnson",
    RecruiterContact = "sarah@google.com",
    Attendees = "Sarah Johnson (Recruiter)",
    Outcome = "Pending"
};
await manager.AddInterview(phoneScreen);

// 3. After phone screen, update outcome
phoneScreen.Outcome = "Passed";
phoneScreen.Notes = "Discussed experience, moving to technical interview";
await manager.UpdateInterview(phoneScreen);

// 4. Schedule technical interview
var technical = new InterviewEntity
{
    Date = DateTime.Today.AddDays(12).ToString("yyyy-MM-dd"),
    StartTime = "10:00 AM",
    EndTime = "12:00 PM",
    Company = "Google",
    JobTitle = "Senior Software Engineer",
    InterviewType = "Technical",
    RecruiterContact = "sarah@google.com",
    Attendees = "Mike Chen (Senior Engineer), Lisa Park (Tech Lead)",
    Outcome = "Pending"
};
await manager.AddInterview(technical);

// 5. After process completes, update decision
application.Decision = "Accepted";
application.DecisionDate = DateTime.Today.AddDays(20).ToString("yyyy-MM-dd");
await manager.UpdateApplication(application);
```

### Bulk Application Import

```csharp
// Import from CSV or external source
var applications = new List<ApplicationEntity>();

foreach (var row in csvData)
{
    applications.Add(new ApplicationEntity
    {
        Date = row["Date"],
        Company = row["Company"],
        JobTitle = row["Job Title"],
        Site = row["Site"],
        Posting = row["URL"],
        PayLow = decimal.Parse(row["Min Salary"]),
        PayHigh = decimal.Parse(row["Max Salary"]),
        Location = row["Location"],
        Schedule = row["Type"],
        Decision = "Pending"
    });
}

await manager.AddApplications(applications);
```

### Weekly Job Search Report

```csharp
var startDate = DateTime.Today.AddDays(-7);
var endDate = DateTime.Today;

var recentApplications = applications
    .Where(a => DateTime.Parse(a.Date) >= startDate && DateTime.Parse(a.Date) <= endDate)
    .ToList();

var recentInterviews = interviews
    .Where(i => DateTime.Parse(i.Date) >= startDate && DateTime.Parse(i.Date) <= endDate)
    .ToList();

Console.WriteLine($"=== Weekly Job Search Report ===");
Console.WriteLine($"Applications submitted: {recentApplications.Count}");
Console.WriteLine($"Interviews attended: {recentInterviews.Count}");
Console.WriteLine($"Pending responses: {recentApplications.Count(a => a.Decision == "Pending")}");
Console.WriteLine($"Offers received: {recentApplications.Count(a => a.Decision == "Accepted")}");

var topSites = recentApplications
    .GroupBy(a => a.Site)
    .OrderByDescending(g => g.Count())
    .Take(3);

Console.WriteLine("\nTop job boards used:");
foreach (var site in topSites)
{
    Console.WriteLine($"  {site.Key}: {site.Count()} applications");
}
```

## Best Practices

### Application Tracking
1. **Submit application, add to tracker immediately** - Don't let applications pile up
2. **Use consistent company names** - "Microsoft" vs "Microsoft Corporation" will create duplicates
3. **Set reminders** - Follow up on pending applications after 2 weeks
4. **Track everything** - Even rejections provide valuable data
5. **Update decisions promptly** - Keeps your analytics accurate

### Interview Management
1. **Add interviews as soon as scheduled** - Don't rely on memory
2. **Parse attendees consistently** - Use "Name (Position)" format
3. **Update outcomes immediately** - Record while fresh in mind
4. **Note next steps** - What to expect, timeline, etc.
5. **Track all rounds** - Keys automatically link multiple rounds

### Data Quality
1. **Use validation dropdowns** - Ensures consistent data
2. **Verify calculated fields** - Check that formulas are working
3. **Regular cleanup** - Remove duplicate or test entries
4. **Backup regularly** - Export to JSON periodically
5. **Review analytics** - Use insights to improve strategy

## Troubleshooting

### Common Issues

**"Key already exists" Error**
- The Key is auto-generated with incrementing numbers
- Ensure Key column formula is working correctly
- Check for duplicate manual entries

**Interview not linking to Application**
- Verify Company and JobTitle match exactly (case-sensitive)
- Check that Key exists in Applications sheet
- Ensure no extra spaces in names

**Calculated fields not updating**
- Verify formulas are present in mapper configuration
- Check that Google Sheets has recalculated (may take a moment)
- Ensure dependent data exists (e.g., Decision Date for Days Active)

**Validation not working**
- Confirm reference sheets (Sites, Decisions, etc.) are populated
- Check that sheet names match exactly
- Verify validation is configured in mapper

## Testing

### Running Integration Tests

The Job project includes comprehensive integration tests that validate sheet creation, data operations, and formulas against actual Google Sheets.

#### Prerequisites

1. **Google Service Account Credentials**: Required for Google Sheets API access
2. **Test Spreadsheet**: A dedicated Google Sheets spreadsheet for testing

#### Configure User Secrets

Set up your test credentials and spreadsheet ID:

```bash
# Navigate to the Test.Common project
cd RaptorSheets.Test

# Set your Google credentials (JSON format)
dotnet user-secrets set "google_credentials:type" "service_account"
dotnet user-secrets set "google_credentials:private_key_id" "YOUR_PRIVATE_KEY_ID"
dotnet user-secrets set "google_credentials:private_key" "YOUR_PRIVATE_KEY"
dotnet user-secrets set "google_credentials:client_email" "YOUR_CLIENT_EMAIL"
dotnet user-secrets set "google_credentials:client_id" "YOUR_CLIENT_ID"

# Set your test spreadsheet ID (found in the Google Sheets URL)
dotnet user-secrets set "spreadsheets:job" "YOUR_SPREADSHEET_ID"
```

#### Run Integration Tests

```bash
# Run all integration tests
dotnet test RaptorSheets.Job.Tests --filter "Category=Integration"

# Run specific test class
dotnet test RaptorSheets.Job.Tests --filter "FullyQualifiedName~GoogleSheetsIntegrationTests"

# Run with detailed output
dotnet test RaptorSheets.Job.Tests --filter "Category=Integration" --logger "console;verbosity=detailed"
```

#### What the Integration Tests Do

1. **Environment Setup** (`IntegrationTestFixture`):
   - Runs once before all tests
   - Deletes existing sheets in test spreadsheet
   - Creates fresh sheets with proper structure
   - Validates sheet creation succeeded

2. **Sheet Structure Validation**:
   - Verifies all required sheets exist
   - Validates headers match expected configuration
   - Checks sheet tab order
   - Ensures proper formatting and protection

3. **Demo Data Generation**:
   - Tests realistic application and interview data generation
   - Validates date ranges
   - Checks reference data population

#### View Test Results in Google Sheets

After running the tests, you can open your test spreadsheet in Google Sheets to see:
- ? All sheets created with proper formatting
- ? Headers with correct names and order  
- ? Formulas configured in calculated columns
- ? Data validation dropdowns working
- ? Proper tab colors and protection

**Example Test Spreadsheet**:
```
https://docs.google.com/spreadsheets/d/YOUR_SPREADSHEET_ID/edit
```

#### Troubleshooting

**"User secrets not configured" - Tests Skipped**:
```bash
# Verify secrets are set
dotnet user-secrets list --project RaptorSheets.Test.Common.csproj
```

**"Sheet creation failed" - Permission Error**:
- Ensure your service account has Editor access to the spreadsheet
- Share the spreadsheet with your service account email: `your-service@project.iam.gserviceaccount.com`

**"Tests are slow"**:
- Integration tests make real API calls to Google Sheets
- Expected runtime: 10-30 seconds for full suite
- Tests run sequentially to avoid API rate limits

### Unit Tests

Unit tests validate individual components without requiring Google Sheets access:

```bash
# Run all unit tests
dotnet test RaptorSheets.Job.Tests --filter "Category!=Integration"

# Run specific test class
dotnet test RaptorSheets.Job.Tests --filter "FullyQualifiedName~SheetsConfigTests"
```

Unit tests cover:
- Sheet configuration validation
- Header name constants
- Sheet ordering and utilities
- Entity structure validation

## License

MIT License - see LICENSE file for details

## Support

- **Documentation**: [https://www.raptorsheets.com](https://www.raptorsheets.com)
- **Issues**: [GitHub Issues](https://github.com/khanjal/RaptorSheets/issues)
- **Discussions**: [GitHub Discussions](https://github.com/khanjal/RaptorSheets/discussions)

---

Built with ?? by Iron Raptor Digital
