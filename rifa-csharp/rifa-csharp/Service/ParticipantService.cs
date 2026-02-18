using System.Globalization;
using System.Text;
using AutoMapper;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using rifa_csharp.DTO.Participant;
using rifa_csharp.Entities;
using rifa_csharp.Repositories;

namespace rifa_csharp.Service;

public class ParticipantService
{
    private readonly UnitOfWork unit;
    private readonly IMapper mapper;

    public ParticipantService(UnitOfWork unit,  IMapper mapper)
    {
        this.unit = unit;
        this.mapper = mapper;
    }
    
    public async Task<Result> listById(int id)
    {
        try
        {
            var item = await unit.ParticipantRepository.findById(id) ?? throw new Exception("Participante não encontrado");
            var dto = mapper.Map<ParticipantDetailsDTO>(item);
            return new Result().WithSuccess(new Success("Sucesso ao buscar rifas").WithMetadata("data", dto));
        }
        catch (Exception e)
        {
            return new Result().WithError(new Error(e.Message));
        }
    }
    
    public async Task<Result> searchByName(int raffleId, string? name)
    {
        try
        {
            var query = unit.ParticipantRepository.Query()
                .Where(p => p.Tickets.Any(t => t.RaffleId == raffleId));

            var participants = await query
                .Select(p => new
                {
                    p.Id,
                    p.ParticipantName,
                    p.Phone
                })
                .ToListAsync();

            if (!string.IsNullOrWhiteSpace(name))
            {
                name = RemoveAccents(name).ToLower();

                participants = participants
                    .Where(p => RemoveAccents(p.ParticipantName)
                        .ToLower()
                        .StartsWith(name))
                    .ToList();
            }

            if (participants.Count == 0)
            {
                return new Result()
                    .WithError(
                        new Error("Nenhum participante encontrado")
                    );
            }

            return new Result()
                .WithSuccess(
                    new Success("Participantes encontrados")
                        .WithMetadata("data", participants)
                );
        }
        catch (Exception e)
        {
            return new Result().WithError(new Error(e.Message));
        }
    }

    private static string RemoveAccents(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;

        var normalized = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();

        foreach (var c in normalized)
        {
            if (Char.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }

        return sb.ToString();
    }

    public async Task<Result> searchByPhone(int raffleId, string? phone)
    {
        try
        {
            var raffle = await unit.RaffleRepository.findById(raffleId)
                         ?? throw new Exception("Rifa não encontrada");

            var query = unit.ParticipantRepository.Query()
                .Where(p => p.Tickets.Any(t => t.RaffleId == raffleId));

            if (!string.IsNullOrWhiteSpace(phone))
            {
                query = query.Where(p => p.Phone.StartsWith(phone));
            }

            var participants = await query
                .Select(p => new
                {
                    p.Id,
                    p.ParticipantName,
                    p.Phone
                }).ToListAsync();

            if (participants.Count == 0)
            {
                return new Result()
                    .WithError(
                        new Error("Nenhum participante encontrado")
                    );
            }
            
            return new Result()
                .WithSuccess(
                    new Success("Participantes encontrados")
                        .WithMetadata("data", participants)
                );
        }
        catch (Exception e)
        {
            return new Result().WithError(new Error(e.Message));
        }
    }
    
    public async Task<Result> createParticipant(CreateParticipantDTO dto)
    {
        try
        {
            var raffleExist = await unit.RaffleRepository.findById(dto.RaffleId)
                              ?? throw new Exception("Rifa não encontrada");

            var tickets = await unit.TicketRepository.findAsync(t =>
                t.RaffleId == dto.RaffleId &&
                dto.TicketNumbers.Contains(t.TicketNumber));
            
            if (!tickets.Any())
                throw new Exception("Números não encontrados");

            if (tickets.Any(t => t.Sold))
                throw new Exception("Um ou mais números já foram vendidos");

            var phoneAlreadyUsed = await unit.TicketRepository.AnyAsync(t =>
                t.RaffleId == dto.RaffleId &&
                t.Participant != null &&
                t.Participant.Phone == dto.Phone
            );

            if (phoneAlreadyUsed)
                throw new Exception("Já existe um participante com esse telefone nesta rifa");

            var participant = mapper.Map<Participant>(dto);

            unit.ParticipantRepository.Add(participant);
            await unit.CommitAsync();

            foreach (var ticket in tickets)
            {
                ticket.ParticipantId = participant.Id;
                ticket.Sold = true;
                unit.TicketRepository.Update(ticket);
            }

            await unit.CommitAsync();

            return new Result()
                .WithSuccess(
                    new Success("Participante criado com sucesso")
                        .WithMetadata("data", participant.Id)
                );
        }
        catch (Exception e)
        {
            return new Result().WithError(new Error(e.Message));
        }
    }

    public async Task<Result> updateParticipant(int participantId, UpdateParticipantDTO dto)
    {
        try
        {
            var raffle = await unit.RaffleRepository.findById(dto.RaffleId)
                         ?? throw new Exception("Rifa não encontrada");

            var participant = await unit.ParticipantRepository.findById(participantId)
                              ?? throw new Exception("Participante não encontrado");

            // 1️⃣ Atualiza dados simples (nome, telefone)
            mapper.Map(dto, participant);

            // 2️⃣ Busca apenas tickets livres
            var tickets = await unit.TicketRepository.findAsync(t =>
                t.RaffleId == dto.RaffleId &&
                dto.TicketNumbers.Contains(t.TicketNumber));

            if (!tickets.Any())
                throw new Exception("Números não encontrados");

            if (tickets.Any(t => t.Sold))
                throw new Exception("Um ou mais números já foram vendidos");

            // 3️⃣ Associa os tickets ao participante
            foreach (var ticket in tickets)
            {
                ticket.ParticipantId = participant.Id;
                ticket.Sold = true;
                unit.TicketRepository.Update(ticket);
            }

            await unit.CommitAsync();

            return new Result()
                .WithSuccess(
                    new Success("Participante atualizado com sucesso")
                        .WithMetadata("data", participant.Id)
                );
        }
        catch (Exception e)
        {
            return new Result().WithError(new Error(e.Message));
        }
    }
    
    public async Task<Result> deleteParticipant(int participantId, int rifaId)
    {
        try
        {
            var raffle = await unit.RaffleRepository.findById(rifaId) ?? throw new Exception("Rifa não encontrada");
            
            var participant = await unit.ParticipantRepository.findById(participantId)
                              ?? throw new Exception("Participante não encontrado");

            if (raffle.Tickets.All(t => t.ParticipantId != participantId))
                throw new Exception("Esse participante não pertence a essa rifa");

            var tickets = await unit.TicketRepository.findAsync(t =>
                t.ParticipantId == participantId);

            foreach (var ticket in tickets)
            {
                ticket.ParticipantId = null;
                ticket.Sold = false;
                unit.TicketRepository.Update(ticket);
            }

            unit.ParticipantRepository.Delete(participant);

            await unit.CommitAsync();

            return new Result()
                .WithSuccess(new Success("Participante deletado com sucesso"));
        }
        catch (Exception e)
        {
            return new Result().WithError(new Error(e.Message));
        }
    }
    
}