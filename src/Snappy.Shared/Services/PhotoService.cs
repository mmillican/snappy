using Amazon.DynamoDBv2.DocumentModel;
using Snappy.Shared.Config;
using Snappy.Shared.Models;

namespace Snappy.Shared.Services;

public interface IPhotoService
{
    Task<Photo> FindPhoto(string albumSlug, Guid id);
    Task<IEnumerable<Photo>> GetPhotosForAlbum(string album);
    Task Save(Photo photo);
}

public class PhotoService : BaseDynamoService<Photo>, IPhotoService
{
    public PhotoService()
        : base(AWSEnvironment.DynamoTables.PhotoTableName)
    {
    }

    public PhotoService(string tableName) : base(tableName)
    {
    }

    public async Task<Photo> FindPhoto(string albumSlug, Guid id)
        => await DbContext.LoadAsync<Photo>(albumSlug, id);

    public async Task<IEnumerable<Photo>> GetPhotosForAlbum(string album)
    {
        var photos = new List<Photo>();


        var query = DbContext.FromQueryAsync<Photo>(new QueryOperationConfig
        {
            KeyExpression = new()
            {
                ExpressionStatement = "AlbumSlug = :album_slug",
                ExpressionAttributeValues = new()
                {
                    { ":album_slug", album },
                },
            },
        });

        while (!query.IsDone)
        {
            photos.AddRange(await query.GetNextSetAsync());
        }

        return photos;
    }

    public async Task Save(Photo photo)
        => await DbContext.SaveAsync(photo);
}
