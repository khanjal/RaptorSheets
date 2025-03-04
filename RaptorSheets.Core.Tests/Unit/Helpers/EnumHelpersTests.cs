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

        Assert.NotNull(result);
        Assert.Equal(3, result.Count);

        Assert.Equal(HeaderEnum.FIRST_COLUMN.GetDescription(), result[0]);
        Assert.Equal(HeaderEnum.SECOND_COLUMN.GetDescription(), result[1]);
        Assert.Equal(HeaderEnum.THIRD_COLUMN.GetDescription(), result[2]);
    }
}