using RaptorSheets.Core.Mappers;
using Xunit;
using File = Google.Apis.Drive.v3.Data.File;

namespace RaptorSheets.Core.Tests.Unit.Mappers;

public class PropertyEntityMapperTests
{
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

    [Fact]
    public void MapFromDriveFile_WithEmptyFileProperties_ShouldMapEmptyValues()
    {
        // Arrange
        var file = new File
        {
            Id = "",
            Name = ""
        };

        // Act
        var result = PropertyEntityMapper.MapFromDriveFile(file);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("", result.Id);
        Assert.Equal("", result.Name);
        Assert.NotNull(result.Attributes);
        Assert.Empty(result.Attributes);
    }

    [Fact]
    public void MapFromDriveFile_WithNullFileProperties_ShouldMapNullValues()
    {
        // Arrange
        var file = new File
        {
            Id = null,
            Name = null
        };

        // Act
        var result = PropertyEntityMapper.MapFromDriveFile(file);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Id);
        Assert.Null(result.Name);
        Assert.NotNull(result.Attributes);
        Assert.Empty(result.Attributes);
    }

    [Theory]
    [InlineData("file-123", "Document.docx")]
    [InlineData("spreadsheet-456", "Budget 2024.xlsx")]
    [InlineData("image-789", "photo with spaces.jpg")]
    [InlineData("a", "b")]
    public void MapFromDriveFile_WithVariousInputs_ShouldMapCorrectly(string id, string name)
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
    }

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

    [Fact]
    public void MapFromDriveFiles_WithMixedValidAndNullFiles_ShouldMapAllFiles()
    {
        // Arrange
        var files = new List<File>
        {
            new File { Id = "file1", Name = "Valid File" },
            null!, // This will be handled by MapFromDriveFile method
            new File { Id = "file2", Name = "Another Valid File" }
        };

        // Act
        var result = PropertyEntityMapper.MapFromDriveFiles(files);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        
        // First file should map correctly
        Assert.Equal("file1", result[0].Id);
        Assert.Equal("Valid File", result[0].Name);
        
        // Null file should create entity with default values
        Assert.Equal("", result[1].Id);
        Assert.Equal("", result[1].Name);
        
        // Third file should map correctly
        Assert.Equal("file2", result[2].Id);
        Assert.Equal("Another Valid File", result[2].Name);
    }

    [Fact]
    public void MapFromDriveFiles_WithSingleFile_ShouldReturnSingleElementList()
    {
        // Arrange
        var files = new List<File>
        {
            new File { Id = "single-file", Name = "Only File" }
        };

        // Act
        var result = PropertyEntityMapper.MapFromDriveFiles(files);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("single-file", result[0].Id);
        Assert.Equal("Only File", result[0].Name);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(100)]
    public void MapFromDriveFiles_WithLargeNumberOfFiles_ShouldHandleCorrectly(int fileCount)
    {
        // Arrange
        var files = new List<File>();
        for (int i = 0; i < fileCount; i++)
        {
            files.Add(new File { Id = $"file-{i}", Name = $"File {i}" });
        }

        // Act
        var result = PropertyEntityMapper.MapFromDriveFiles(files);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(fileCount, result.Count);
        
        // Verify first and last elements
        Assert.Equal("file-0", result[0].Id);
        Assert.Equal("File 0", result[0].Name);
        Assert.Equal($"file-{fileCount - 1}", result[fileCount - 1].Id);
        Assert.Equal($"File {fileCount - 1}", result[fileCount - 1].Name);
    }

    [Fact]
    public void MapFromDriveFiles_PreservesOrder_ShouldMaintainOriginalSequence()
    {
        // Arrange
        var files = new List<File>
        {
            new File { Id = "z-file", Name = "Z File" },
            new File { Id = "a-file", Name = "A File" },
            new File { Id = "m-file", Name = "M File" }
        };

        // Act
        var result = PropertyEntityMapper.MapFromDriveFiles(files);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        
        // Verify order is preserved (not alphabetically sorted)
        Assert.Equal("z-file", result[0].Id);
        Assert.Equal("a-file", result[1].Id);
        Assert.Equal("m-file", result[2].Id);
    }

    [Fact]
    public void MapFromDriveFiles_WithFilesHavingSpecialCharacters_ShouldMapCorrectly()
    {
        // Arrange
        var files = new List<File>
        {
            new File { Id = "file-with-unicode-©", Name = "File with émojis 🚀 and spëcial chars" },
            new File { Id = "file/with\\slashes", Name = "File with \"quotes\" and 'apostrophes'" },
            new File { Id = "file\nwith\tspecial\r\nchars", Name = "File\nwith\ttabs\r\nand newlines" }
        };

        // Act
        var result = PropertyEntityMapper.MapFromDriveFiles(files);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        
        Assert.Equal("file-with-unicode-©", result[0].Id);
        Assert.Equal("File with émojis 🚀 and spëcial chars", result[0].Name);
        
        Assert.Equal("file/with\\slashes", result[1].Id);
        Assert.Equal("File with \"quotes\" and 'apostrophes'", result[1].Name);
        
        Assert.Equal("file\nwith\tspecial\r\nchars", result[2].Id);
        Assert.Equal("File\nwith\ttabs\r\nand newlines", result[2].Name);
    }
}