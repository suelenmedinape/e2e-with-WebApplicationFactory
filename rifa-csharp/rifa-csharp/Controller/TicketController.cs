using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using rifa_csharp.Service;

namespace rifa_csharp.Controller;

[ApiController]
[Route("tickets")]
public class TicketController : ControllerBase
{
    private readonly UnitOfService service;
    private readonly IMapper mapper;
    private readonly ILogger logger;

    public TicketController(UnitOfService service, IMapper mapper, ILogger<TicketController> logger)
    {
        this.service = service;
        this.mapper = mapper;
        this.logger = logger;
    }
    
    [HttpGet]
    [Route("listTicketsSold/{raffleId}")]
    public async Task<IActionResult> listTicketsSolds(int raffleId)
    {
        logger.LogError($"============= GET tickets/listTicketsSold/{raffleId} =============");
        
        var result = await service.TicketService.listTicketsSolds(raffleId);

        return result.IsSuccess
            ? Ok(result.Successes.FirstOrDefault())
            : StatusCode(404, result.Errors.FirstOrDefault());
    }

    [HttpGet]
    [Route("findByTicket/{raffleId}/{number}")]
    public async Task<IActionResult> findByTicket(int raffleId, int number)
    {
        logger.LogError($"============= GET tickets/findByTicket/{raffleId}/{number} =============");
        
        var result = await service.TicketService.findByNumber(number, raffleId);

        return result.IsSuccess
            ? Ok(result.Successes.FirstOrDefault())
            : StatusCode(404, result.Errors.FirstOrDefault());
    }
}