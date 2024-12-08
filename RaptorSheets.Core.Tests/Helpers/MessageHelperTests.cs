using FluentAssertions;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using Xunit;

namespace RaptorSheets.Core.Tests.Helpers;

public class MessageHelperTests
{
    [Fact]
    public void GivenCreateError_ThenReturnErrorMessage()
    {
        var messageText = "This is an error message.";
        var message = MessageHelpers.CreateErrorMessage(messageText, MessageTypeEnum.GENERAL);

        message.Level.Should().Be(MessageLevelEnum.ERROR.UpperName());
        message.Message.Should().Be(messageText);
        message.Time.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GivenCreateWarning_ThenReturnWarningMessage()
    {
        var messageText = "This is a warning message.";
        var message = MessageHelpers.CreateWarningMessage(messageText, MessageTypeEnum.GENERAL);

        message.Level.Should().Be(MessageLevelEnum.WARNING.UpperName());
        message.Message.Should().Be(messageText);
        message.Time.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GivenCreateInfo_ThenReturnInfoMessage()
    {
        var messageText = "This is an info message.";
        var message = MessageHelpers.CreateInfoMessage(messageText, MessageTypeEnum.GENERAL);

        message.Level.Should().Be(MessageLevelEnum.INFO.UpperName());
        message.Message.Should().Be(messageText);
        message.Time.Should().BeGreaterThan(0);
    }
}
