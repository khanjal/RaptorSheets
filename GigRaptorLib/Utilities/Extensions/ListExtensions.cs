using GigRaptorLib.Enums;
using GigRaptorLib.Models;

namespace GigRaptorLib.Utilities.Extensions
{
    public static class ListExtensions
    {
        public static void AddColumn(this List<SheetCellModel> headers, SheetCellModel header)
        {
            var column = SheetHelper.GetColumnName(headers.Count);
            header.Column = column;
            header.Index = headers.Count;
            headers.Add(header);
        }

        public static SheetCellModel GetHeader(this List<SheetCellModel> headers, HeaderEnum header)
        {
            return headers.FirstOrDefault(x => x.Name == HeaderEnum.TYPE.DisplayName());
        }
    }
}