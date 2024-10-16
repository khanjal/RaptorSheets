using FluentAssertions;
using RLE.Core.Extensions;
using RLE.Core.Tests.Data;
using RLE.Core.Utilities;
using Xunit;

namespace RLE.Core.Tests.Helpers;

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
