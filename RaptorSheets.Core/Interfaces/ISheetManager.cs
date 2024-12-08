using RaptorSheets.Core.Entities;

namespace RaptorSheets.Core.Interfaces
{
    public interface ISheetManager
    {
        public Task<List<MessageEntity>> CheckSheets();
        public Task<List<MessageEntity>> CheckSheets(bool checkHeaders);
        public Task<List<MessageEntity>> CheckSheetHeaders(List<string> sheets);
        public Task<string?> GetSpreadsheetName();
    }
}
