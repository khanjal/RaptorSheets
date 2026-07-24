using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Models;

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

    /// <summary>
    /// Renders a <see cref="MappingIssue"/> as a warning message. Always a warning, never an error -
    /// a mapping issue never fails or blocks a read, it only explains why a value came back as its
    /// type default. Omits the raw cell value even when present on the issue, since a message string
    /// is exactly the kind of place personal/financial cell content should not end up by default; a
    /// caller that opted into raw values reads them from the MappingIssue directly.
    /// </summary>
    public static MessageEntity CreateMappingIssueMessage(MappingIssue issue)
    {
        var location = string.IsNullOrEmpty(issue.SheetName)
            ? $"Row {issue.RowId}, column [{issue.Header}]"
            : $"[{issue.SheetName}] Row {issue.RowId}, column [{issue.Header}]";

        return CreateWarningMessage(
            $"{location}: could not parse value for [{issue.PropertyName}] - left at default",
            MessageType.MAPPING);
    }
}
