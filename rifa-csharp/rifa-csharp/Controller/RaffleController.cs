using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rifa_csharp.DTO.Raffle;
using rifa_csharp.Entities;
using rifa_csharp.Service;

namespace rifa_csharp.Controller;

[ApiController]
[Route("raffle")]
[Produces("application/json")]
public class RaffleController : ControllerBase
{
    private readonly UnitOfService service;
    private readonly IMapper mapper;
    private readonly ILogger logger;

    public RaffleController(UnitOfService service, IMapper mapper,  ILogger<RaffleController> logger)
    {
        this.service = service;
        this.mapper = mapper;
        this.logger = logger;
    }

    [HttpGet]
    [Route("listAll")]
    public async Task<IActionResult> ListAll()
    {
        logger.LogError($"============= GET raffle/listAll =============");
        
        var items = await service.RaffleService.listAll();
        return items.IsSuccess ? Ok(items.Successes.FirstOrDefault()) : StatusCode(400, items.Errors.FirstOrDefault()!);
    }
    
    [HttpGet]
    [Route("listNamesRaffle")]
    public async Task<IActionResult> ListNamesRaffle()
    {
        logger.LogError($"============= GET raffle/listNamesRaffle =============");
        
        var items = await service.RaffleService.listNamesRaffle();
        return items.IsSuccess ? Ok(items.Successes.FirstOrDefault()) : StatusCode(400, items.Errors.FirstOrDefault()!);
    }

    [HttpGet]
    [Route("{id}")]
    public async Task<IActionResult> ListById(int id)
    {
        logger.LogError($"============= GET raffle/{id} =============");
        
        var item = await service.RaffleService.listById(id);
        return item.IsSuccess ? Ok(item.Successes.FirstOrDefault()) : StatusCode(404, item.Errors.FirstOrDefault()!);
    }
    
    [HttpGet]
    [Route("listParticipants/{raffleId}")]
    public async Task<IActionResult> listParticipantsRaffle(int raffleId)
    {
        logger.LogError($"============= GET raffle/listParticipants/{raffleId} =============");
        
        var result = await service.RaffleService.listParticipantsRaffle(raffleId);

        return result.IsSuccess
            ? Ok(result.Successes.FirstOrDefault())
            : StatusCode(400, result.Errors.FirstOrDefault());
    }

    [HttpGet]
    [Route("getReport")]
    [Authorize (Policy = "ADMIN_OPERATOR")]
    public async Task<IActionResult> GetReport()
    {
        logger.LogError($"============= GET raffle/getReport =============");
        
        var items = await service.RaffleService.GetRafflesReportAsync();
        return items.IsSuccess ? Ok(items.Successes.FirstOrDefault()) : StatusCode(400, items.Errors.FirstOrDefault()!);
    }

    [HttpPost]
    [Authorize (Policy = "ADMIN")]
    public async Task<IActionResult> addRaffle([FromBody] CreateRaffleDTO dto)
    {
        logger.LogError($"============= POST raffle =============");
        
        var createdRaffle = await service.RaffleService.createRaffle(dto);
        return createdRaffle.IsSuccess
            ? Created("Criado", createdRaffle.Successes.FirstOrDefault())
            : StatusCode(400, createdRaffle.Errors.FirstOrDefault()); 
    }
    
    [HttpPut]
    [Route("{id}")]
    [Authorize (Policy = "ADMIN")]
    public async Task<IActionResult> updateRaffle(int id, [FromBody] UpdateRaffleDTO dto)
    {
        logger.LogError($"============= PUT raffle/{id} =============");
        
        var updatedRaffle = await service.RaffleService.updateRaffle(id, dto);
        return updatedRaffle.IsSuccess
            ? Ok(updatedRaffle.Successes.FirstOrDefault())
            : StatusCode(400, updatedRaffle.Errors.FirstOrDefault()); 
    }

    [HttpPut]
    [Route("changeStatus/{raffleId}")]
    [Authorize (Policy = "ADMIN")]
    public async Task<IActionResult> changeStatus(int raffleId, [FromBody] ChangeRaffleStatusDTO dto)
    {
        logger.LogError($"============= PUT raffle/changeStatus/{raffleId} =============");
        
        var raffle = mapper.Map<Raffle>(dto);
        var newStatus = await service.RaffleService.ChangeRaffleStatus(raffleId, raffle);
        return newStatus.IsSuccess
            ? Created("Criado", newStatus.Successes.FirstOrDefault())
            : StatusCode(400, newStatus.Errors.FirstOrDefault()); 
    }

    [HttpDelete]
    [Route("{raffleId}")]
    [Authorize (Policy = "ADMIN")]
    public async Task<IActionResult> deleteRaffle(int raffleId)
    {
        logger.LogError($"============= DELETE raffle/{raffleId} =============");
        
        var items = await service.RaffleService.deleteRaffle(raffleId);
        return items.IsSuccess ? Ok(items.Successes.FirstOrDefault()) : StatusCode(400, items.Errors.FirstOrDefault()!);
    }
}