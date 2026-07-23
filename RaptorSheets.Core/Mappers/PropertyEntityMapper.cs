using RaptorSheets.Core.Entities;
using File = Google.Apis.Drive.v3.Data.File;

namespace RaptorSheets.Core.Mappers;

public static class PropertyEntityMapper
{
    /// <summary>
    /// Map a Google Drive <see cref="File"/> to a <see cref="PropertyEntity"/>.
    /// </summary>
    /// <remarks>
    /// Additional file metadata can be placed into the returned entity's <see cref="PropertyEntity.Attributes"/> dictionary.
    /// Example usage:
    /// <code>
    /// <![CDATA[
    /// var entity = new PropertyEntity
    /// {
    ///     Id = file.Id,
    ///     Name = file.Name,
    ///     Attributes = new Dictionary<string,string>
    ///     {
    ///         { "MimeType", file.MimeType },
    ///         { "CreatedTime", file.CreatedTimeRaw }
    ///     }
    /// };
    /// ]]>
    /// </code>
    /// </remarks>
    public static PropertyEntity MapFromDriveFile(File? file)
    {
        if (file == null) return new PropertyEntity();

        return new PropertyEntity
        {
            Id = file.Id,
            Name = file.Name
        };
    }

    public static IList<PropertyEntity> MapFromDriveFiles(IList<File> files)
    {
        return files?.Select(MapFromDriveFile).ToList() ?? new List<PropertyEntity>();
    }
}
