using Moq;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Managers;
using RaptorSheets.Core.Services;
using Xunit;

namespace RaptorSheets.Core.Tests.Unit.Managers;

public class BaseDomainSheetManagerTests
{
    private class TestSheetManager : BaseDomainSheetManager<MessageEntity, MessageTypeEnum>
    {
        public TestSheetManager(string accessToken, string spreadsheetId) : base(accessToken, spreadsheetId) { }

        public TestSheetManager(Dictionary<string, string> parameters, string spreadsheetId) : base(parameters, spreadsheetId) { }

        public override List<string> GetAvailableSheetNames() => new() { "Sheet1", "Sheet2" };

        protected override Task<MessageEntity> CreateSheets(List<string> sheets)
        {
            return Task.FromResult(new MessageEntity { Message = "Sheets created successfully" });
        }
    }

    [Fact]
    public async Task ValidateSheets_WithAllSheetsPresent_ShouldReturnSuccessMessage()
    {
        // Arrange
        var mockService = new Mock<GoogleSheetService>("token", "spreadsheetId");
        mockService.Setup(s => s.GetSheetInfo()).ReturnsAsync(new Spreadsheet
        {
            Sheets = new List<Sheet>
            {
                new() { Properties = new SheetProperties { Title = "Sheet1" } },
                new() { Properties = new SheetProperties { Title = "Sheet2" } }
            }
        });

        var manager = new TestSheetManager("token", "spreadsheetId")
        {
            _googleSheetService = mockService.Object
        };

        // Act
        var result = await manager.ValidateSheets();

        // Assert
        Assert.Single(result);
        Assert.Contains(result, m => m.Message.Contains("All required sheets exist"));
    }

    [Fact]
    public async Task ValidateSheets_WithMissingSheets_ShouldReturnErrorMessages()
    {
        // Arrange
        var mockService = new Mock<GoogleSheetService>("token", "spreadsheetId");
        mockService.Setup(s => s.GetSheetInfo()).ReturnsAsync(new Spreadsheet
        {
            Sheets = new List<Sheet>
            {
                new() { Properties = new SheetProperties { Title = "Sheet1" } }
            }
        });

        var manager = new TestSheetManager("token", "spreadsheetId")
        {
            _googleSheetService = mockService.Object
        };

        // Act
        var result = await manager.ValidateSheets();

        // Assert
        Assert.NotEmpty(result);
        Assert.Contains(result, m => m.Message.Contains("Missing required sheet: Sheet2"));
    }

    [Fact]
    public async Task SheetExists_WithExistingSheet_ShouldReturnTrue()
    {
        // Arrange
        var mockService = new Mock<GoogleSheetService>("token", "spreadsheetId");
        mockService.Setup(s => s.GetSheetInfo()).ReturnsAsync(new Spreadsheet
        {
            Sheets = new List<Sheet>
            {
                new() { Properties = new SheetProperties { Title = "Sheet1" } }
            }
        });

        var manager = new TestSheetManager("token", "spreadsheetId")
        {
            _googleSheetService = mockService.Object
        };

        // Act
        var exists = await manager.SheetExists("Sheet1");

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task SheetExists_WithNonExistingSheet_ShouldReturnFalse()
    {
        // Arrange
        var mockService = new Mock<GoogleSheetService>("token", "spreadsheetId");
        mockService.Setup(s => s.GetSheetInfo()).ReturnsAsync(new Spreadsheet
        {
            Sheets = new List<Sheet>
            {
                new() { Properties = new SheetProperties { Title = "Sheet1" } }
            }
        });

        var manager = new TestSheetManager("token", "spreadsheetId")
        {
            _googleSheetService = mockService.Object
        };

        // Act
        var exists = await manager.SheetExists("Sheet2");

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public async Task HandleMissingSheets_WithSheetsCreated_ShouldReturnSuccessMessage()
    {
        // Arrange
        var manager = new TestSheetManager("token", "spreadsheetId");

        // Act
        var result = await manager.HandleMissingSheets(new List<string> { "Sheet3" });

        // Assert
        Assert.Single(result);
        Assert.Contains(result, m => m.Message.Contains("Created 1 missing sheets"));
    }

    [Fact]
    public void CreateSuccessMessage_ShouldReturnCorrectMessage()
    {
        // Act
        var message = BaseDomainSheetManager<MessageEntity, MessageTypeEnum>.CreateSuccessMessage("Operation", "Details", MessageTypeEnum.ADD_DATA);

        // Assert
        Assert.Equal("Operation successful: Details", message.Message);
    }

    [Fact]
    public void CreateErrorMessage_ShouldReturnCorrectMessage()
    {
        // Act
        var message = BaseDomainSheetManager<MessageEntity, MessageTypeEnum>.CreateErrorMessage("Operation", "Details", MessageTypeEnum.ADD_DATA);

        // Assert
        Assert.Equal("Operation failed: Details", message.Message);
    }

    [Fact]
    public void CreateWarningMessage_ShouldReturnCorrectMessage()
    {
        // Act
        var message = BaseDomainSheetManager<MessageEntity, MessageTypeEnum>.CreateWarningMessage("Operation", "Details", MessageTypeEnum.ADD_DATA);

        // Assert
        Assert.Equal("Operation: Details", message.Message);
    }
}