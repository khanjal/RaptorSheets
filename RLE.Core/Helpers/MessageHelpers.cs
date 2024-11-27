using RLE.Core.Entities;
using RLE.Core.Enums;
using RLE.Core.Extensions;

namespace RLE.Core.Helpers;

public static class MessageHelpers
{
    public static MessageEntity CreateMessage(MessageEntity message)
    {
        if (string.IsNullOrEmpty(message.Type))
        {
            message.Type = MessageTypeEnum.General.GetDescription();
        }

        message.Time = (long)DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalSeconds;
        return message;
    }
    public static MessageEntity CreateErrorMessage(string message, MessageTypeEnum type)
    {
        return CreateMessage(new MessageEntity { Message = message, Level = MessageLevelEnum.Error.UpperName(), Type = type.GetDescription() });
    }

    public static MessageEntity CreateWarningMessage(string message, MessageTypeEnum type)
    {
        return CreateMessage(new MessageEntity { Message = message, Level = MessageLevelEnum.Warning.UpperName(), Type = type.GetDescription() });
    }

    public static MessageEntity CreateInfoMessage(string message, MessageTypeEnum type)
    {
        return CreateMessage(new MessageEntity { Message = message, Level = MessageLevelEnum.Info.UpperName(), Type = type.GetDescription() });
    }
}
