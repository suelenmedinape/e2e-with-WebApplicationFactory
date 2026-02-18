using AutoMapper;
using FluentResults;
using rifa_csharp.DTO.Gallery;
using rifa_csharp.Entities;
using rifa_csharp.Interface;

namespace rifa_csharp.Service;

public class GalleryService
{
    private readonly IUnitOfWork unit;
    private readonly IMapper mapper;
    private readonly SupabaseService supabaseService;

    public GalleryService(IUnitOfWork unit,  IMapper mapper, SupabaseService supabaseService)
    {
        this.unit = unit;
        this.mapper = mapper;
        this.supabaseService = supabaseService;
    }

    public async Task<Result> FindById(int id)
    {
        try
        {
            var item = await unit.RaffleRepository.findById(id) ?? throw new Exception("Rifa não encontrada");
            return new Result().WithSuccess(new Success("Sucesso ao buscar rifas").WithMetadata("data", item));
        }
        catch (Exception e)
        {
            return new Result().WithError(new Error(e.Message));
        }
    }

    public async Task<Result> Create(IFormFile image, string name, string description)
    {
        try
        {
            if (supabaseService == null)
                throw new Exception("supabaseService não foi injetado");
        
            var imageUrl = await supabaseService.UploadImageAsync(image);
        
            var gallery = new Gallery
            {
                Name = name,
                Description = description,
                ImageUrl = imageUrl
            };
        
            unit.GalleryRepository.Add(gallery);
            await unit.CommitAsync();

            var dto = mapper.Map<GalleryDataDTO>(gallery);
        
            // Cria o wrapper customizado
            var response = new GalleryResponse
            {
                Message = "Imagem adicionada com sucesso!",
                Metadata = new GalleryMetadata { Data = dto }
            };
        
            return new Result()
                .WithSuccess(
                    new Success("Imagem adicionada com sucesso!")
                        .WithMetadata("response", response)
                );
        }
        catch (Exception e)
        {
            return new Result().WithError(new Error(e.Message));
        }
    }
    
    public async Task<Result> DeleteGalleryAsync(int id)
    {
        try
        {
            var gallery = await unit.GalleryRepository.findById(id);

            if (gallery == null)
                throw new Exception("Galeria não encontrada");

            await supabaseService.DeleteImageAsync(gallery.ImageUrl);

            unit.GalleryRepository.Delete(gallery);
            await unit.CommitAsync();

            return new Result()
                .WithSuccess(new Success("Imagem deletada com sucesso!"));
        }
        catch (Exception e)
        {
            return new Result().WithError(new Error(e.Message));
        }
    }
}