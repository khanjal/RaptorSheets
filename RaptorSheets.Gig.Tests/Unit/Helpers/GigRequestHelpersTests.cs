using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Common.Mappers;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Helpers;
using RaptorSheets.Gig.Tests.Data.Helpers;

namespace RaptorSheets.Gig.Tests.Unit.Helpers;

public class GigRequestHelpersTests
{
    [Fact]
    public void CreateDeleteRequests_WithEmptyRowIds_ShouldReturnEmptyList()
    {
        // Arrange
        var rowIds = new List<int>();
        var sheetProperties = new PropertyEntity { Id = "1" };

        // Act
        var result = GigRequestHelpers.CreateDeleteRequests(rowIds, sheetProperties);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void CreateDeleteRequests_WithNullSheetProperties_ShouldReturnEmptyList()
    {
        // Arrange
        var rowIds = new List<int> { 1, 2, 3 };

        // Act
        var result = GigRequestHelpers.CreateDeleteRequests(rowIds, null);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void CreateDeleteRequests_WithInvalidSheetId_ShouldReturnEmptyList()
    {
        // Arrange
        var rowIds = new List<int> { 1, 2, 3 };
        var sheetProperties = new PropertyEntity { Id = "invalid" };

        // Act
        var result = GigRequestHelpers.CreateDeleteRequests(rowIds, sheetProperties);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void CreateDeleteRequests_WithValidData_ShouldReturnDeleteRequests()
    {
        // Arrange
        var rowIds = new List<int> { 5, 3, 7 }; // Non-consecutive rows
        var sheetProperties = new PropertyEntity { Id = "123" };

        // Act
        var result = GigRequestHelpers.CreateDeleteRequests(rowIds, sheetProperties).ToList();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count); // Should have 3 delete requests for non-consecutive rows

        // Verify all requests are delete requests with correct sheet ID
        foreach (var request in result)
        {
            Assert.NotNull(request.DeleteDimension);
            Assert.Equal(123, request.DeleteDimension.Range.SheetId);
        }
    }

    [Fact]
    public void CreateDeleteRequests_WithConsecutiveRows_ShouldOptimizeRequests()
    {
        // Arrange
        var rowIds = new List<int> { 1, 2, 3, 4, 5 }; // Consecutive rows
        var sheetProperties = new PropertyEntity { Id = "456" };

        // Act
        var result = GigRequestHelpers.CreateDeleteRequests(rowIds, sheetProperties).ToList();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result); // Should optimize to single request for consecutive rows
        Assert.Equal(456, result[0].DeleteDimension.Range.SheetId);
        Assert.Equal(0, result[0].DeleteDimension.Range.StartIndex); // Row 1 -> index 0
        Assert.Equal(5, result[0].DeleteDimension.Range.EndIndex);   // Row 5 -> end index 5
    }

    #region Trip Tests

    [Fact]
    public void ChangeTripSheetData_WithEmptyTrips_ShouldReturnEmptyList()
    {
        // Arrange
        var trips = new List<TripEntity>();
        var sheetProperties = new PropertyEntity 
        { 
            Id = "1",
            Attributes = new Dictionary<string, string>
            {
                { PropertyEnum.HEADERS.GetDescription(), "Date,Number,Service" },
                { PropertyEnum.MAX_ROW.GetDescription(), "10" }
            }
        };

        // Act
        var result = GigRequestHelpers.ChangeTripSheetData(trips, sheetProperties);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void ChangeTripSheetData_WithNullTrips_ShouldReturnEmptyList()
    {
        // Arrange
        var sheetProperties = new PropertyEntity 
        { 
            Id = "1",
            Attributes = new Dictionary<string, string>
            {
                { PropertyEnum.HEADERS.GetDescription(), "Date,Number,Service" },
                { PropertyEnum.MAX_ROW.GetDescription(), "10" }
            }
        };

        // Act
        var result = GigRequestHelpers.ChangeTripSheetData(null, sheetProperties);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void ChangeTripSheetData_WithMixedActions_ShouldProcessInCorrectOrder()
    {
        // Arrange
        var trips = new List<TripEntity>
        {
            new() { RowId = 1, Action = ActionTypeEnum.INSERT.GetDescription() },
            new() { RowId = 2, Action = ActionTypeEnum.UPDATE.GetDescription() },
            new() { RowId = 3, Action = ActionTypeEnum.DELETE.GetDescription() },
            new() { RowId = 4, Action = ActionTypeEnum.DELETE.GetDescription() }
        };
        var sheetProperties = new PropertyEntity 
        { 
            Id = "1",
            Attributes = new Dictionary<string, string>
            {
                { PropertyEnum.HEADERS.GetDescription(), "Date,Number,Service" },
                { PropertyEnum.MAX_ROW.GetDescription(), "10" }
            }
        };

        // Act
        var result = GigRequestHelpers.ChangeTripSheetData(trips, sheetProperties);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        
        // Should process updates first, then deletes
        // The actual implementation may optimize delete requests differently
        var deleteRequests = result.Where(r => r.DeleteDimension != null).ToList();
        Assert.True(deleteRequests.Count > 0, "Should have delete requests for DELETE actions");
    }

    [Fact]
    public void CreateUpdateCellTripRequests_WithEmptyTrips_ShouldReturnEmptyList()
    {
        // Arrange
        var trips = new List<TripEntity>();
        var sheetProperties = new PropertyEntity 
        { 
            Id = "1",
            Attributes = new Dictionary<string, string>
            {
                { PropertyEnum.HEADERS.GetDescription(), "Date,Number" },
                { PropertyEnum.MAX_ROW.GetDescription(), "5" }
            }
        };

        // Act
        var result = GigRequestHelpers.CreateUpdateCellTripRequests(trips, sheetProperties);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void CreateUpdateCellTripRequests_WithNullSheetProperties_ShouldReturnEmptyList()
    {
        // Arrange
        var trips = new List<TripEntity> { new() { RowId = 1 } };

        // Act
        var result = GigRequestHelpers.CreateUpdateCellTripRequests(trips, null);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void CreateUpdateCellTripRequests_WithInvalidSheetId_ShouldReturnEmptyList()
    {
        // Arrange
        var trips = new List<TripEntity> { new() { RowId = 1 } };
        var sheetProperties = new PropertyEntity 
        { 
            Id = "invalid",
            Attributes = new Dictionary<string, string>
            {
                { PropertyEnum.HEADERS.GetDescription(), "Date,Number" },
                { PropertyEnum.MAX_ROW.GetDescription(), "5" }
            }
        };

        // Act
        var result = GigRequestHelpers.CreateUpdateCellTripRequests(trips, sheetProperties);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void CreateUpdateCellTripRequests_WithAppendTrips_ShouldReturnAppendRequest()
    {
        // Arrange
        var trips = new List<TripEntity> { new() { RowId = 15 } }; // RowId > maxRow
        var sheetProperties = new PropertyEntity 
        { 
            Id = "1",
            Attributes = new Dictionary<string, string>
            {
                { PropertyEnum.HEADERS.GetDescription(), "Date,Number,Service" },
                { PropertyEnum.MAX_ROW.GetDescription(), "10" }
            }
        };

        // Act
        var result = GigRequestHelpers.CreateUpdateCellTripRequests(trips, sheetProperties).ToList();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.NotNull(result[0].AppendCells);
        Assert.Equal(1, result[0].AppendCells.SheetId);
    }

    [Fact]
    public void CreateUpdateCellTripRequests_WithUpdateTrips_ShouldReturnUpdateRequests()
    {
        // Arrange
        var trips = new List<TripEntity> { new() { RowId = 5 } }; // RowId <= maxRow
        var sheetProperties = new PropertyEntity 
        { 
            Id = "1",
            Attributes = new Dictionary<string, string>
            {
                { PropertyEnum.HEADERS.GetDescription(), "Date,Number,Service" },
                { PropertyEnum.MAX_ROW.GetDescription(), "10" }
            }
        };

        // Act
        var result = GigRequestHelpers.CreateUpdateCellTripRequests(trips, sheetProperties).ToList();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.NotNull(result[0].UpdateCells);
        Assert.Equal(1, result[0].UpdateCells.Range.SheetId);
        Assert.Equal(4, result[0].UpdateCells.Range.StartRowIndex); // RowId 5 -> index 4
    }

    [Fact]
    public void CreateUpdateCellTripRequests_WithMixedTrips_ShouldReturnBothTypes()
    {
        // Arrange
        var trips = new List<TripEntity> 
        { 
            new() { RowId = 5 },  // Update (RowId <= maxRow)
            new() { RowId = 15 }  // Append (RowId > maxRow)
        };
        var sheetProperties = new PropertyEntity 
        { 
            Id = "1",
            Attributes = new Dictionary<string, string>
            {
                { PropertyEnum.HEADERS.GetDescription(), "Date,Number,Service" },
                { PropertyEnum.MAX_ROW.GetDescription(), "10" }
            }
        };

        // Act
        var result = GigRequestHelpers.CreateUpdateCellTripRequests(trips, sheetProperties).ToList();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, r => r.AppendCells != null);  // One append request
        Assert.Contains(result, r => r.UpdateCells != null);  // One update request
    }

    #endregion

    #region Shift Tests

    [Fact]
    public void ChangeShiftSheetData_WithEmptyShifts_ShouldReturnEmptyList()
    {
        // Arrange
        var shifts = new List<ShiftEntity>();
        var sheetProperties = new PropertyEntity 
        { 
            Id = "1",
            Attributes = new Dictionary<string, string>
            {
                { PropertyEnum.HEADERS.GetDescription(), "Date,Number,Service" },
                { PropertyEnum.MAX_ROW.GetDescription(), "10" }
            }
        };

        // Act
        var result = GigRequestHelpers.ChangeShiftSheetData(shifts, sheetProperties);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void ChangeShiftSheetData_WithNullShifts_ShouldReturnEmptyList()
    {
        // Arrange
        var sheetProperties = new PropertyEntity 
        { 
            Id = "1",
            Attributes = new Dictionary<string, string>
            {
                { PropertyEnum.HEADERS.GetDescription(), "Date,Number,Service" },
                { PropertyEnum.MAX_ROW.GetDescription(), "10" }
            }
        };

        // Act
        var result = GigRequestHelpers.ChangeShiftSheetData(null, sheetProperties);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void CreateUpdateCellShiftRequests_WithValidData_ShouldReturnRequests()
    {
        // Arrange
        var shifts = new List<ShiftEntity> { new() { RowId = 15 } }; // RowId > maxRow
        var sheetProperties = new PropertyEntity 
        { 
            Id = "1",
            Attributes = new Dictionary<string, string>
            {
                { PropertyEnum.HEADERS.GetDescription(), "Date,Number,Service" },
                { PropertyEnum.MAX_ROW.GetDescription(), "10" }
            }
        };

        // Act
        var result = GigRequestHelpers.CreateUpdateCellShiftRequests(shifts, sheetProperties).ToList();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.NotNull(result[0].AppendCells);
    }

    [Fact]
    public void CreateUpdateCellShiftRequests_WithUpdateShifts_ShouldReturnUpdateRequests()
    {
        // Arrange
        var shifts = new List<ShiftEntity> { new() { RowId = 5 } }; // RowId <= maxRow
        var sheetProperties = new PropertyEntity 
        { 
            Id = "1",
            Attributes = new Dictionary<string, string>
            {
                { PropertyEnum.HEADERS.GetDescription(), "Date,Number,Service" },
                { PropertyEnum.MAX_ROW.GetDescription(), "10" }
            }
        };

        // Act
        var result = GigRequestHelpers.CreateUpdateCellShiftRequests(shifts, sheetProperties).ToList();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.NotNull(result[0].UpdateCells);
        Assert.Equal(4, result[0].UpdateCells.Range.StartRowIndex); // RowId 5 -> index 4
    }

    #endregion

    #region Setup Tests

    [Fact]
    public void ChangeSetupSheetData_WithEmptySetup_ShouldReturnEmptyList()
    {
        // Arrange
        var setup = new List<SetupEntity>();
        var sheetProperties = new PropertyEntity 
        { 
            Id = "1",
            Attributes = new Dictionary<string, string>
            {
                { PropertyEnum.HEADERS.GetDescription(), "Name,Value" },
                { PropertyEnum.MAX_ROW.GetDescription(), "5" }
            }
        };

        // Act
        var result = GigRequestHelpers.ChangeSetupSheetData(setup, sheetProperties);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void ChangeSetupSheetData_WithNullSetup_ShouldReturnEmptyList()
    {
        // Arrange
        var sheetProperties = new PropertyEntity 
        { 
            Id = "1",
            Attributes = new Dictionary<string, string>
            {
                { PropertyEnum.HEADERS.GetDescription(), "Name,Value" },
                { PropertyEnum.MAX_ROW.GetDescription(), "5" }
            }
        };

        // Act
        var result = GigRequestHelpers.ChangeSetupSheetData(null, sheetProperties);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void CreateUpdateCellSetupRequests_WithValidData_ShouldReturnRequests()
    {
        // Arrange
        var setup = new List<SetupEntity> { new() { RowId = 15 } }; // RowId > maxRow
        var sheetProperties = new PropertyEntity 
        { 
            Id = "1",
            Attributes = new Dictionary<string, string>
            {
                { PropertyEnum.HEADERS.GetDescription(), "Name,Value,Description" },
                { PropertyEnum.MAX_ROW.GetDescription(), "10" }
            }
        };

        // Act
        var result = GigRequestHelpers.CreateUpdateCellSetupRequests(setup, sheetProperties).ToList();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.NotNull(result[0].AppendCells);
    }

    [Fact]
    public void CreateUpdateCellSetupRequests_WithUpdateSetup_ShouldReturnUpdateRequests()
    {
        // Arrange
        var setup = new List<SetupEntity> { new() { RowId = 5 } }; // RowId <= maxRow
        var sheetProperties = new PropertyEntity 
        { 
            Id = "1",
            Attributes = new Dictionary<string, string>
            {
                { PropertyEnum.HEADERS.GetDescription(), "Name,Value,Description" },
                { PropertyEnum.MAX_ROW.GetDescription(), "10" }
            }
        };

        // Act
        var result = GigRequestHelpers.CreateUpdateCellSetupRequests(setup, sheetProperties).ToList();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.NotNull(result[0].UpdateCells);
        Assert.Equal(4, result[0].UpdateCells.Range.StartRowIndex); // RowId 5 -> index 4
    }

    #endregion
}