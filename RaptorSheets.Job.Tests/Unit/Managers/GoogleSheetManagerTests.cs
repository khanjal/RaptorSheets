using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Job.Constants;
using RaptorSheets.Job.Entities;
using RaptorSheets.Job.Managers;

namespace RaptorSheets.Job.Tests.Unit.Managers;

/// <summary>
/// Edge-case coverage against a real (but fake-token) GoogleSheetManager - network calls fail
/// gracefully (caught/logged inside GoogleSheetService), so these exercise the manager's dispatch
/// logic (accessor map, empty-list handling, message building) without needing a live spreadsheet.
/// </summary>
public class GoogleSheetManagerTests
{
    private readonly GoogleSheetManager _manager = new("test-token", "test-spreadsheet-id");

    [Fact]
    public void Constructor_WithAccessToken_Initializes()
    {
        var manager = new GoogleSheetManager("test-token", "test-spreadsheet-id");
        Assert.NotNull(manager);
    }

    [Fact]
    public async Task ChangeSheetData_WithNoData_ReturnsWarningMessage()
    {
        var sheets = new List<string> { SheetsConfig.SheetNames.Applications };
        var sheetEntity = new SheetEntity();

        var result = await _manager.ChangeSheetData(sheets, sheetEntity);

        Assert.Contains(result.Messages, m => m.Message.Contains("No data to change"));
        Assert.Contains(result.Messages, m => m.Level == MessageLevel.WARNING.GetDescription());
    }

    [Fact]
    public async Task ChangeSheetData_ForUnsupportedSheet_ReturnsErrorMessage()
    {
        var sheets = new List<string> { "NotARealSheet" };
        var sheetEntity = new SheetEntity();

        var result = await _manager.ChangeSheetData(sheets, sheetEntity);

        Assert.Contains(result.Messages, m => m.Message.Contains("not supported"));
    }

    [Fact]
    public async Task ChangeSheetData_WithData_AttemptsToProcessRequest()
    {
        var sheets = new List<string> { SheetsConfig.SheetNames.Applications };
        var sheetEntity = new SheetEntity();
        sheetEntity.Sheets.Applications.Add(new ApplicationEntity { RowId = 2, Company = "TechCorp", JobTitle = "Software Engineer" });

        var result = await _manager.ChangeSheetData(sheets, sheetEntity);

        // Fails against fake credentials, but should have gotten past dispatch/resolution.
        Assert.NotEmpty(result.Messages);
    }

    [Fact]
    public async Task DeleteSheets_WithEmptyList_ReturnsWarningMessage()
    {
        var result = await _manager.DeleteSheets([]);

        Assert.Contains(result.Messages, m => m.Message.Contains("No sheets found to delete"));
    }

    [Fact]
    public async Task CreateSheets_WithEmptyList_DoesNotThrow()
    {
        var result = await _manager.CreateSheets(new List<string>());

        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetSheets_WithEmptyList_DoesNotThrow()
    {
        var result = await _manager.GetSheets([]);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetSheet_ForUnknownSheetName_ReturnsErrorMessage()
    {
        var result = await _manager.GetSheet("NotARealSheet");

        Assert.Contains(result.Messages, m => m.Message.Contains("does not exist"));
    }

    [Fact]
    public void GetSheetLayout_ForEveryConfiguredSheet_ReturnsNonNull()
    {
        foreach (var name in SheetsConfig.SheetUtilities.GetAllSheetNames())
        {
            Assert.NotNull(_manager.GetSheetLayout(name));
        }
    }

    [Fact]
    public void GetSheetLayout_ForUnknownSheet_ReturnsNull()
    {
        Assert.Null(_manager.GetSheetLayout("NotARealSheet"));
    }
}
