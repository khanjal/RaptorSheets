using System.Collections.Generic;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Models.Google;
using Xunit;

namespace RaptorSheets.Core.Tests.Unit.Extensions;

public class EnsureHeaderPlaceholdersTests
{
    [Fact]
    public void KeepOne_PadsToOriginalCount()
    {
        var headers = new List<SheetCellModel>
        {
            new SheetCellModel { Name = "H1" },
            new SheetCellModel { Name = "H2" },
            new SheetCellModel { Name = "H3" }
        };

        headers.EnsureHeaderPlaceholders(1);

        Assert.Equal(3, headers.Count);
        Assert.Equal("H1", headers[0].Name);
        // After simplification we simply mark the trailing headers as hidden
        Assert.Equal("H2", headers[1].Name);
        Assert.True(headers[1].HideHeaderName);
        Assert.Equal("H3", headers[2].Name);
        Assert.True(headers[2].HideHeaderName);
        Assert.Equal("A", headers[0].Column);
        Assert.Equal("B", headers[1].Column);
        Assert.Equal("C", headers[2].Column);
        Assert.Equal(0, headers[0].Index);
        Assert.Equal(1, headers[1].Index);
        Assert.Equal(2, headers[2].Index);
    }

    [Fact]
    public void KeepTwo_PadsToOriginalCount()
    {
        var headers = new List<SheetCellModel>
        {
            new SheetCellModel { Name = "H1" },
            new SheetCellModel { Name = "H2" },
            new SheetCellModel { Name = "H3" },
            new SheetCellModel { Name = "H4" }
        };

        headers.EnsureHeaderPlaceholders(2);

        Assert.Equal(4, headers.Count);
        Assert.Equal("H1", headers[0].Name);
        Assert.Equal("H2", headers[1].Name);
        Assert.Equal("H3", headers[2].Name);
        Assert.True(headers[2].HideHeaderName);
        Assert.Equal("H4", headers[3].Name);
        Assert.True(headers[3].HideHeaderName);
    }

    [Fact]
    public void KeepMoreThanOriginal_DoesNotAdd()
    {
        var headers = new List<SheetCellModel>
        {
            new SheetCellModel { Name = "A" },
            new SheetCellModel { Name = "B" }
        };

        headers.EnsureHeaderPlaceholders(5);

        Assert.Equal(2, headers.Count);
        Assert.Equal("A", headers[0].Name);
        Assert.Equal("B", headers[1].Name);
    }

    [Fact]
    public void EmptyList_RemainsEmpty()
    {
        var headers = new List<SheetCellModel>();
        headers.EnsureHeaderPlaceholders(1);
        Assert.Empty(headers);
    }

    [Fact]
    public void PreserveExistingProperties()
    {
        var headers = new List<SheetCellModel>
        {
            new SheetCellModel { Name = "H1", Formula = "X" },
            new SheetCellModel { Name = "H2", Formula = "Y" },
            new SheetCellModel { Name = "H3", Formula = "Z" }
        };

        headers.EnsureHeaderPlaceholders(1);

        Assert.Equal("X", headers[0].Formula);
    }
}
