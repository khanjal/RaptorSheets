using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Managers;
using System.Reflection;
using System.ComponentModel;
using RaptorSheets.Core.Extensions;

namespace RaptorSheets.Gig.Tests.Unit.Managers;

[Category("Unit Tests")]
public class GoogleSheetManagerHelpersTests
{
    [Fact]
    public async Task HandleMissingSheets_WithNullSpreadsheet_ReturnsErrorMessage()
    {
        var manager = new GoogleSheetManager("test-token", "test-spreadsheet-id");

        var method = typeof(GoogleSheetManager).GetMethod("HandleMissingSheets", BindingFlags.NonPublic | BindingFlags.Instance);
        var task = (Task<List<MessageEntity>>)method!.Invoke(manager, new object?[] { null })!;
        var messages = await task;

        Assert.NotNull(messages);
        Assert.Single(messages);
        Assert.Equal(MessageTypeEnum.GET_SHEETS.GetDescription(), messages[0].Type);
        Assert.Contains("Unable to retrieve sheet(s)", messages[0].Message);
    }

    [Fact]
    public async Task HandleMissingSheets_WithAllSheetsPresent_ReturnsEmptyList()
    {
        var manager = new GoogleSheetManager("test-token", "test-spreadsheet-id");

        var spreadsheet = new Spreadsheet
        {
            Sheets = Enum.GetNames(typeof(SheetEnum))
                .Select(n => new Sheet { Properties = new SheetProperties { Title = n } })
                .ToList()
        };

        var method = typeof(GoogleSheetManager).GetMethod("HandleMissingSheets", BindingFlags.NonPublic | BindingFlags.Instance);
        var task = (Task<List<MessageEntity>>)method!.Invoke(manager, new object?[] { spreadsheet })!;
        var messages = await task;

        Assert.NotNull(messages);
        Assert.Empty(messages);
    }
}
