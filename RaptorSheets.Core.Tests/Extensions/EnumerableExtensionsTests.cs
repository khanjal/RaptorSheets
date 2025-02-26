using RaptorSheets.Core.Extensions;
using Xunit;

namespace RaptorSheets.Core.Tests.Extensions;

public class EnumerableExtensionsTests
{
    [Fact]
    public void AddRange_ShouldAddItemsToCollection()
    {
        // Arrange
        IList<int> collection = [ 1, 2, 3 ];
        var itemsToAdd = new List<int> { 4, 5, 6 };

        // Act
        collection.AddRange(itemsToAdd);

        // Assert
        Assert.Equal(6, collection.Count);
        Assert.Contains(4, collection);
        Assert.Contains(5, collection);
        Assert.Contains(6, collection);
    }

    [Fact]
    public void AddRange_ShouldHandleEmptyItems()
    {
        // Arrange
        IList<int> collection = [ 1, 2, 3 ];
        var itemsToAdd = new List<int>();

        // Act
        collection.AddRange(itemsToAdd);

        // Assert
        Assert.Equal(3, collection.Count);
    }

    [Fact]
    public void AddRange_ShouldHandleEmptyCollection()
    {
        // Arrange
        IList<int> collection = [];
        var itemsToAdd = new List<int> { 1, 2, 3 };

        // Act
        collection.AddRange(itemsToAdd);

        // Assert
        Assert.Equal(3, collection.Count);
        Assert.Contains(1, collection);
        Assert.Contains(2, collection);
        Assert.Contains(3, collection);
    }
}
