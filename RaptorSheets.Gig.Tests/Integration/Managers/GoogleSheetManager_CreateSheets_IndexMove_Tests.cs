using RaptorSheets.Core.Helpers;
using RaptorSheets.Gig.Constants;

namespace RaptorSheets.Gig.Tests.Integration.Managers;

public class GoogleSheetManager_CreateSheets_IndexMove_Tests
{
    [Fact]
    public void ComputeEndIndex_ReturnsAbsoluteEnd()
    {
        // Arrange
        var existing = 4;
        var adding = SheetsConfig.SheetUtilities.GetAllSheetNames().Count;

        // Act
        var endIndex = GoogleRequestHelpers.ComputeEndIndex(existing, adding);

        // Assert
        Assert.Equal(existing + adding, endIndex);
    }

    [Fact]
    public void GenerateUpdateSheetIndex_BuildsRequestWithIndexField()
    {
        // Arrange
        var sheetId = 42;
        var index = 10;

        // Act
        var request = GoogleRequestHelpers.GenerateUpdateSheetIndex(sheetId, index);

        // Assert
        Assert.NotNull(request.UpdateSheetProperties);
        Assert.Equal("index", request.UpdateSheetProperties.Fields);
        Assert.NotNull(request.UpdateSheetProperties.Properties);
        Assert.Equal(sheetId, request.UpdateSheetProperties.Properties.SheetId);
        Assert.Equal(index, request.UpdateSheetProperties.Properties.Index);
    }
}
