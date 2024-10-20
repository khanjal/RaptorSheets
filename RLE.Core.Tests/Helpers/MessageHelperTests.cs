using FluentAssertions;
using RLE.Core.Enums;
using RLE.Core.Extensions;
using RLE.Core.Helpers;
using Xunit;

namespace RLE.Core.Tests.Helpers;

public class MessageHelperTests
{
    [Fact]
    public void GivenCreateError_ThenReturnErrorMessage()
    {
        var messageText = "This is an error message.";
        var message = MessageHelper.CreateErrorMessage(messageText, MessageTypeEnum.General);

        message.Level.Should().Be(MessageLevelEnum.Error.UpperName());
        message.Message.Should().Be(messageText);
        message.Time.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GivenCreateWarning_ThenReturnWarningMessage()
    {
        var messageText = "This is a warning message.";
        var message = MessageHelper.CreateWarningMessage(messageText, MessageTypeEnum.General);

        message.Level.Should().Be(MessageLevelEnum.Warning.UpperName());
        message.Message.Should().Be(messageText);
        message.Time.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GivenCreateInfo_ThenReturnInfoMessage()
    {
        var messageText = "This is an info message.";
        var message = MessageHelper.CreateInfoMessage(messageText, MessageTypeEnum.General);

        message.Level.Should().Be(MessageLevelEnum.Info.UpperName());
        message.Message.Should().Be(messageText);
        message.Time.Should().BeGreaterThan(0);
    }
}
