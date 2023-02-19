using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Snappy.Shared.Config;
using Snappy.Shared.Images;
using Snappy.Shared.Models;
using Snappy.Shared.Services;

namespace Snappy.Api.Controllers;

[ApiController]
[Route("/photos")]
public class PhotosController : ControllerBase
{
    private readonly IPhotoService _photoService;
    private readonly AwsConfig _awsConfig;

    public PhotosController(IPhotoService photoService,
        IOptions<AwsConfig> awsOptions)
    {
        _photoService = photoService;
        _awsConfig = awsOptions.Value;
    }

    [HttpGet("")]
    public async Task<ActionResult<IEnumerable<Photo>>> Get(string albumSlug)
    {
        var photos = await _photoService.GetPhotosForAlbum(albumSlug);

        var models = photos.Select(ToModel);
        return Ok(models);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Photo>> GetById(string albumSlug, Guid id)
    {
        var photo = await _photoService.FindPhoto(albumSlug, id);
        if (photo is null)
        {
            return NotFound();
        }

        var model = ToModel(photo);

        return Ok(model);
    }

    private PhotoModel ToModel(Photo photo)
    {
        var model = new PhotoModel(photo);
        var sizes = ImageHelper.GetImageSizes();

        foreach(var size in sizes)
        {
            model.Sizes.Add(size.Key, ResolveImageUrl(photo.AlbumSlug, photo.SavedFileName, size.Key));
        }
        model.Sizes.Add("full",ResolveImageUrl(photo.AlbumSlug, photo.SavedFileName));

        return model;
    }

    private string ResolveImageUrl(string album, string filename, string? sizeKey = null)
    {
        filename = ImageHelper.GetResizedFileName(filename, sizeKey);

        return $"https://{_awsConfig.StorageBucketName}.s3.amazonaws.com/{album}/{filename}";
    }

    public class PhotoModel : Photo
    {
        public Dictionary<string, string> Sizes { get; set; } = new();

        public PhotoModel(Photo photo)
        {
            Id = photo.Id;
            AlbumSlug = photo.AlbumSlug;
            FileName = photo.FileName;
            SavedFileName = photo.SavedFileName;
            Title = photo.Title;
            Description = photo.Description;
            CreatedOn = photo.CreatedOn;
            UpdatedOn = photo.UpdatedOn;
            CaptureDate = photo.CaptureDate;
            Metadata = photo.Metadata;
            Tags = photo.Tags;
        }
    }
}
