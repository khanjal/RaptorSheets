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
        var result = EnumHelpers.GetListOfDescription<Header>();

        Assert.NotNull(result);
        Assert.Equal(3, result.Count);

        Assert.Equal(Header.FIRST_COLUMN.GetDescription(), result[0]);
        Assert.Equal(Header.SECOND_COLUMN.GetDescription(), result[1]);
        Assert.Equal(Header.THIRD_COLUMN.GetDescription(), result[2]);
    }
}