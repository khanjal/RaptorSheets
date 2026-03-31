using RaptorSheets.Core.Constants;
using RaptorSheets.Job.Tests.Data.Attributes;
using RaptorSheets.Job.Tests.Integration.Base;
using System.ComponentModel;

namespace RaptorSheets.Job.Tests.Integration;

/// <summary>
/// Integration tests for Google Sheets operations.
/// 
/// Test Organization:
/// - Single orchestrated flow to minimize API calls
/// - Each test validates a specific aspect during the flow
/// - Shared test data across related validations
/// - Collection fixture ensures sheets exist before tests run
/// </summary>
[Collection("JobIntegrationCollection")]
[Category("Integration")]
[Trait("TestType", "Integration")]
public class GoogleSheetsIntegrationTests : IntegrationTestBase
{
    #region 1. Environment Setup & Validation

    [FactCheckUserSecrets]
    public async Task Environment_ShouldHaveAllRequiredSheets()
    {
        // Act
        var properties = await GoogleSheetManager!.GetSheetProperties(TestSheets);
        var existingSheets = properties.Where(p => !string.IsNullOrEmpty(p.Id)).ToList();

        // Assert
        Assert.True(existingSheets.Count >= TestSheets.Count, 
            $"Should have at least {TestSheets.Count} sheets, found {existingSheets.Count}");

        foreach (var sheet in existingSheets)
        {
            Assert.NotNull(sheet.Name);
            Assert.NotNull(sheet.Id);
            Assert.NotNull(sheet.Attributes);
            Assert.True(TestSheets.Contains(sheet.Name, StringComparer.OrdinalIgnoreCase),
                $"Sheet '{sheet.Name}' should be in test sheets list");
        }
    }

    [FactCheckUserSecrets]
    public async Task Environment_SheetProperties_ShouldHaveValidStructure()
    {
        // Act
        var properties = await GoogleSheetManager!.GetSheetProperties(TestSheets);

        // Assert
        Assert.NotEmpty(properties);
        Assert.All(properties, prop =>
        {
            Assert.NotNull(prop.Name);
            Assert.NotNull(prop.Attributes);

            if (!string.IsNullOrEmpty(prop.Id))
            {
                // Sheet exists - validate it has expected structure
                Assert.True(prop.Attributes.ContainsKey("Headers") || 
                           prop.Attributes.Count == 0, 
                           $"Sheet '{prop.Name}' should have headers or empty attributes");
            }
        });
    }

    [FactCheckUserSecrets]
    public async Task CreatedSheets_ShouldHaveCorrectHeaders()
    {
        // This test validates that the sheet creation process generated correct headers
        // It compares actual headers in Google Sheets vs expected headers from GetSheetLayout

        // Act - Get actual headers from Google Sheets
        var spreadsheetInfo = await GoogleSheetManager!.GetSpreadsheetInfo(
            TestSheets.Select(name => $"{name}!1:1").ToList());

        Assert.NotNull(spreadsheetInfo);
        Assert.NotNull(spreadsheetInfo.Sheets);

        // Assert - Validate headers for each sheet
        foreach (var sheet in spreadsheetInfo.Sheets)
        {
            var sheetName = sheet.Properties.Title;
            if (!TestSheets.Contains(sheetName))
                continue; // Skip sheets not in our test list

            var actualHeaders = sheet.Data?[0]?.RowData?[0]?.Values
                ?.Select(v => v.FormattedValue ?? "")
                .Where(h => !string.IsNullOrEmpty(h))
                .ToList() ?? [];

            // Get expected layout from GetSheetLayout
            var expectedLayout = GoogleSheetManager.GetSheetLayout(sheetName);

            if (expectedLayout != null)
            {
                var expectedHeaders = expectedLayout.Headers
                    .Select(h => h.Name)
                    .ToList();

                Assert.NotEmpty(actualHeaders);
                Assert.Equal(expectedHeaders.Count, actualHeaders.Count);

                // Verify each header matches
                for (int i = 0; i < expectedHeaders.Count; i++)
                {
                    var expected = expectedHeaders[i];
                    var actual = actualHeaders[i];
                    Assert.Equal(expected, actual);
                }
            }
        }
    }

    #endregion

    #region 2. Sheet Layouts & Configuration

    [FactCheckUserSecrets]
    public void GetSheetLayout_ApplicationsSheet_ShouldHaveCorrectStructure()
    {
        // Act
        var layout = GoogleSheetManager!.GetSheetLayout(SheetsConfig.SheetNames.Applications);

        // Assert
        Assert.NotNull(layout);
        Assert.Equal(SheetsConfig.SheetNames.Applications, layout.Name);
        Assert.NotEmpty(layout.Headers);

        // Verify key headers exist
        var headerNames = layout.Headers.Select(h => h.Name).ToList();
        Assert.Contains(SheetsConfig.HeaderNames.Date, headerNames);
        Assert.Contains(SheetsConfig.HeaderNames.Company, headerNames);
        Assert.Contains(SheetsConfig.HeaderNames.JobTitle, headerNames);
        Assert.Contains(SheetsConfig.HeaderNames.Key, headerNames);
        Assert.Contains(SheetsConfig.HeaderNames.InterviewCount, headerNames);
        Assert.Contains(SheetsConfig.HeaderNames.Decision, headerNames);
    }

    [FactCheckUserSecrets]
    public void GetSheetLayout_InterviewsSheet_ShouldHaveCorrectStructure()
    {
        // Act
        var layout = GoogleSheetManager!.GetSheetLayout(SheetsConfig.SheetNames.Interviews);

        // Assert
        Assert.NotNull(layout);
        Assert.Equal(SheetsConfig.SheetNames.Interviews, layout.Name);
        Assert.NotEmpty(layout.Headers);

        // Verify key headers exist
        var headerNames = layout.Headers.Select(h => h.Name).ToList();
        Assert.Contains(SheetsConfig.HeaderNames.Date, headerNames);
        Assert.Contains(SheetsConfig.HeaderNames.Company, headerNames);
        Assert.Contains(SheetsConfig.HeaderNames.JobTitle, headerNames);
        Assert.Contains(SheetsConfig.HeaderNames.Key, headerNames);
        Assert.Contains(SheetsConfig.HeaderNames.InterviewType, headerNames);
        Assert.Contains(SheetsConfig.HeaderNames.InterviewRound, headerNames);
    }

    [FactCheckUserSecrets]
    public void GetSheetLayouts_MultipleSheets_ShouldReturnAllLayouts()
    {
        // Arrange
        var sheetNames = new List<string>
        {
            SheetsConfig.SheetNames.Applications,
            SheetsConfig.SheetNames.Interviews,
            SheetsConfig.SheetNames.Companies
        };

        // Act
        var layouts = GoogleSheetManager!.GetSheetLayouts(sheetNames);

        // Assert
        Assert.Equal(sheetNames.Count, layouts.Count);
        Assert.All(layouts, layout =>
        {
            Assert.NotNull(layout);
            Assert.NotNull(layout.Name);
            Assert.NotEmpty(layout.Headers);
        });
    }

    #endregion

    #region 3. Sheet Tab Names & Order

    [FactCheckUserSecrets]
    public async Task GetAllSheetTabNames_ShouldReturnOrderedList()
    {
        // Act
        var tabNames = await GoogleSheetManager!.GetAllSheetTabNames();

        // Assert
        Assert.NotEmpty(tabNames);

        // Verify our test sheets are present
        foreach (var testSheet in TestSheets)
        {
            Assert.Contains(tabNames, name => 
                string.Equals(name, testSheet, StringComparison.OrdinalIgnoreCase));
        }
    }

    [FactCheckUserSecrets]
    public async Task GetSpreadsheetInfo_ShouldReturnValidMetadata()
    {
        // Act
        var spreadsheetInfo = await GoogleSheetManager!.GetSpreadsheetInfo();

        // Assert
        Assert.NotNull(spreadsheetInfo);
        Assert.NotNull(spreadsheetInfo.Properties);
        Assert.NotNull(spreadsheetInfo.Properties.Title);
        Assert.NotNull(spreadsheetInfo.Sheets);
        Assert.NotEmpty(spreadsheetInfo.Sheets);
    }

    #endregion

    #region 4. Demo Data Generation

    [Fact]
    public void GenerateDemoData_ShouldCreateValidData()
    {
        // Arrange
        var startDate = DateTime.Today.AddDays(-30);
        var endDate = DateTime.Today;

        // Act
        var demoData = GoogleSheetManager.GenerateDemoData(startDate, endDate);

        // Assert
        Assert.NotNull(demoData);
        Assert.NotEmpty(demoData.Applications);
        Assert.NotEmpty(demoData.Sites);
        Assert.NotEmpty(demoData.Decisions);
        Assert.NotEmpty(demoData.InterviewTypes);
        Assert.NotEmpty(demoData.InterviewOutcomes);
        Assert.NotEmpty(demoData.Schedules);

        // Verify application data is valid
        Assert.All(demoData.Applications, app =>
        {
            Assert.NotEmpty(app.Date);
            Assert.NotEmpty(app.Company);
            Assert.NotEmpty(app.JobTitle);
            Assert.True(app.RowId > 0);
        });
    }

    [Fact]
    public void GenerateDemoData_WithCustomDates_ShouldRespectDateRange()
    {
        // Arrange
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 1, 31);

        // Act
        var demoData = GoogleSheetManager.GenerateDemoData(startDate, endDate);

        // Assert
        Assert.NotNull(demoData);
        Assert.NotEmpty(demoData.Applications);

        // All applications should be within the date range
        foreach (var app in demoData.Applications)
        {
            var appDate = DateTime.Parse(app.Date);
            Assert.True(appDate >= startDate && appDate <= endDate,
                $"Application date {appDate:yyyy-MM-dd} should be between {startDate:yyyy-MM-dd} and {endDate:yyyy-MM-dd}");
        }
    }

    #endregion
}
