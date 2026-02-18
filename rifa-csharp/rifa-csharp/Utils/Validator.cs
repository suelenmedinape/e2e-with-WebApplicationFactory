using FluentResults;
using rifa_csharp.DTO.Raffle;
using rifa_csharp.Entities;
using rifa_csharp.Enums;

namespace rifa_csharp.Utils;

public static class Validator
{
    public static Result Validate(CreateRaffleDTO cmd)
    {
        if (string.IsNullOrWhiteSpace(cmd.RaffleName))
            return Result.Fail("O nome da rifa não pode estar vazio");

        if (string.IsNullOrWhiteSpace(cmd.Description))
            return Result.Fail("A descrição da rifa não pode estar vazia");

        if (cmd.TicketPrice <= 0)
            return Result.Fail("O valor da rifa deve ser maior que zero");

        if (cmd.TotalTickets <= 0)
            return Result.Fail("A quantidade de tickets deve ser maior que zero");

        if (cmd.DrawDate <= DateTime.UtcNow)
            return Result.Fail("A rifa não pode acontecer no passado");

        return Result.Ok();
    }

    public static Result Validate(UpdateRaffleDTO cmd, Raffle raffle)
    {
        if (raffle.Status == RaffleStatus.COMPLETED || raffle.Status == RaffleStatus.CANCELLED)
            return Result.Fail("Não é possivel editar uma rifa cancelada ou finalizada");

        if (string.IsNullOrEmpty(cmd.RaffleName))
            return Result.Fail("O nome da rifa não pode estar vazio");

        if (string.IsNullOrEmpty(cmd.Description))
            return Result.Fail("A descrição da rifa não pode estar vazia");

        if (cmd.TicketPrice <= 0)
            return Result.Fail("O valor na rifa não pode ser zero ou negativo");

        if (cmd.TotalTickets <= 0)
            return Result.Fail("A quantidade de rifas não pode ser zero ou negativo");

        var dateActuality = DateTime.UtcNow;
        if (dateActuality > cmd.DrawDate)
            return Result.Fail("A rifa não pode acontecer no passado");

        if (cmd.TotalTickets < raffle.TotalTickets)
            return Result.Fail("Você não pode diminuir a quantidade de números");
        
        return Result.Ok();
    }
}