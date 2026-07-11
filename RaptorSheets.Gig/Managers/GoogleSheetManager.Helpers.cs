using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Helpers;

namespace RaptorSheets.Gig.Managers;

/// <summary>
/// Private helper methods for Google Sheet Manager.
/// Internal utilities and support methods.
/// </summary>
public partial class GoogleSheetManager
{
    #region Private Helpers

    private async Task<List<MessageEntity>> HandleMissingSheets(Spreadsheet? spreadsheet)
    {
        var messages = new List<MessageEntity>();
        if (spreadsheet != null)
        {
            var missingSheets = SheetHelpers.CheckSheets<SheetEnum>(spreadsheet);

            if (missingSheets.Count != 0)
            {
                messages.AddRange(SheetHelpers.CheckSheets(missingSheets));

                // Compute a title->desiredIndex map for missing sheets using the canonical ordered sheet list.
                // This ensures insertion indices are computed relative to the full expected ordering,
                // not just the missing subset (avoids incorrectly appending sheets).
                var allSheets = GenerateSheetsHelpers.GetSheetNames();
                var missingIndexMap = SheetInitializationHelper.GetMissingSheets(spreadsheet, allSheets);

                messages.AddRange((await CreateSheets(missingIndexMap)).Messages);
            }
        }
        else
        {
            messages.Add(MessageHelpers.CreateErrorMessage($"Unable to retrieve sheet(s)", MessageTypeEnum.GET_SHEETS));
        }

        return messages;
    }

    #endregion
}
