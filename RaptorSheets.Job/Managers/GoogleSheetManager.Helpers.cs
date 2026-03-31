using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Job.Helpers;

namespace RaptorSheets.Job.Managers;

/// <summary>
/// Helper methods for the GoogleSheetManager.
/// Private methods used across the manager partial classes.
/// </summary>
public partial class GoogleSheetManager
{
    private async Task<List<MessageEntity>> HandleMissingSheets(Spreadsheet? spreadsheetInfo)
    {
        var messages = new List<MessageEntity>();

        if (spreadsheetInfo == null)
        {
            messages.Add(MessageHelpers.CreateErrorMessage("Unable to retrieve spreadsheet info", MessageTypeEnum.GET_SHEETS));
            return messages;
        }

        var missingSheets = JobSheetHelpers.GetMissingSheets(spreadsheetInfo);

        if (missingSheets.Count > 0)
        {
            messages.Add(MessageHelpers.CreateWarningMessage(
                $"Missing sheets: {string.Join(", ", missingSheets.Select(s => s.Name))}",
                MessageTypeEnum.GET_SHEETS));
        }
        else
        {
            messages.Add(MessageHelpers.CreateInfoMessage("All sheets present", MessageTypeEnum.GET_SHEETS));
        }

        return messages;
    }
}
