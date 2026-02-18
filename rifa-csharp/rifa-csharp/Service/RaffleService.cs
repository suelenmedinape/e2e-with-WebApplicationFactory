using AutoMapper;
using FluentResults;
using rifa_csharp.DTO.Raffle;
using rifa_csharp.DTO.Ticket;
using rifa_csharp.Entities;
using rifa_csharp.Enums;
using rifa_csharp.Interface;
using rifa_csharp.Repositories;
using rifa_csharp.Utils;

namespace rifa_csharp.Service;

public class RaffleService
{
    private readonly IUnitOfWork unit;
    private readonly IMapper mapper;

    public RaffleService(IUnitOfWork unit,  IMapper mapper)
    {
        this.unit = unit;
        this.mapper = mapper;
    }

    public async Task<Result> listAll()
    {
        try
        {
            var all = await unit.RaffleRepository.ListAll();
            
            var result = new List<ListAllRafflesDTO>();

            foreach (var raffle in all)
            {
                var soldTicketsCount =
                    await unit.TicketRepository.CountAsync(
                        t => t.RaffleId == raffle.Id && t.Sold
                    );

                result.Add(new ListAllRafflesDTO()
                {
                    Id =  raffle.Id,
                    RaffleName = raffle.RaffleName,
                    SoldTicketsCount = soldTicketsCount,
                    TicketPrice = raffle.TicketPrice,
                    TotalTickets = raffle.TotalTickets,
                    Status =  raffle.Status,
                });
            }
            
            return new Result().WithSuccess(new Success("Sucesso ao buscar rifas").WithMetadata("data", result));
        }
        catch (Exception e)
        {
            return new Result().WithError(new Error(e.Message));
        }
    }
    
    public async Task<Result> listNamesRaffle()
    {
        try
        {
            var all = await unit.RaffleRepository.ListAll();
            var dto = mapper.Map<List<RaffleNamesDTO>>(all);
            return new Result().WithSuccess(new Success("Sucesso ao buscar rifas").WithMetadata("data", dto));
        }
        catch (Exception e)
        {
            return new Result().WithError(new Error(e.Message));
        }
    }

    public async Task<Result> listById(int id)
    {
        try
        {
            var item = await unit.RaffleRepository.findById(id) ?? throw new Exception("Rifa não encontrada");
            var dto = mapper.Map<RaffleDetailsDTO>(item);
            return new Result().WithSuccess(new Success("Sucesso ao buscar rifas").WithMetadata("data", dto));
        }
        catch (Exception e)
        {
            return new Result().WithError(new Error(e.Message));
        }
    }

    public async Task<Result> createRaffle(CreateRaffleDTO newRaffle)
    {
        var validation = Validator.Validate(newRaffle);
        if (validation.IsFailed)
            return validation;

        try
        {
            var raffle = new Raffle
            {
                RaffleName = newRaffle.RaffleName,
                Description = newRaffle.Description,
                TicketPrice = newRaffle.TicketPrice,
                TotalTickets = newRaffle.TotalTickets,
                DrawDate = newRaffle.DrawDate,
                CreatedAt = DateTime.UtcNow,
                Status = 0
            };

            var tickets = Enumerable.Range(1, raffle.TotalTickets)
                .Select(i => new Ticket
                {
                    Raffle = raffle,
                    TicketNumber = i,
                    Sold = false
                })
                .ToList();

            unit.RaffleRepository.Add(raffle);
            await unit.TicketRepository.AddRangeAsync(tickets);

            await unit.CommitAsync();

            return new Result()
                .WithSuccess(new Success("Rifa criada com sucesso").WithMetadata("data", raffle.Id));
        }
        catch (Exception e)
        {
            return Result.Fail(e.Message);
        }
    }

    public async Task<Result> updateRaffle(int id, UpdateRaffleDTO updatedRaffle)
    {
        try
        {
            
            var raffle = await unit.RaffleRepository.findById(id) ??
                throw new Exception("Rifa não encontrada");
            
            var validation = Validator.Validate(updatedRaffle, raffle);
            if (validation.IsFailed)
                return validation;
            
            var oldTotalTickets = raffle.TotalTickets;
            
            mapper.Map(updatedRaffle, raffle);
            await unit.CommitAsync();

            if (updatedRaffle.TotalTickets > oldTotalTickets)
            {
                var tickets = Enumerable.Range(oldTotalTickets + 1, updatedRaffle.TotalTickets - oldTotalTickets)
                    .Select(i => new Ticket
                    {
                        RaffleId = raffle.Id,
                        TicketNumber = i,
                        Sold = false,
                        ParticipantId = null
                    }).ToList();

                await unit.TicketRepository.AddRangeAsync(tickets);
                await unit.CommitAsync();
            }
            
            return new Result().WithSuccess(new Success("Rifa atualizada com sucesso").WithMetadata("data", updatedRaffle.RaffleName));
        }
        catch (Exception e)
        {
            return new Result().WithError(new Error(e.Message));
        }
    }

    public async Task<Result> listParticipantsRaffle(int raffleId)
    {
        try
        {
            var raffleExist = await unit.RaffleRepository.findById(raffleId)
                              ?? throw new Exception("Rifa não encontrada");

            var tickets = await unit.TicketRepository.findAsync(t =>
                t.RaffleId == raffleId &&
                t.ParticipantId != null
            );

            if (!tickets.Any())
                throw new Exception("Nenhum participante encontrado para esta rifa");

            var result = tickets
                .GroupBy(t => t.Participant)
                .Select(g => new ParticipantRaffleDTO
                {
                    Id = g.Key.Id,
                    ParticipantName = g.Key.ParticipantName,
                    Phone = g.Key.Phone,
                    TicketNumbers = g.Select(t => t.TicketNumber).ToList()
                })
                .ToList();

            return new Result()
                .WithSuccess(
                    new Success("Participantes listados com sucesso")
                        .WithMetadata("data", result)
                );
        }
        catch (Exception e)
        {
            return new Result().WithError(new Error(e.Message));
        }
    }

    public async Task<Result> ChangeRaffleStatus(int id, Raffle newStatus)
    {
        try
        {
            var raffle = await unit.RaffleRepository.findById(id);
            if (raffle == null)
                throw new Exception("Rifa não encontrada");

            // valida se o status é permitido
            if (newStatus.Status != RaffleStatus.CANCELLED &&
                newStatus.Status != RaffleStatus.COMPLETED)
                throw new Exception("Status inválido para alteração"); 

            raffle.Status = newStatus.Status;
            await unit.CommitAsync();

            var message = newStatus.Status == RaffleStatus.CANCELLED
                ? "Rifa cancelada com sucesso"
                : "Rifa finalizada com sucesso";

            return new Result()
                .WithSuccess(new Success(message));
        }
        catch (Exception e)
        {
            return new Result().WithError(new Error(e.Message));
        }
    }

    public async Task<Result> deleteRaffle(int raffleId)
    {
        try
        {
            var raffle = await unit.RaffleRepository.findById(raffleId) ?? throw new Exception("Rifa não encontrada");

            unit.RaffleRepository.Delete(raffle);
            await unit.CommitAsync();
            
            return new Result()
                .WithSuccess(new Success("Rifa deletada com sucesso"));
        }
        catch (Exception e)
        {
            return new Result().WithError(new Error(e.Message));
        }
    }
    
    public async Task<Result> GetRafflesReportAsync()
    {
        try
        {
            var raffles = await unit.RaffleRepository.ListAll();

            var result = new List<RaffleSummaryDto>();

            foreach (var raffle in raffles)
            {
                var soldTicketsCount =
                    await unit.TicketRepository.CountAsync(
                        t => t.RaffleId == raffle.Id && t.Sold
                    );

                var participantsCount =
                    await unit.TicketRepository.CountDistinctAsync(
                        t => t.RaffleId == raffle.Id && t.ParticipantId != null,
                        t => t.ParticipantId
                    );

                result.Add(new RaffleSummaryDto
                {
                    RaffleName = raffle.RaffleName,
                    SoldTicketsCount = soldTicketsCount,
                    ParticipantsCount = participantsCount,
                    TotalTicketsCount = raffle.TotalTickets
                });
            }

            return new Result()
                .WithSuccess(
                    new Success("Relatório de rifas gerado com sucesso")
                        .WithMetadata("data", result)
                );
        }
        catch (Exception e)
        {
            return new Result()
                .WithError(new Error("Erro ao gerar relatório de rifas"));
        }
    }
}