using FluentAssertions;
using GigRaptorLib.Enums;
using GigRaptorLib.Utilities;
using GigRaptorLib.Utilities.Extensions;

namespace GigRaptorLib.Tests.Utilities;

public class MessageHelperTests
{
    [Fact]
    public void GivenCreateError_ThenReturnErrorMessage()
    {
        var messageText = "This is an error message.";
        var message = MessageHelper.CreateErrorMessage(messageText);

        message.Level.Should().Be(MessageLevelEnum.Error.UpperName());
        message.Message.Should().Be(messageText);
        message.Time.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GivenCreateWarning_ThenReturnWarningMessage()
    {
        var messageText = "This is a warning message.";
        var message = MessageHelper.CreateWarningMessage(messageText);

        message.Level.Should().Be(MessageLevelEnum.Warning.UpperName());
        message.Message.Should().Be(messageText);
        message.Time.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GivenCreateInfo_ThenReturnInfoMessage()
    {
        var messageText = "This is an info message.";
        var message = MessageHelper.CreateInfoMessage(messageText);

        message.Level.Should().Be(MessageLevelEnum.Info.UpperName());
        message.Message.Should().Be(messageText);
        message.Time.Should().BeGreaterThan(0);
    }
}
