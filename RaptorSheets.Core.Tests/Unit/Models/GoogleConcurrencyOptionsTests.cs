using RaptorSheets.Core.Models;
using Xunit;

namespace RaptorSheets.Core.Tests.Unit.Models;

public class GoogleConcurrencyOptionsTests
{
    [Fact]
    public void Default_ShouldBeUnlimited()
    {
        var options = GoogleConcurrencyOptions.Default;

        Assert.True(options.MaxConcurrentRequests <= 0);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(5)]
    public void MaxConcurrentRequests_ShouldAcceptAnyValue(int value)
    {
        var options = new GoogleConcurrencyOptions { MaxConcurrentRequests = value };

        Assert.Equal(value, options.MaxConcurrentRequests);
    }
}
