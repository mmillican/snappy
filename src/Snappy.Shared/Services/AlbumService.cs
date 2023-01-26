using Snappy.Shared.Config;
using Snappy.Shared.Models;

namespace Snappy.Shared.Services;

public class AlbumService : BaseDynamoService<Album>
{
    public AlbumService()
        : base(AWSEnvironment.DynamoTables.AlbumTableName)
    {
    }

    public AlbumService(string tableName) : base(tableName)
    {
    }

    public async Task<Album> FindAlbum(string slug)
        => await DbContext.LoadAsync<Album>(slug);

    public async Task<IEnumerable<Album>> GetAlbums()
    {
        var albums = new List<Album>();

        var scan = DbContext.ScanAsync<Album>(null);
        while (!scan.IsDone)
        {
            albums.AddRange(await scan.GetNextSetAsync());
        }

        return albums;
    }

    public async Task<Album> CreateAlbumIfNotExists(string slug)
    {
        var album = await FindAlbum(slug);
        if (album is not null)
        {
            return album;
        }

        var parentSlug = slug.Contains("/")
            ? slug.Substring(0, slug.LastIndexOf('/'))
            : null;

        var title = slug.Contains("/")
            ? slug.Substring(slug.LastIndexOf('/') + 1)
            : slug;

        album = new Album
        {
            Slug = slug,
            ParentSlug = parentSlug,
            Title = title,
            CreatedOn = DateTime.UtcNow, // TODO: Use a service for testing
            UpdatedOn = DateTime.UtcNow,
        };

        await Save(album);
        return album;
    }

    public async Task Save(Album album)
        => await DbContext.SaveAsync(album);
}
