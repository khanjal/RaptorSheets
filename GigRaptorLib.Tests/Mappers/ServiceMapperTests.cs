using FluentAssertions;
using GigRaptorLib.Mappers;

namespace GigRaptorLib.Tests.Mappers
{
    public class ServiceMapperTests
    {
        [Fact]
        public void GivenGetCall_ThenReturnSheet()
        {
            var result = ServiceMapper.GetSheet();

            result.CellColor.Should().BeDefined();
            result.FreezeColumnCount.Should().Be(1);
            result.FreezeRowCount.Should().Be(1);
            result.Name.Should().Be("Services");
            result.Headers.Count.Should().Be(12);
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
