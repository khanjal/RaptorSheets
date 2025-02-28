using FluentAssertions;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Tests.Data;
using Xunit;

namespace RaptorSheets.Core.Tests.Unit.Helpers;

public class EnumHelpersTests
{
    [Fact]
    public void GivenEnumWithDescriptions_ThenReturnDescriptions()
    {
        var result = EnumHelpers.GetListOfDescription<HeaderEnum>();

        result.Should().NotBeNull();
        result.Should().HaveCount(3);

        result![0].Should().BeEquivalentTo(HeaderEnum.FIRST_COLUMN.GetDescription());
        result![1].Should().BeEquivalentTo(HeaderEnum.SECOND_COLUMN.GetDescription());
        result![2].Should().BeEquivalentTo(HeaderEnum.THIRD_COLUMN.GetDescription());
    }
}
