using Google.Apis.Sheets.v4.Data;
using Moq;
using RaptorSheets.Core.Services;
using RaptorSheets.Job.Constants;
using RaptorSheets.Job.Managers;
using RaptorSheets.Core.Models;

namespace RaptorSheets.Job.Tests.Unit.Managers;

/// <summary>
/// Exercises GetSheets end-to-end (registry dispatch -> MapData -> entity population,
/// unknown-tab detection) against a mocked IGoogleSheetService, without any live network call.
/// </summary>
public class GetSheetsBehaviorTests
{
    private static BatchGetValuesByDataFilterResponse BuildBatchResponse(string sheetName, IList<object> headerRow, IList<object>? dataRow = null)
    {
        var values = new List<IList<object>> { headerRow };
        if (dataRow != null)
        {
            values.Add(dataRow);
        }

        return new BatchGetValuesByDataFilterResponse
        {
            ValueRanges = new List<MatchedValueRange>
            {
                new()
                {
                    DataFilters = new List<DataFilter> { new() { A1Range = sheetName } },
                    ValueRange = new ValueRange { Values = values }
                }
            }
        };
    }

    [Fact]
    public async Task GetSheets_ForApplications_MapsRowDataOntoEntity()
    {
        var mockService = new Mock<IGoogleSheetService>();

        mockService
            .Setup(s => s.GetBatchData(It.IsAny<List<string>>(), It.IsAny<string>()))
            .ReturnsAsync(BuildBatchResponse(
                SheetsConfig.SheetNames.Applications,
                new List<object> { "Date", "Company", "Job Title", "Posting", "Site", "Interviews", "Decision", "Decision Date", "Days Active", "Notes", "Pay Low", "Pay High", "Pay Avg", "Location", "Schedule", "#", "Key" },
                new List<object> { "2026-06-01", "TechCorp", "Software Engineer", "https://example.com/job/1", "LinkedIn", "0", "Pending", "", "5", "", "100000", "150000", "125000", "Remote", "Full-time", "0", "TechCorp-Software Engineer-0" }));
        mockService
            .Setup(s => s.GetBatchDataResult(It.IsAny<List<string>>(), It.IsAny<string>()))
            .ReturnsAsync(GoogleApiResult<BatchGetValuesByDataFilterResponse>.Ok(BuildBatchResponse(
                SheetsConfig.SheetNames.Applications,
                new List<object> { "Date", "Company", "Job Title", "Posting", "Site", "Interviews", "Decision", "Decision Date", "Days Active", "Notes", "Pay Low", "Pay High", "Pay Avg", "Location", "Schedule", "#", "Key" },
                new List<object> { "2026-06-01", "TechCorp", "Software Engineer", "https://example.com/job/1", "LinkedIn", "0", "Pending", "", "5", "", "100000", "150000", "125000", "Remote", "Full-time", "0", "TechCorp-Software Engineer-0" })));

        mockService
            .Setup(s => s.GetSheetInfo())
            .ReturnsAsync(new Spreadsheet
            {
                Properties = new SpreadsheetProperties { Title = "MyJobSheet" },
                Sheets = new List<Sheet>
                {
                    new() { Properties = new SheetProperties { Title = SheetsConfig.SheetNames.Applications } }
                }
            });

        var manager = new GoogleSheetManager(mockService.Object);

        var result = await manager.GetSheets([SheetsConfig.SheetNames.Applications]);

        Assert.NotNull(result);
        var application = Assert.Single(result.Sheets.Applications);
        Assert.Equal("TechCorp", application.Company);
        Assert.Equal("Software Engineer", application.JobTitle);
        Assert.Equal("Pending", application.Decision);
        Assert.Equal("MyJobSheet", result.Properties.Name);
    }

    [Fact]
    public async Task GetSheets_WithUnknownSheetTab_SurfacesWarning()
    {
        var mockService = new Mock<IGoogleSheetService>();

        mockService
            .Setup(s => s.GetBatchData(It.IsAny<List<string>>(), It.IsAny<string>()))
            .ReturnsAsync(BuildBatchResponse(SheetsConfig.SheetNames.Applications, new List<object> { "Date", "Company", "Job Title" }));
        mockService
            .Setup(s => s.GetBatchDataResult(It.IsAny<List<string>>(), It.IsAny<string>()))
            .ReturnsAsync(GoogleApiResult<BatchGetValuesByDataFilterResponse>.Ok(BuildBatchResponse(SheetsConfig.SheetNames.Applications, new List<object> { "Date", "Company", "Job Title" })));

        mockService
            .Setup(s => s.GetSheetInfo())
            .ReturnsAsync(new Spreadsheet
            {
                Properties = new SpreadsheetProperties { Title = "MyJobSheet" },
                Sheets = new List<Sheet>
                {
                    new() { Properties = new SheetProperties { Title = SheetsConfig.SheetNames.Applications } },
                    new() { Properties = new SheetProperties { Title = "SomeRandomTab" } }
                }
            });

        var manager = new GoogleSheetManager(mockService.Object);

        var result = await manager.GetSheets([SheetsConfig.SheetNames.Applications]);

        Assert.Contains(result.Messages, m => m.Message.Contains("SomeRandomTab"));
    }

    [Fact]
    public async Task GetSheets_WhenBatchDataFails_ReturnsErrorMessage()
    {
        var mockService = new Mock<IGoogleSheetService>();

        mockService
            .Setup(s => s.GetBatchData(It.IsAny<List<string>>(), It.IsAny<string>()))
            .ReturnsAsync((BatchGetValuesByDataFilterResponse?)null);
        mockService
            .Setup(s => s.GetBatchDataResult(It.IsAny<List<string>>(), It.IsAny<string>()))
            .ReturnsAsync(GoogleApiResult<BatchGetValuesByDataFilterResponse>.Failed(new GoogleApiFailure { Reason = GoogleApiFailureReason.Unknown, Message = "test failure" }));

        // Populate every canonical Job sheet so GetSheets' self-heal finds nothing missing and
        // falls through to the "unable to retrieve" error this test is actually targeting.
        mockService
            .Setup(s => s.GetSheetInfo())
            .ReturnsAsync(new Spreadsheet
            {
                Sheets = SheetsConfig.SheetUtilities.GetAllSheetNames()
                    .Select(name => new Sheet { Properties = new SheetProperties { Title = name } })
                    .ToList()
            });

        var manager = new GoogleSheetManager(mockService.Object);

        var result = await manager.GetSheets([SheetsConfig.SheetNames.Applications]);

        Assert.Contains(result.Messages, m => m.Message.Contains("Unable to retrieve"));
    }
}
