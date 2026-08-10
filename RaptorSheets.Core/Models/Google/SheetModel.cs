using RaptorSheets.Core.Enums;

namespace RaptorSheets.Core.Models.Google;

public class SheetModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public List<SheetCellModel> Headers { get; set; } = [];
    public SheetColor CellColor { get; set; }
    public SheetColor FontColor { get; set; }
    public SheetColor TabColor { get; set; }
    public bool ProtectSheet { get; set; }
    public int FreezeColumnCount { get; set; }
    public int FreezeRowCount { get; set; }

    // Opt-in: gives the sheet a basic filter (sort/filter dropdown arrows on the header row)
    // covering the header plus every declared column. Not automatic - not every sheet wants one.
    public bool BasicFilter { get; set; }
}