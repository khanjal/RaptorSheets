using GigRaptorLib.Entities;

namespace GigRaptorLib.Utilities;

public static class MessageHelper
{
    public static MessageEntity CreateMessage(MessageEntity message)
    {
        message.Time = (long)DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalSeconds;
        return message;
    }
    public static MessageEntity CreateErrorMessage(string message)
    {
        return CreateMessage(new MessageEntity { Message = message, Type = Enums.MessageEnum.Error });
    }

    public static MessageEntity CreateWarningMessage(string message)
    {
        return CreateMessage(new MessageEntity { Message = message, Type = Enums.MessageEnum.Warning });
    }

    public static MessageEntity CreateInfoMessage(string message)
    {
        return CreateMessage(new MessageEntity { Message = message, Type = Enums.MessageEnum.Info });
    }
}
