using FluentAssertions;
using GigRaptorLib.Enums;
using GigRaptorLib.Utilities;

namespace GigRaptorLib.Tests.Utilities
{
    public class SheetHelperTests
    {
        [Theory]
        [InlineData(0,"A")]
        [InlineData(26, "AA")]
        [InlineData(701, "ZZ")]
        public void GivenNumber_ThenReturnColumnLetter(int index, string column)
        {
            string result = SheetHelper.GetColumnName(index);

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
            var result = SheetHelper.GetColor(color);

            result.Should().NotBeNull();
        }
    }
}