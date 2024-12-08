
using Google.Apis.Sheets.v4.Data;

namespace RaptorSheets.Core.Models.Google;

public class RepeatCellModel
{
    public GridRange GridRange { get; set; } = new GridRange();
    public CellFormat? CellFormat { get; set; }
    public DataValidationRule? DataValidation { get; set; }
}
