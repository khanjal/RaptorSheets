using RaptorSheets.Core.Factories;
using RaptorSheets.Core.Models.Google;
using Xunit;

namespace RaptorSheets.Core.Tests.Unit.Factories;

public class SheetModelFactoryTests
{
    [Fact]
    public void CreateSheet_WithValidSheet_ReturnsSheet()
    {
        var model = new SheetModel { Name = "Test", Headers = new List<SheetCellModel> { new() { Name = "Header1" } } };
        var result = SheetModelFactory.CreateSheet(() => model, "Test");
        Assert.NotNull(result);
        Assert.Equal("Test", result.Name);
    }

    [Fact]
    public void CreateSheet_WithEmptyName_ReturnsNull()
    {
        var model = new SheetModel { Name = "", Headers = new List<SheetCellModel> { new() { Name = "Header1" } } };
        var result = SheetModelFactory.CreateSheet(() => model, "Test");
        Assert.Null(result);
    }

    [Fact]
    public void CreateSheet_WithNoHeaders_ReturnsNull()
    {
        var model = new SheetModel { Name = "Test", Headers = new List<SheetCellModel>() };
        var result = SheetModelFactory.CreateSheet(() => model, "Test");
        Assert.Null(result);
    }

    [Fact]
    public void CreateSheet_WithException_ReturnsNull()
    {
        var result = SheetModelFactory.CreateSheet(() => throw new Exception("fail"), "Test");
        Assert.Null(result);
    }

    [Fact]
    public void CreateSheets_WithMultipleCreators_ReturnsValidSheets()
    {
        var creators = new Dictionary<string, Func<SheetModel>>
        {
            ["A"] = () => new SheetModel { Name = "A", Headers = new List<SheetCellModel> { new() { Name = "H" } } },
            ["B"] = () => new SheetModel { Name = "B", Headers = new List<SheetCellModel> { new() { Name = "H" } } },
            ["C"] = () => new SheetModel { Name = "", Headers = new List<SheetCellModel>() } // Invalid
        };
        var result = SheetModelFactory.CreateSheets(creators);
        Assert.Equal(2, result.Count);
        Assert.Contains("A", result.Keys);
        Assert.Contains("B", result.Keys);
        Assert.DoesNotContain("C", result.Keys);
    }

    [Fact]
    public void ValidateSheetModel_WithValidSheet_ReturnsEmptyList()
    {
        var model = new SheetModel { Name = "Test", Headers = new List<SheetCellModel> { new() { Name = "Header1" } } };
        var result = SheetModelFactory.ValidateSheetModel(model);
        Assert.Empty(result);
    }

    [Fact]
    public void ValidateSheetModel_WithEmptyNameOrHeaders_ReturnsErrors()
    {
        var model1 = new SheetModel { Name = "", Headers = new List<SheetCellModel> { new() { Name = "Header1" } } };
        var model2 = new SheetModel { Name = "Test", Headers = new List<SheetCellModel>() };
        var result1 = SheetModelFactory.ValidateSheetModel(model1);
        var result2 = SheetModelFactory.ValidateSheetModel(model2);
        Assert.Contains(result1, e => e.Contains("name cannot be empty"));
        Assert.Contains(result2, e => e.Contains("at least one header"));
    }

    [Fact]
    public void ValidateSheetModel_WithDuplicateHeaders_ReturnsError()
    {
        var model = new SheetModel
        {
            Name = "Test",
            Headers = new List<SheetCellModel>
            {
                new() { Name = "A" },
                new() { Name = "A" },
                new() { Name = "B" }
            }
        };
        var result = SheetModelFactory.ValidateSheetModel(model);
        Assert.Contains(result, e => e.Contains("Duplicate header names"));
    }
}
