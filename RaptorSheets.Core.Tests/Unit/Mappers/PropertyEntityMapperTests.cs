using RaptorSheets.Core.Mappers;
using Xunit;
using File = Google.Apis.Drive.v3.Data.File;

namespace RaptorSheets.Core.Tests.Unit.Mappers;

public class PropertyEntityMapperTests
{
    #region Core Mapping Tests
    
    [Fact]
    public void MapFromDriveFile_WithValidFile_ShouldMapCorrectly()
    {
        // Arrange
        var file = new File
        {
            Id = "test-file-id",
            Name = "Test File Name"
        };

        // Act
        var result = PropertyEntityMapper.MapFromDriveFile(file);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(file.Id, result.Id);
        Assert.Equal(file.Name, result.Name);
        Assert.NotNull(result.Attributes);
        Assert.Empty(result.Attributes);
    }

    [Fact]
    public void MapFromDriveFile_WithNullFile_ShouldReturnEmptyEntity()
    {
        // Arrange
        File? file = null;

        // Act
        var result = PropertyEntityMapper.MapFromDriveFile(file);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("", result.Id);
        Assert.Equal("", result.Name);
        Assert.NotNull(result.Attributes);
        Assert.Empty(result.Attributes);
    }

    [Theory]
    [InlineData("file-123", "Document.docx")]
    [InlineData("spreadsheet-456", "Budget 2024.xlsx")]
    [InlineData("", "")]
    [InlineData(null, null)]
    public void MapFromDriveFile_WithVariousInputs_ShouldMapCorrectly(string? id, string? name)
    {
        // Arrange
        var file = new File
        {
            Id = id,
            Name = name
        };

        // Act
        var result = PropertyEntityMapper.MapFromDriveFile(file);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal(name, result.Name);
        Assert.NotNull(result.Attributes);
        Assert.Empty(result.Attributes);
    }
    
    #endregion

    #region Collection Mapping Tests
    
    [Fact]
    public void MapFromDriveFiles_WithValidFileList_ShouldMapAllFiles()
    {
        // Arrange
        var files = new List<File>
        {
            new File { Id = "file1", Name = "File One" },
            new File { Id = "file2", Name = "File Two" },
            new File { Id = "file3", Name = "File Three" }
        };

        // Act
        var result = PropertyEntityMapper.MapFromDriveFiles(files);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        
        Assert.Equal("file1", result[0].Id);
        Assert.Equal("File One", result[0].Name);
        
        Assert.Equal("file2", result[1].Id);
        Assert.Equal("File Two", result[1].Name);
        
        Assert.Equal("file3", result[2].Id);
        Assert.Equal("File Three", result[2].Name);
    }

    [Fact]
    public void MapFromDriveFiles_WithEmptyList_ShouldReturnEmptyList()
    {
        // Arrange
        var files = new List<File>();

        // Act
        var result = PropertyEntityMapper.MapFromDriveFiles(files);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void MapFromDriveFiles_WithNullList_ShouldReturnEmptyList()
    {
        // Arrange
        IList<File>? files = null;

        // Act
        var result = PropertyEntityMapper.MapFromDriveFiles(files!);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
    
    #endregion
}