namespace rifa_csharp.DTO.Gallery;

public class CreateImageDTO
{
    public IFormFile Image { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}