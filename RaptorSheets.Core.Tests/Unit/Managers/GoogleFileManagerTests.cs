using Moq;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Managers;
using RaptorSheets.Core.Services;
using Xunit;

namespace RaptorSheets.Core.Tests.Unit.Managers;

public class GoogleFileManagerTests
{
    private readonly Mock<IGoogleDriveService> _mockGoogleDriveService;
    private readonly GoogleFileManager _manager;

    public GoogleFileManagerTests()
    {
        _mockGoogleDriveService = new Mock<IGoogleDriveService>();
        _manager = new GoogleFileManager(_mockGoogleDriveService.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidService_ShouldInitialize()
    {
        // Arrange
        var mockService = new Mock<IGoogleDriveService>();

        // Act
        var manager = new GoogleFileManager(mockService.Object);

        // Assert
        Assert.NotNull(manager);
    }

    [Fact]
    public void Constructor_WithNullService_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new GoogleFileManager((IGoogleDriveService)null!));
    }

    [Fact]
    public void Constructor_WithValidAccessToken_ShouldInitialize()
    {
        // Arrange
        var accessToken = "test-access-token";

        // Act
        var manager = new GoogleFileManager(accessToken);

        // Assert
        Assert.NotNull(manager);
    }

    [Theory]
    [InlineData("valid-token-123")]
    [InlineData("Bearer token")]
    [InlineData("a")]
    public void Constructor_WithVariousAccessTokens_ShouldInitialize(string accessToken)
    {
        // Act & Assert - Should not throw exception
        var manager = new GoogleFileManager(accessToken);
        Assert.NotNull(manager);
    }

    [Fact]
    public void Constructor_WithNullAccessToken_ShouldThrowArgumentNullException()
    {
        // Arrange
        string accessToken = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new GoogleFileManager(accessToken));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptyOrWhitespaceAccessToken_ShouldInitialize(string accessToken)
    {
        // Act & Assert - Should not throw exception for empty/whitespace strings
        var manager = new GoogleFileManager(accessToken);
        Assert.NotNull(manager);
    }

    #endregion

    #region CreateFile Tests

    [Fact]
    public async Task CreateFile_WithValidName_ShouldCallServiceAndReturnResult()
    {
        // Arrange
        var fileName = "Test Spreadsheet";
        var expectedEntity = new PropertyEntity { Id = "123", Name = fileName };
        
        _mockGoogleDriveService
            .Setup(x => x.CreateSpreadsheet(fileName))
            .ReturnsAsync(expectedEntity);

        // Act
        var result = await _manager.CreateFile(fileName);

        // Assert
        Assert.Equal(expectedEntity, result);
        Assert.Equal(expectedEntity.Id, result.Id);
        Assert.Equal(expectedEntity.Name, result.Name);
        _mockGoogleDriveService.Verify(x => x.CreateSpreadsheet(fileName), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("My Budget Spreadsheet")]
    [InlineData("Spreadsheet with special chars: !@#$%")]
    [InlineData("File with émojis ?? and unicode")]
    public async Task CreateFile_WithVariousNames_ShouldPassThroughToService(string fileName)
    {
        // Arrange
        var expectedEntity = new PropertyEntity { Id = "123", Name = fileName };
        
        _mockGoogleDriveService
            .Setup(x => x.CreateSpreadsheet(fileName))
            .ReturnsAsync(expectedEntity);

        // Act
        var result = await _manager.CreateFile(fileName);

        // Assert
        Assert.Equal(expectedEntity, result);
        Assert.Equal(fileName, result.Name);
        _mockGoogleDriveService.Verify(x => x.CreateSpreadsheet(fileName), Times.Once);
    }

    [Fact]
    public async Task CreateFile_WithNullName_ShouldPassThroughToService()
    {
        // Arrange
        var expectedEntity = new PropertyEntity { Id = "123", Name = "" };
        
        _mockGoogleDriveService
            .Setup(x => x.CreateSpreadsheet(null!))
            .ReturnsAsync(expectedEntity);

        // Act
        var result = await _manager.CreateFile(null!);

        // Assert
        Assert.Equal(expectedEntity, result);
        _mockGoogleDriveService.Verify(x => x.CreateSpreadsheet(null!), Times.Once);
    }

    [Fact]
    public async Task CreateFile_WhenServiceThrowsException_ShouldPropagateException()
    {
        // Arrange
        var fileName = "Test File";
        var expectedException = new InvalidOperationException("Service error");
        
        _mockGoogleDriveService
            .Setup(x => x.CreateSpreadsheet(fileName))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _manager.CreateFile(fileName));
        
        Assert.Equal(expectedException.Message, exception.Message);
        _mockGoogleDriveService.Verify(x => x.CreateSpreadsheet(fileName), Times.Once);
    }

    [Fact]
    public async Task CreateFile_WhenServiceThrowsArgumentException_ShouldPropagateException()
    {
        // Arrange
        var fileName = "Invalid File Name";
        var expectedException = new ArgumentException("Invalid file name", nameof(fileName));
        
        _mockGoogleDriveService
            .Setup(x => x.CreateSpreadsheet(fileName))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _manager.CreateFile(fileName));
        
        Assert.Equal(expectedException.Message, exception.Message);
        _mockGoogleDriveService.Verify(x => x.CreateSpreadsheet(fileName), Times.Once);
    }

    [Fact]
    public async Task CreateFile_WithLongFileName_ShouldPassThroughToService()
    {
        // Arrange
        var fileName = new string('a', 1000); // Very long file name
        var expectedEntity = new PropertyEntity { Id = "123", Name = fileName };
        
        _mockGoogleDriveService
            .Setup(x => x.CreateSpreadsheet(fileName))
            .ReturnsAsync(expectedEntity);

        // Act
        var result = await _manager.CreateFile(fileName);

        // Assert
        Assert.Equal(expectedEntity, result);
        _mockGoogleDriveService.Verify(x => x.CreateSpreadsheet(fileName), Times.Once);
    }

    #endregion

    #region GetFiles Tests

    [Fact]
    public async Task GetFiles_ShouldCallServiceAndReturnConvertedList()
    {
        // Arrange
        var serviceResult = new List<PropertyEntity>
        {
            new PropertyEntity { Id = "1", Name = "File 1" },
            new PropertyEntity { Id = "2", Name = "File 2" }
        };
        
        _mockGoogleDriveService
            .Setup(x => x.GetSpreadsheets())
            .ReturnsAsync(serviceResult);

        // Act
        var result = await _manager.GetFiles();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal(serviceResult[0].Id, result[0].Id);
        Assert.Equal(serviceResult[0].Name, result[0].Name);
        Assert.Equal(serviceResult[1].Id, result[1].Id);
        Assert.Equal(serviceResult[1].Name, result[1].Name);
        _mockGoogleDriveService.Verify(x => x.GetSpreadsheets(), Times.Once);
    }

    [Fact]
    public async Task GetFiles_WithEmptyServiceResult_ShouldReturnEmptyList()
    {
        // Arrange
        var serviceResult = new List<PropertyEntity>();
        
        _mockGoogleDriveService
            .Setup(x => x.GetSpreadsheets())
            .ReturnsAsync(serviceResult);

        // Act
        var result = await _manager.GetFiles();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        _mockGoogleDriveService.Verify(x => x.GetSpreadsheets(), Times.Once);
    }

    [Fact]
    public async Task GetFiles_WithSingleFile_ShouldReturnSingleElementList()
    {
        // Arrange
        var serviceResult = new List<PropertyEntity>
        {
            new PropertyEntity { Id = "1", Name = "Only File" }
        };
        
        _mockGoogleDriveService
            .Setup(x => x.GetSpreadsheets())
            .ReturnsAsync(serviceResult);

        // Act
        var result = await _manager.GetFiles();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("1", result[0].Id);
        Assert.Equal("Only File", result[0].Name);
        _mockGoogleDriveService.Verify(x => x.GetSpreadsheets(), Times.Once);
    }

    [Fact]
    public async Task GetFiles_WhenServiceThrowsException_ShouldPropagateException()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Service error");
        
        _mockGoogleDriveService
            .Setup(x => x.GetSpreadsheets())
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _manager.GetFiles());
        
        Assert.Equal(expectedException.Message, exception.Message);
        _mockGoogleDriveService.Verify(x => x.GetSpreadsheets(), Times.Once);
    }

    [Fact]
    public async Task GetFiles_WithNullServiceResult_ShouldReturnEmptyList()
    {
        // Arrange
        _mockGoogleDriveService
            .Setup(x => x.GetSpreadsheets())
            .ReturnsAsync((IList<PropertyEntity>)null!);

        // Act
        var result = await _manager.GetFiles();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        _mockGoogleDriveService.Verify(x => x.GetSpreadsheets(), Times.Once);
    }

    [Fact]
    public async Task GetFiles_WithLargeResultSet_ShouldHandleCorrectly()
    {
        // Arrange
        var serviceResult = new List<PropertyEntity>();
        for (int i = 0; i < 1000; i++)
        {
            serviceResult.Add(new PropertyEntity { Id = i.ToString(), Name = $"File {i}" });
        }
        
        _mockGoogleDriveService
            .Setup(x => x.GetSpreadsheets())
            .ReturnsAsync(serviceResult);

        // Act
        var result = await _manager.GetFiles();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1000, result.Count);
        Assert.Equal("0", result[0].Id);
        Assert.Equal("File 0", result[0].Name);
        Assert.Equal("999", result[999].Id);
        Assert.Equal("File 999", result[999].Name);
        _mockGoogleDriveService.Verify(x => x.GetSpreadsheets(), Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(100)]
    public async Task GetFiles_WithVariousResultSetSizes_ShouldHandleCorrectly(int fileCount)
    {
        // Arrange
        var serviceResult = new List<PropertyEntity>();
        for (int i = 0; i < fileCount; i++)
        {
            serviceResult.Add(new PropertyEntity { Id = i.ToString(), Name = $"File {i}" });
        }
        
        _mockGoogleDriveService
            .Setup(x => x.GetSpreadsheets())
            .ReturnsAsync(serviceResult);

        // Act
        var result = await _manager.GetFiles();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(fileCount, result.Count);
        
        if (fileCount > 0)
        {
            Assert.Equal("0", result[0].Id);
            Assert.Equal($"{fileCount - 1}", result[fileCount - 1].Id);
        }
        
        _mockGoogleDriveService.Verify(x => x.GetSpreadsheets(), Times.Once);
    }

    [Fact]
    public async Task GetFiles_WithFilesHavingSpecialCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        var serviceResult = new List<PropertyEntity>
        {
            new PropertyEntity { Id = "1", Name = "File with émojis ??" },
            new PropertyEntity { Id = "2", Name = "File with \"quotes\" and 'apostrophes'" },
            new PropertyEntity { Id = "3", Name = "File with\nnewlines\tand\ttabs" }
        };
        
        _mockGoogleDriveService
            .Setup(x => x.GetSpreadsheets())
            .ReturnsAsync(serviceResult);

        // Act
        var result = await _manager.GetFiles();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.Equal("File with émojis ??", result[0].Name);
        Assert.Equal("File with \"quotes\" and 'apostrophes'", result[1].Name);
        Assert.Equal("File with\nnewlines\tand\ttabs", result[2].Name);
        _mockGoogleDriveService.Verify(x => x.GetSpreadsheets(), Times.Once);
    }

    [Fact]
    public async Task GetFiles_ShouldPreserveOrderFromService()
    {
        // Arrange
        var serviceResult = new List<PropertyEntity>
        {
            new PropertyEntity { Id = "z", Name = "Z File" },
            new PropertyEntity { Id = "a", Name = "A File" },
            new PropertyEntity { Id = "m", Name = "M File" }
        };
        
        _mockGoogleDriveService
            .Setup(x => x.GetSpreadsheets())
            .ReturnsAsync(serviceResult);

        // Act
        var result = await _manager.GetFiles();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        
        // Verify order is preserved (not alphabetically sorted)
        Assert.Equal("z", result[0].Id);
        Assert.Equal("a", result[1].Id);
        Assert.Equal("m", result[2].Id);
        _mockGoogleDriveService.Verify(x => x.GetSpreadsheets(), Times.Once);
    }

    #endregion

    #region Interface Implementation Tests

    [Fact]
    public void GoogleFileManager_ImplementsIGoogleFileManager()
    {
        // Assert
        Assert.IsAssignableFrom<IGoogleFileManager>(_manager);
    }

    [Fact]
    public void IGoogleFileManager_HasCorrectMethods()
    {
        // Act
        var interfaceType = typeof(IGoogleFileManager);
        var methods = interfaceType.GetMethods();

        // Assert
        Assert.Equal(2, methods.Length);
        Assert.Contains(methods, m => m.Name == "CreateFile" && m.ReturnType == typeof(Task<PropertyEntity>));
        Assert.Contains(methods, m => m.Name == "GetFiles" && m.ReturnType == typeof(Task<List<PropertyEntity>>));
    }

    #endregion
}