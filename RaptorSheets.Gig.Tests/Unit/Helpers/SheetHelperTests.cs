using FluentAssertions;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Enums;
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

        result.Should().Be(column);
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

        result.Should().NotBeNull();
    }

    [Fact]
    public void GivenGetSheets_ThenReturnSheets()
    {
        var sheets = GigSheetHelpers.GetSheets();

        sheets.Should().NotBeNull();
        sheets.Count.Should().Be(2);
        sheets[0].Name.Should().BeEquivalentTo("Shifts");
        sheets[1].Name.Should().BeEquivalentTo("Trips");
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

        headerList.Should().NotBeNull();
        headerList[0].Count.Should().Be(2);
        headerList[0][0].Should().BeEquivalentTo("Formula");
        headerList[0][1].Should().BeEquivalentTo("Name");
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

        cellFormat.Should().NotBeNull();
        cellFormat.NumberFormat.Type.Should().BeEquivalentTo(type);

        if (hasPattern)
        {
            cellFormat.NumberFormat.Pattern.Should().NotBeNull();
        }
        else
        {
            cellFormat.NumberFormat.Pattern.Should().Be(null);
        }
    }

    [Fact]
    public void GivenSpreadsheet_ThenReturnProperties()
    {
        var spreadsheet = JsonHelpers.LoadDemoSpreadsheet();

        var spreadsheetTitle = SheetHelpers.GetSpreadsheetTitle(spreadsheet!);
        spreadsheetTitle.Should().NotBeNull();
        spreadsheetTitle.Should().BeEquivalentTo("Demo Raptor Gig Sheet");

        var spreadsheetSheets = SheetHelpers.GetSpreadsheetSheets(spreadsheet!);
        spreadsheetSheets.Should().NotBeNull();
        spreadsheetSheets.Count.Should().Be(Enum.GetNames(typeof(SheetEnum)).Length);
    }
}