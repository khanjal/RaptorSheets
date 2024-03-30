using FluentAssertions;
using GigRaptorLib.Mappers;

namespace GigRaptorLib.Tests.Mappers
{
    public class WeeklyMapperTests
    {
        [Fact]
        public void GivenGetCall_ThenReturnSheet()
        {
            var result = WeeklyMapper.GetSheet();

            result.CellColor.Should().BeDefined();
            result.FreezeColumnCount.Should().Be(1);
            result.FreezeRowCount.Should().Be(1);
            result.Name.Should().Be("Weekly");
            result.Headers.Count.Should().Be(19);
            result.ProtectSheet.Should().BeTrue();
            result.TabColor.Should().BeDefined();

            foreach (var header in result.Headers)
            {
                header.Column.Should().NotBeNullOrWhiteSpace();
                header.Formula.Should().NotBeNullOrWhiteSpace();
                header.Name.Should().NotBeNullOrWhiteSpace();
            }
        }
    }
}
