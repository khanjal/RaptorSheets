using GigRaptorLib.Entities;
using GigRaptorLib.Utilities.Extensions;

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
        return CreateMessage(new MessageEntity { Message = message, Type = Enums.MessageEnum.Error.UpperName() });
    }

    public static MessageEntity CreateWarningMessage(string message)
    {
        return CreateMessage(new MessageEntity { Message = message, Type = Enums.MessageEnum.Warning.UpperName() });
    }

    public static MessageEntity CreateInfoMessage(string message)
    {
        return CreateMessage(new MessageEntity { Message = message, Type = Enums.MessageEnum.Info.UpperName() });
    }
}
