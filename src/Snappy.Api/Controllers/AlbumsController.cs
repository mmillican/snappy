using Microsoft.AspNetCore.Mvc;
using Snappy.Shared.Models;
using Snappy.Shared.Services;

namespace Snappy.Api.Controllers;

[ApiController]
[Route("albums")]
public class AlbumsController : ControllerBase
{
    private readonly IAlbumService _albumService;

    public AlbumsController(IAlbumService albumService)
    {
        _albumService = albumService;
    }

    [HttpGet("")]
    public async Task<ActionResult<IEnumerable<Album>>> Get()
    {
        var albums = await _albumService.GetAlbums();
        return Ok(albums);
    }

    [HttpGet("{*slug}")]
    public async Task<ActionResult<Album>> GetBySlug(string slug)
    {
        slug = slug.Replace("%2F", "/"); // hack

        var album = await _albumService.FindAlbum(slug);
        if (album is null)
        {
            return NotFound();
        }
        return Ok(album);
    }
}
