using RaptorSheets.Core.Extensions;
using Xunit;

namespace RaptorSheets.Core.Tests.Unit.Extensions;

public class EnumerableExtensionsTests
{
    [Fact]
    public void AddRange_ShouldAddItemsToCollection()
    {
        // Arrange
        IList<int> collection = [1, 2, 3];
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
        IList<int> collection = [1, 2, 3];
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

    [Fact]
    public void AddRange_WithNullItems_ShouldThrow()
    {
        // Arrange
        IList<int> collection = [1, 2, 3];

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => collection.AddRange(null!));
    }

    [Fact]
    public void AddRange_WithNullCollection_ShouldThrow()
    {
        // Arrange
        IList<int>? collection = null;
        var itemsToAdd = new List<int> { 1, 2, 3 };

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => collection!.AddRange(itemsToAdd));
    }

    [Fact]
    public void AddRange_WithDuplicateItems_ShouldAddAll()
    {
        // Arrange
        IList<int> collection = [1, 2, 3];
        var itemsToAdd = new List<int> { 2, 3, 4 };

        // Act
        collection.AddRange(itemsToAdd);

        // Assert
        Assert.Equal(6, collection.Count);
        Assert.Equal(2, collection.Count(x => x == 2));
        Assert.Equal(2, collection.Count(x => x == 3));
    }

    [Fact]
    public void AddRange_WithLargeCollection_ShouldMaintainOrder()
    {
        // Arrange
        IList<int> collection = [];
        var itemsToAdd = Enumerable.Range(1, 1000).ToList();

        // Act
        collection.AddRange(itemsToAdd);

        // Assert
        Assert.Equal(1000, collection.Count);
        Assert.Equal(1, collection.First());
        Assert.Equal(1000, collection.Last());
        Assert.True(collection.SequenceEqual(itemsToAdd));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public void AddRange_WithVariousItemCounts_ShouldWorkCorrectly(int itemCount)
    {
        // Arrange
        IList<int> collection = [1, 2, 3];
        var itemsToAdd = Enumerable.Range(4, itemCount).ToList();

        // Act
        collection.AddRange(itemsToAdd);

        // Assert
        Assert.Equal(3 + itemCount, collection.Count);
        if (itemCount > 0)
        {
            Assert.Contains(4, collection);
            Assert.Contains(3 + itemCount, collection);
        }
    }
}
