using AutoMapper;
using FluentResults;
using rifa_csharp.DTO.Ticket;
using rifa_csharp.Repositories;

namespace rifa_csharp.Service;

public class TicketService
{
    
    private readonly UnitOfWork unit;
    private readonly IMapper mapper;

    public TicketService(UnitOfWork unit,  IMapper mapper)
    {
        this.unit = unit;
        this.mapper = mapper;
    }
    
    public async Task<Result> listTicketsSolds(int raffleId)
    {
        try
        {
            var raffleExist = await unit.RaffleRepository.findById(raffleId)
                              ?? throw new Exception("Rifa não encontrada");

            var ticketNumbers = await unit.TicketRepository.findAsync(t =>
                t.RaffleId == raffleId &&
                t.Sold == true
            );

            if (!ticketNumbers.Any())
                throw new Exception("Nenhum ticket vendido para esta rifa");

            var numbers = ticketNumbers
                .Select(t => t.TicketNumber)
                .ToList();

            return new Result()
                .WithSuccess(
                    new Success("Tickets vendidos listados com sucesso")
                        .WithMetadata("data", numbers)
                );
        }
        catch (Exception e)
        {
            return new Result().WithError(new Error(e.Message));
        }
    }

    public async Task<Result> findByNumber(int number, int raffleId)
    {
        try
        {
            var ticket = await unit.TicketRepository.FirstOrDefaultAsync(
                t => t.TicketNumber == number && t.RaffleId == raffleId,
                t => t.Participant
            );

            if (ticket == null)
                return new Result().WithError(new Error("Número não encontrado"));

            if (ticket.Participant == null)
                return new Result().WithError(new Error("Número ainda não foi vendido"));

            var dto = new FindByNumberDTO
            {
                TicketNumber = ticket.TicketNumber,
                ParticipantName = ticket.Participant.ParticipantName,
                Phone = ticket.Participant.Phone
            };

            return new Result()
                .WithSuccess(
                    new Success("Número encontrado")
                        .WithMetadata("data", dto)
                );
        }
        catch (Exception e)
        {
            return new Result().WithError(new Error(e.Message));
        }
    }
}