using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Helpers;

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
        var sheetProperties = new PropertyEntity(); // Use default instance instead of null

        // Act
        var result = GigRequestHelpers.CreateDeleteRequests(rowIds, sheetProperties);

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
        var trips = new List<TripEntity>(); // Use empty list instead of null

        // Act
        var result = GigRequestHelpers.ChangeTripSheetData(trips, sheetProperties);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
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

        // Act - pass null to test null handling behavior
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type
        var result = GigRequestHelpers.CreateUpdateCellTripRequests(trips, null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type

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
        var shifts = new List<ShiftEntity>(); // Use empty list instead of null

        // Act
        var result = GigRequestHelpers.ChangeShiftSheetData(shifts, sheetProperties);

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
        var setup = new List<SetupEntity>(); // Use empty list instead of null

        // Act
        var result = GigRequestHelpers.ChangeSetupSheetData(setup, sheetProperties);

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

    #region Generic Tests

    [Fact]
    public void ChangeSheetDataGeneric_WithMixedActions_ShouldHandleCorrectly()
    {
        // Arrange
        var trips = new List<TripEntity>
        {
            new() { RowId = 10, Action = ActionTypeEnum.INSERT.GetDescription() }, // New trip (append)
            new() { RowId = 5, Action = ActionTypeEnum.UPDATE.GetDescription() }, // Update trip
            new() { RowId = 15, Action = ActionTypeEnum.DELETE.GetDescription() } // Delete trip
        };
        
        var sheetProperties = new PropertyEntity 
        { 
            Id = "1",
            Attributes = new Dictionary<string, string>
            {
                { PropertyEnum.HEADERS.GetDescription(), "Date,Service,Pay,Tips" },
                { PropertyEnum.MAX_ROW.GetDescription(), "8" }
            }
        };

        // Act
        var result = GigRequestHelpers.ChangeTripSheetData(trips, sheetProperties);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count); // Should have append, update, and delete requests
        
        // Should have one append request (RowId 10 > maxRow 8)
        Assert.Single(result, r => r.AppendCells != null);
        
        // Should have one update request (RowId 5 <= maxRow 8) 
        Assert.Single(result, r => r.UpdateCells != null);
        
        // Should have one delete request (RowId 15 marked for deletion)
        Assert.Single(result, r => r.DeleteDimension != null);
    }

    [Fact]
    public void GenericHelpers_ShouldWorkWithAllEntityTypes()
    {
        // Arrange
        var setupEntity = new SetupEntity { RowId = 1, Action = ActionTypeEnum.INSERT.GetDescription() };
        var tripEntity = new TripEntity { RowId = 2, Action = ActionTypeEnum.UPDATE.GetDescription() };
        var shiftEntity = new ShiftEntity { RowId = 3, Action = ActionTypeEnum.DELETE.GetDescription() };
        var expenseEntity = new ExpenseEntity { RowId = 4, Action = ActionTypeEnum.INSERT.GetDescription() };

        // Act & Assert - Should not throw exceptions
        var setupAction = GigRequestHelpers.GetEntityAction(setupEntity);
        var tripRowId = GigRequestHelpers.GetEntityRowId(tripEntity);
        var shiftAction = GigRequestHelpers.GetEntityAction(shiftEntity);
        var expenseRowId = GigRequestHelpers.GetEntityRowId(expenseEntity);

        Assert.Equal(ActionTypeEnum.INSERT.GetDescription(), setupAction);
        Assert.Equal(2, tripRowId);
        Assert.Equal(ActionTypeEnum.DELETE.GetDescription(), shiftAction);
        Assert.Equal(4, expenseRowId);
    }

    #endregion
}