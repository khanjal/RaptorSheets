using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Entities;

namespace RaptorSheets.Core.Interfaces;

public interface ISheetManager
{
    public Task<List<MessageEntity>> CheckSheets();
    public Task<List<MessageEntity>> CheckSheets(bool checkHeaders);
    public List<MessageEntity> CheckSheetHeaders(Spreadsheet sheetInfoResponse);
    public Task<string?> GetSpreadsheetName();
}
