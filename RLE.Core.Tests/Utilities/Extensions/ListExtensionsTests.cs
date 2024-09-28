using FluentAssertions;
using RLE.Gig.Tests.Data;
using Xunit;

namespace RLE.Core.Tests.Utilities.Extensions;

public class ListExtensionsTests
{
    [Theory]
    [InlineData("A", 0)]
    [InlineData("B", 1)]
    public void GivenHeaders_ShouldAddColumnAndIndex(string column, int index)
    {
        var result = TestSheetData.GetModelData();

        result.Headers[index].Column.Should().Be(column);
        result.Headers[index].Index.Should().Be(index);
    }
}
