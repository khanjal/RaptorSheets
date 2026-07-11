using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Gig.Enums;

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

                // Build a title->index map from the spreadsheet to allow CreateSheets to compute insertion indices
                var existingIndexMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                if (spreadsheet?.Sheets != null)
                {
                    for (int i = 0; i < spreadsheet.Sheets.Count; i++)
                    {
                        var s = spreadsheet.Sheets[i];
                        var title = s?.Properties?.Title;
                        if (!string.IsNullOrEmpty(title))
                        {
                            existingIndexMap[title] = s?.Properties?.Index ?? i;
                        }
                    }
                }

                messages.AddRange((await CreateSheets(missingSheets, existingIndexMap)).Messages);
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
