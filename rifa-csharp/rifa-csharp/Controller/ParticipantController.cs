using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rifa_csharp.DTO.Participant;
using rifa_csharp.Entities;
using rifa_csharp.Service;

namespace rifa_csharp.Controller;

[ApiController]
[Route("participant")]
public class ParticipantController : ControllerBase
{
    private readonly UnitOfService service;
    private readonly IMapper mapper;
    private readonly ILogger logger;

    public ParticipantController(UnitOfService service, IMapper mapper,  ILogger<ParticipantController> logger)
    {
        this.service = service;
        this.mapper = mapper;
        this.logger = logger;
    }
    
    [HttpGet]
    [Route("{id}")]
    public async Task<IActionResult> ListById(int id)
    {
        logger.LogError($"============= GET participant/{id} =============");
        
        var item = await service.ParticipantService.listById(id);
        return item.IsSuccess ? Ok(item.Successes.FirstOrDefault()) : StatusCode(404, item.Errors.FirstOrDefault()!);
    }
    
    [HttpGet("by-phone/{raffleId}")]
    public async Task<IActionResult> findByPhone(
        [FromQuery] string phone,
        int raffleId)
    {
        logger.LogError($"============= GET participant/by-phone/{raffleId} =============");
        
        var result = await service.ParticipantService.searchByPhone(raffleId, phone);

        return result.IsSuccess
            ? Ok(result.Successes.FirstOrDefault())
            : StatusCode(404, result.Errors.FirstOrDefault());
    }
    
    [HttpGet("by-participantName/{raffleId}")]
    public async Task<IActionResult> findByName(
        [FromQuery] string name,
        int raffleId)
    {
        logger.LogError($"============= GET participant/by-participantName/{raffleId} =============");
        
        var result = await service.ParticipantService.searchByName(raffleId, name);

        return result.IsSuccess
            ? Ok(result.Successes.FirstOrDefault())
            : StatusCode(404, result.Errors.FirstOrDefault());
    }

    [HttpPost]
    [Authorize (Policy = "ADMIN_OPERATOR")]
    public async Task<IActionResult> addParticipant([FromBody] CreateParticipantDTO dto)
    {
        logger.LogError($"============= POST participant =============");
        
        var result = await service.ParticipantService.createParticipant(dto);
        return result.IsSuccess
            ? Created("Criado", result.Successes.FirstOrDefault())
            : StatusCode(400, result.Errors.FirstOrDefault());
    }

    [HttpPut]
    [Route("{participantId}")]
    [Authorize (Policy = "ADMIN_OPERATOR")]
    public async Task<IActionResult> updateParticipant(int participantId, [FromBody] UpdateParticipantDTO dto)
    {
        logger.LogError($"============= PUT participant/{participantId} =============");

        var result = await service.ParticipantService.updateParticipant(participantId, dto);
        return result.IsSuccess
            ? Ok(result.Successes.FirstOrDefault())
            : StatusCode(400, result.Errors.FirstOrDefault()); 
    }

    [HttpDelete]
    [Route("{raffleId}/{participantId}")]
    [Authorize (Policy = "ADMIN")]
    public async Task<IActionResult> deleteParticipant(int participantId, int raffleId)
    {
        logger.LogError($"============= DELETE participant/{raffleId}/{participantId} =============");
        
        var result = await service.ParticipantService.deleteParticipant(participantId, raffleId);
        return result.IsSuccess
            ? Ok(result.Successes.FirstOrDefault())
            : StatusCode(404, result.Errors.FirstOrDefault()); 
    }

}