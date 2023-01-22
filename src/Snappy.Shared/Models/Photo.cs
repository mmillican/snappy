namespace Snappy.Shared.Models;

public class Photo
{
	public Guid Id { get; set; } = Guid.NewGuid();

	public string AlbumSlug { get; set; }

	public string FileName { get; set; }
	public string SavedFiledName { get; set; }

	public string Title { get; set; }
	public string Description { get; set; }

	public DateTime CreatedOn { get; set; }
	public DateTime UpdatedOn { get; set; }

	public DateTime? CaptureDate { get; set; }

	public Dictionary<string, string> Metadata { get; set; } = new();

    public string[] Tags { get; set; }
}

public class Album
{
    // I don't think albums need an Id?... ('Slug' can be the primary key)

    public string Slug { get; set; }
    public string ParentSlug { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

	public DateTime CreatedOn { get; set; }
	public DateTime UpdatedOn { get; set; }

    public DateTime? LastPhotoDate { get; set; }
}
