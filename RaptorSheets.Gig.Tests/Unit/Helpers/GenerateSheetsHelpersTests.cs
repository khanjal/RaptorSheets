using RaptorSheets.Core.Managers;
using RaptorSheets.Gig.Helpers;
using Xunit;

namespace RaptorSheets.Gig.Tests.Unit.Helpers;

public class GenerateSheetsHelpersTests
{
    [Fact]
    public void Generate_WithTempSheetName_ReturnsBareAddSheetRequest()
    {
        // DeleteSheets' safety mechanism (GoogleSheetManagerBase<TEntity>.DeleteSheets) needs a bare
        // AddSheet request for this specific ad-hoc name, not a NotImplementedException.
        var result = GenerateSheetsHelpers.Generate([GoogleSheetManagerBase.TempSheetName]);

        Assert.NotEmpty(result.Requests);
        Assert.Contains(result.Requests, r => r.AddSheet?.Properties?.Title == GoogleSheetManagerBase.TempSheetName);
    }

    [Fact]
    public void Generate_WithUnknownSheetName_Throws()
    {
        Assert.Throws<NotImplementedException>(() => GenerateSheetsHelpers.Generate(["NotARealSheet"]));
    }

    [Fact]
    public void Generate_WithEmptyList_ReturnsEmptyRequest()
    {
        var result = GenerateSheetsHelpers.Generate([]);

        Assert.Empty(result.Requests);
    }
}
