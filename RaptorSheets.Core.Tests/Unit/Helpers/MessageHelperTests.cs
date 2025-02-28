using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using Xunit;

namespace RaptorSheets.Core.Tests.Unit.Helpers;

public class MessageHelperTests
{
    [Fact]
    public void CreateMessage_ShouldSetDefaultType_WhenTypeIsNullOrEmpty()
    {
        // Arrange
        var message = new MessageEntity { Message = "Test message", Level = "INFO" };

        // Act
        var result = MessageHelpers.CreateMessage(message);

        // Assert
        Assert.Equal(MessageTypeEnum.GENERAL.GetDescription(), result.Type);
        Assert.True(result.Time > 0);
    }

    [Fact]
    public void CreateErrorMessage_ShouldSetCorrectProperties()
    {
        // Arrange
        var message = "Error occurred";
        var type = MessageTypeEnum.ADD_DATA;

        // Act
        var result = MessageHelpers.CreateErrorMessage(message, type);

        // Assert
        Assert.Equal(message, result.Message);
        Assert.Equal(MessageLevelEnum.ERROR.UpperName(), result.Level);
        Assert.Equal(type.GetDescription(), result.Type);
        Assert.True(result.Time > 0);
    }

    [Fact]
    public void CreateWarningMessage_ShouldSetCorrectProperties()
    {
        // Arrange
        var message = "Warning issued";
        var type = MessageTypeEnum.CHECK_SHEET;

        // Act
        var result = MessageHelpers.CreateWarningMessage(message, type);

        // Assert
        Assert.Equal(message, result.Message);
        Assert.Equal(MessageLevelEnum.WARNING.UpperName(), result.Level);
        Assert.Equal(type.GetDescription(), result.Type);
        Assert.True(result.Time > 0);
    }

    [Fact]
    public void CreateInfoMessage_ShouldSetCorrectProperties()
    {
        // Arrange
        var message = "Information message";
        var type = MessageTypeEnum.GET_SHEETS;

        // Act
        var result = MessageHelpers.CreateInfoMessage(message, type);

        // Assert
        Assert.Equal(message, result.Message);
        Assert.Equal(MessageLevelEnum.INFO.UpperName(), result.Level);
        Assert.Equal(type.GetDescription(), result.Type);
        Assert.True(result.Time > 0);
    }
}
