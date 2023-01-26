using Snappy.Shared.Config;
using Snappy.Shared.Models;

namespace Snappy.Shared.Services;

public class PhotoService : BaseDynamoService<Photo>
{
    public PhotoService()
        : base(AWSEnvironment.DynamoTables.PhotoTableName)
    {
    }

    public PhotoService(string tableName) : base(tableName)
    {
    }

    public async Task<Photo> FindPhoto(Guid id)
        => await DbContext.LoadAsync<Photo>(id);

    public async Task<IEnumerable<Photo>> GetPhotos()
    {
        var albums = new List<Photo>();

        var scan = DbContext.ScanAsync<Photo>(null);
        while (!scan.IsDone)
        {
            albums.AddRange(await scan.GetNextSetAsync());
        }

        return albums;
    }

    public async Task Save(Photo photo)
        => await DbContext.SaveAsync(photo);
}
