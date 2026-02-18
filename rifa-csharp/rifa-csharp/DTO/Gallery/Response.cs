namespace rifa_csharp.DTO.Gallery;

public class GalleryResponse
{
    public string Message { get; set; }
    public GalleryMetadata Metadata { get; set; }
}

public class GalleryMetadata
{
    public GalleryDataDTO Data { get; set; }
}