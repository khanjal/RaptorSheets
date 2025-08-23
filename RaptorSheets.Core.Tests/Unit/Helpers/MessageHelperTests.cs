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

    [Fact]
    public void CreateMessage_WithExistingType_ShouldPreserveType()
    {
        // Arrange
        var message = new MessageEntity 
        { 
            Message = "Test message", 
            Level = "INFO", 
            Type = MessageTypeEnum.UPDATE_DATA.GetDescription() 
        };

        // Act
        var result = MessageHelpers.CreateMessage(message);

        // Assert
        Assert.Equal(MessageTypeEnum.UPDATE_DATA.GetDescription(), result.Type);
        Assert.Equal(message.Message, result.Message);
        Assert.Equal(message.Level, result.Level);
    }

    [Fact]
    public void CreateMessage_WithNullMessage_ShouldHandleGracefully()
    {
        // Arrange
        var message = new MessageEntity 
        { 
            Message = null!, 
            Level = "INFO" 
        };

        // Act
        var result = MessageHelpers.CreateMessage(message);

        // Assert
        Assert.Null(result.Message);
        Assert.Equal(MessageTypeEnum.GENERAL.GetDescription(), result.Type);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void CreateMessage_WithEmptyOrNullType_ShouldSetDefaultType(string? type)
    {
        // Arrange
        var message = new MessageEntity 
        { 
            Message = "Test message", 
            Level = "INFO",
            Type = type!
        };

        // Act
        var result = MessageHelpers.CreateMessage(message);

        // Assert
        Assert.Equal(MessageTypeEnum.GENERAL.GetDescription(), result.Type);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void CreateErrorMessage_WithEmptyMessage_ShouldHandleGracefully(string? messageText)
    {
        // Arrange
        var type = MessageTypeEnum.ADD_DATA;

        // Act
        var result = MessageHelpers.CreateErrorMessage(messageText!, type);

        // Assert
        Assert.Equal(messageText, result.Message);
        Assert.Equal(MessageLevelEnum.ERROR.UpperName(), result.Level);
        Assert.Equal(type.GetDescription(), result.Type);
    }

    [Fact]
    public void CreateMessage_ShouldSetCorrectTimestamp()
    {
        // Arrange
        var beforeTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var message = new MessageEntity { Message = "Test message", Level = "INFO" };

        // Act
        var result = MessageHelpers.CreateMessage(message);
        var afterTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // Assert
        Assert.InRange(result.Time, beforeTime, afterTime);
    }

    [Theory]
    [InlineData(MessageTypeEnum.ADD_DATA)]
    [InlineData(MessageTypeEnum.CHECK_SHEET)]
    [InlineData(MessageTypeEnum.CREATE_SHEET)]
    [InlineData(MessageTypeEnum.DELETE_DATA)]
    [InlineData(MessageTypeEnum.GENERAL)]
    [InlineData(MessageTypeEnum.GET_SHEETS)]
    [InlineData(MessageTypeEnum.UPDATE_DATA)]
    public void CreateMessage_WithAllMessageTypes_ShouldWorkCorrectly(MessageTypeEnum messageType)
    {
        // Arrange
        var message = "Test message";

        // Act
        var result = MessageHelpers.CreateInfoMessage(message, messageType);

        // Assert
        Assert.Equal(message, result.Message);
        Assert.Equal(MessageLevelEnum.INFO.UpperName(), result.Level);
        Assert.Equal(messageType.GetDescription(), result.Type);
    }
}
