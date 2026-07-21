using RaptorSheets.Core.Managers;
using RaptorSheets.Stock.Helpers;
using Xunit;

namespace RaptorSheets.Stock.Tests.Unit.Helpers;

public class GenerateSheetHelpersTests
{
    [Fact]
    public void Generate_WithEmptyList_ReturnsEmptyRequest()
    {
        var result = GenerateSheetHelpers.Generate([]);
        Assert.NotNull(result.Requests);
        Assert.Empty(result.Requests);
    }

    [Fact]
    public void Generate_WithKnownSheet_ReturnsFullyConfiguredRequest()
    {
        var result = GenerateSheetHelpers.Generate(["Stocks"]);
        Assert.NotEmpty(result.Requests);
        Assert.Contains(result.Requests, r => r.AddSheet?.Properties?.Title == "Stocks");
        // A real domain sheet has headers, so formatting/protection requests are expected too.
        Assert.Contains(result.Requests, r => r.AddProtectedRange != null);
    }

    [Fact]
    public void Generate_WithTempSheetName_ReturnsBareAddSheetRequest()
    {
        // DeleteSheets' safety mechanism (GoogleSheetManagerBase<TEntity>.DeleteSheets) needs a bare
        // AddSheet request for this specific ad-hoc name, not a NotImplementedException.
        var result = GenerateSheetHelpers.Generate([GoogleSheetManagerBase.TempSheetName]);
        Assert.NotEmpty(result.Requests);
        Assert.Contains(result.Requests, r => r.AddSheet?.Properties?.Title == GoogleSheetManagerBase.TempSheetName);
    }

    [Fact]
    public void Generate_WithUnknownSheetName_Throws()
    {
        Assert.Throws<NotImplementedException>(() => GenerateSheetHelpers.Generate(["NotARealSheet"]));
    }
}
