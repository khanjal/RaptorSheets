using RaptorSheets.Core.Entities;
using File = Google.Apis.Drive.v3.Data.File;

namespace RaptorSheets.Core.Mappers;

public static class PropertyEntityMapper
{
    public static PropertyEntity MapFromDriveFile(File? file)
    {
        if (file == null) return new PropertyEntity();

        return new PropertyEntity
        {
            Id = file.Id,
            Name = file.Name
            // You can map additional fields to Attributes if needed, e.g.:
            // Attributes = new Dictionary<string, string>
            // {
            //     { "MimeType", file.MimeType },
            //     { "CreatedTime", file.CreatedTimeRaw }
            // }
        };
    }

    public static IList<PropertyEntity> MapFromDriveFiles(IList<File> files)
    {
        return files?.Select(MapFromDriveFile).ToList() ?? new List<PropertyEntity>();
    }
}
