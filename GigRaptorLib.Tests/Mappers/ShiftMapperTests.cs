using FluentAssertions;
using GigRaptorLib.Mappers;

namespace GigRaptorLib.Tests.Mappers
{
    public class ShiftMapperTests
    {
        [Fact]
        public void GivenGetCall_ThenReturnSheet()
        {
            var result = ShiftMapper.GetSheet();

            result.CellColor.Should().BeDefined();
            result.FreezeColumnCount.Should().Be(1);
            result.FreezeRowCount.Should().Be(1);
            result.Name.Should().Be("Shifts");
            result.Headers.Count.Should().Be(33);
            result.ProtectSheet.Should().BeFalse();
            result.TabColor.Should().BeDefined();

            foreach (var header in result.Headers)
            {
                header.Column.Should().NotBeNullOrWhiteSpace();
                header.Name.Should().NotBeNullOrWhiteSpace();
            }
        }
    }
}
