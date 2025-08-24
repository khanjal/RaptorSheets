using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Helpers;
using RaptorSheets.Core.Tests.Data.Helpers;

namespace RaptorSheets.Gig.Tests.Unit.Helpers;

public class SheetHelperTests
{
    [Theory]
    [InlineData(0, "A")]
    [InlineData(26, "AA")]
    [InlineData(701, "ZZ")]
    public void GivenNumber_ThenReturnColumnLetter(int index, string column)
    {
        string result = SheetHelpers.GetColumnName(index);

        Assert.Equal(column, result);
    }

    [Theory]
    [InlineData(ColorEnum.BLACK)]
    [InlineData(ColorEnum.BLUE)]
    [InlineData(ColorEnum.CYAN)]
    [InlineData(ColorEnum.DARK_YELLOW)]
    [InlineData(ColorEnum.GREEN)]
    [InlineData(ColorEnum.LIGHT_CYAN)]
    [InlineData(ColorEnum.LIGHT_GRAY)]
    [InlineData(ColorEnum.LIGHT_GREEN)]
    [InlineData(ColorEnum.LIGHT_RED)]
    [InlineData(ColorEnum.LIGHT_YELLOW)]
    [InlineData(ColorEnum.LIME)]
    [InlineData(ColorEnum.ORANGE)]
    [InlineData(ColorEnum.MAGENTA)]
    [InlineData(ColorEnum.PINK)]
    [InlineData(ColorEnum.PURPLE)]
    [InlineData(ColorEnum.RED)]
    [InlineData(ColorEnum.WHITE)]
    [InlineData(ColorEnum.YELLOW)]
    public void GivenColorEnum_ThenReturnColor(ColorEnum color)
    {
        var result = SheetHelpers.GetColor(color);

        Assert.NotNull(result);
    }

    [Fact]
    public void GivenGetSheets_ThenReturnSheets()
    {
        var sheets = GigSheetHelpers.GetSheets();

        Assert.NotNull(sheets);
        Assert.Equal(2, sheets.Count);
        Assert.Equal("Shifts", sheets[0].Name);
        Assert.Equal("Trips", sheets[1].Name);
    }

    [Fact]
    public void GivenHeaders_ThenReturnHeadersList()
    {
        var headers = new List<SheetCellModel>();

        var headerFormula = new SheetCellModel { Formula = "Formula" };
        headers.Add(headerFormula);

        var headerName = new SheetCellModel { Name = "Name" };
        headers.Add(headerName);

        var headerList = SheetHelpers.HeadersToList(headers);

        Assert.NotNull(headerList);
        Assert.Equal(2, headerList[0].Count);
        Assert.Equal("Formula", headerList[0][0]);
        Assert.Equal("Name", headerList[0][1]);
    }

    [Theory]
    [InlineData(FormatEnum.ACCOUNTING, "NUMBER", true)]
    [InlineData(FormatEnum.DATE, "DATE", true)]
    [InlineData(FormatEnum.DISTANCE, "NUMBER", true)]
    [InlineData(FormatEnum.DURATION, "DATE", true)]
    [InlineData(FormatEnum.NUMBER, "NUMBER", true)]
    [InlineData(FormatEnum.TEXT, "TEXT", false)]
    [InlineData(FormatEnum.TIME, "DATE", true)]
    [InlineData(FormatEnum.WEEKDAY, "DATE", true)]
    public void GivenFormatHeader_ThenReturnCellFormat(FormatEnum format, string type, bool hasPattern)
    {
        var cellFormat = SheetHelpers.GetCellFormat(format);

        Assert.NotNull(cellFormat);
        Assert.Equal(type, cellFormat.NumberFormat.Type);

        if (hasPattern)
        {
            Assert.NotNull(cellFormat.NumberFormat.Pattern);
        }
        else
        {
            Assert.Null(cellFormat.NumberFormat.Pattern);
        }
    }

    [Fact]
    public void GivenSpreadsheet_ThenReturnProperties()
    {
        var spreadsheet = JsonHelpers.LoadDemoSpreadsheet();

        var spreadsheetTitle = SheetHelpers.GetSpreadsheetTitle(spreadsheet!);
        Assert.NotNull(spreadsheetTitle);
        Assert.Equal("Demo Raptor Gig Sheet", spreadsheetTitle);

        var spreadsheetSheets = SheetHelpers.GetSpreadsheetSheets(spreadsheet!);
        Assert.NotNull(spreadsheetSheets);
        Assert.Equal(Enum.GetNames(typeof(Enums.SheetEnum)).Length, spreadsheetSheets.Count);
    }
}