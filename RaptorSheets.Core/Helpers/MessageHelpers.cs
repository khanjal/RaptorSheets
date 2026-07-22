using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;

namespace RaptorSheets.Core.Helpers;

public static class MessageHelpers
{
    public static MessageEntity CreateMessage(MessageEntity message)
    {
        if (string.IsNullOrWhiteSpace(message.Type))
        {
            message.Type = MessageType.GENERAL.GetDescription();
        }

        message.Time = (long)DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalSeconds;
        return message;
    }
    public static MessageEntity CreateErrorMessage(string message, MessageType type)
    {
        return CreateMessage(new MessageEntity { Message = message, Level = MessageLevel.ERROR.UpperName(), Type = type.GetDescription() });
    }

    public static MessageEntity CreateWarningMessage(string message, MessageType type)
    {
        return CreateMessage(new MessageEntity { Message = message, Level = MessageLevel.WARNING.UpperName(), Type = type.GetDescription() });
    }

    public static MessageEntity CreateInfoMessage(string message, MessageType type)
    {
        return CreateMessage(new MessageEntity { Message = message, Level = MessageLevel.INFO.UpperName(), Type = type.GetDescription() });
    }
}
