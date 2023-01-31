using Microsoft.AspNetCore.Mvc;
using Snappy.Shared.Models;
using Snappy.Shared.Services;

namespace Snappy.Api.Controllers;

[ApiController]
[Route("/albums/{albumSlug}/photos")]
public class PhotosController : ControllerBase
{
    private readonly IPhotoService _photoService;

    public PhotosController(IPhotoService photoService)
    {
        _photoService = photoService;
    }

    [HttpGet("")]
    public async Task<ActionResult<IEnumerable<Photo>>> Get(string albumSlug)
    {
        var photos = await _photoService.GetPhotosForAlbum(albumSlug);
        return Ok(photos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Photo>> GetById(string albumSlug, Guid id)
    {
        var photo = await _photoService.FindPhoto(albumSlug, id);
        if (photo is null)
        {
            return NotFound();
        }

        return Ok(photo);
    }
}
