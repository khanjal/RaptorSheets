using FluentAssertions;
using GigRaptorLib.Enums;
using GigRaptorLib.Models;
using GigRaptorLib.Utilities.Extensions;

namespace GigRaptorLib.Tests.Utilities.Extensions
{
    public class ListExtensionsTests
    {
        [Theory]
        [InlineData("A", 0)]
        [InlineData("B", 1)]
        public void GivenHeaders_AddColumnAndIndex(string column, int index)
        {
            var result = GetModelData();

            result.Headers[index].Column.Should().Be(column);
            result.Headers[index].Index.Should().Be(index);
        }


        private static SheetModel GetModelData()
        {
            var sheet = new SheetModel
            {
                Headers = []
            };

            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = "Test",
                Formula = "Formula",
                Format = FormatEnum.TEXT
            });

            sheet.Headers.AddColumn(new SheetCellModel
            {
                Name = "Second",
                Formula = "None",
                Format = FormatEnum.NUMBER
            });


            return sheet;
        }
    }
}
