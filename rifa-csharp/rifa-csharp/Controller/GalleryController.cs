using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rifa_csharp.DTO.Gallery;
using rifa_csharp.Service;

namespace rifa_csharp.Controller;

[ApiController]
[Route("gallery")]
public class GalleryController : ControllerBase
{
    private readonly UnitOfService service;
    private readonly IMapper mapper;
    private readonly ILogger logger;

    public GalleryController(UnitOfService service, IMapper mapper, ILogger<GalleryController> logger)
    {
        this.service = service;
        this.mapper = mapper;
        this.logger = logger;
    }

    [HttpGet]
    [Route("{id}")]
    public async Task<IActionResult> ListById(int id)
    {
        logger.LogError($"============= GET gallery/{id} =============");

        var item = await service.GalleryService.FindById(id);
        return item.IsSuccess ? Ok(item.Successes.FirstOrDefault()) : StatusCode(404, item.Errors.FirstOrDefault()!);
    }
    
    [HttpPost]
    public async Task<IActionResult> createGallery([FromForm] CreateImageDTO dto)
    {
        logger.LogError($"============= POST gallery =============");

        var result = await service.GalleryService.Create(dto.Image, dto.Name, dto.Description);
    
        if (result.IsSuccess)
        {
            var response = result.Successes.FirstOrDefault()?.Metadata["response"];
            return Created("Criado", response);
        }
        else
        {
            return StatusCode(400, result.Errors.FirstOrDefault()?.Message);
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "ADMIN")]
    public async Task<IActionResult> DeleteGallery(int id)
    {
        var item = await service.GalleryService.DeleteGalleryAsync(id);
        return item.IsSuccess ? Ok(item.Successes.FirstOrDefault()) : StatusCode(404, item.Errors.FirstOrDefault()!);
    }
}