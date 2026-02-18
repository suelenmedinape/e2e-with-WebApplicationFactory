using AutoMapper;
using Moq;
using rifa_csharp.DTO.Raffle;
using rifa_csharp.Entities;
using rifa_csharp.Interface;
using rifa_csharp.Service;

namespace rifa_csharp_tests.RaffleTests;

public class RaffleServiceTests
{
    private Mock<IGenericRepository<Raffle>> CreateRaffleRepositoryMock()
    {
        var repoMock = new Mock<IGenericRepository<Raffle>>();
        return repoMock;
    }
    
    private Mock<IGenericRepository<Ticket>> CreateTicketRepositoryMock()
    {
        var repoMock = new Mock<IGenericRepository<Ticket>>();
        return repoMock;
    }

    private Mock<IUnitOfWork> CreateUnitOfWorkMock()
    {
        var unitMock = new Mock<IUnitOfWork>();
        return unitMock;
    }

    private Mock<IMapper> CreateMapperMock()
    {
        var mapperMock = new Mock<IMapper>();
        return mapperMock;
    }

    [Fact(DisplayName = "listNamesRaffle: Deve dar sucesso ao ao buscar rifas")]
    public async Task ListNamesRaffle_ListAllRaffles_ShouldReturnSuccess()
    {
        var unit = CreateUnitOfWorkMock();
        var repoMock = CreateRaffleRepositoryMock();
        var mapperMock = CreateMapperMock();

        var id = 0;
        
        var raffle = new Raffle
        {
            Id = id,
            RaffleName = "Rifa de Teste",
            Description = "Testando o listall do service",
            TotalTickets = 100,
            TicketPrice = 10,
            DrawDate = DateTime.UtcNow,
            Status = 0,
            CreatedAt = DateTime.UtcNow,
        };

        var raffles = new List<Raffle>();
        raffles.Add(raffle);

        repoMock.Setup(r => r.ListAll()).ReturnsAsync(raffles);
        unit.Setup(u => u.RaffleRepository).Returns(repoMock.Object);
        mapperMock
            .Setup(m => m.Map<List<RaffleNamesDTO>>(It.IsAny<IEnumerable<Raffle>>()))
            .Returns((IEnumerable<Raffle> raff) => raff.Select(r => new RaffleNamesDTO
            {
                Id = r.Id,
                RaffleName = r.RaffleName,
            }).ToList()
        );
        
        var service = new RaffleService(unit.Object, mapperMock.Object);

        var result = await service.listNamesRaffle();
        
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
    }

    [Fact(DisplayName = "listById: Deve retornar sucesso se o id for correto")]
    public async Task ListByIdRaffle_IdExist_ShouldReturnSuccess()
    {
        var repoMock = CreateRaffleRepositoryMock();
        var mapperMock = CreateMapperMock();
        var unit =  CreateUnitOfWorkMock();

        var id = 1;

        var raffle = new Raffle
        {
            Id = id,
            RaffleName = "Rifa de Teste",
            Description = "Testando o listbyid do service",
            TotalTickets = 100,
            TicketPrice = 10,
            DrawDate = DateTime.UtcNow,
            Status = 0,
            CreatedAt = DateTime.UtcNow,
        };
        
        repoMock.Setup(r => r.findById(id)).ReturnsAsync(raffle);
        unit.Setup(u => u.RaffleRepository).Returns(repoMock.Object);
        mapperMock
            .Setup(m => m.Map<Raffle>(It.IsAny<RaffleDetailsDTO>()))
            .Returns((RaffleDetailsDTO dto) => new Raffle()
            {
                Id = dto.Id,
                RaffleName = dto.RaffleName,
                Description = dto.Description,
                TotalTickets = dto.TotalTickets,
                TicketPrice = dto.TicketPrice,
                DrawDate = dto.DrawDate,
                Status = dto.Status,
            });
        
        var service = new RaffleService(unit.Object, mapperMock.Object);
        
        var result = await service.listById(id);
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
    }
    
    [Fact(DisplayName = "listById: Deve retornar erro se o id for errado")]
    public async Task ListByIdRaffle_IdNotExist_ShouldReturnError()
    {
        var repoMock = CreateRaffleRepositoryMock();
        var mapperMock = CreateMapperMock();
        var unit =  CreateUnitOfWorkMock();

        var id = 1;
        
        repoMock.Setup(r => r.findById(id)).ReturnsAsync((Raffle?)null);
        unit.Setup(u => u.RaffleRepository).Returns(repoMock.Object);
        
        var service = new RaffleService(unit.Object, mapperMock.Object);
        
        var result = await service.listById(id);
        
        Assert.NotNull(result);
        Assert.True(result.IsFailed);
        
    }

    [Fact(DisplayName = "createRaffle: Deve retornar sucesso se os dados estiverem corretos")]
    public async Task CreateRaffle_DataCorrect_ShouldReturnSuccess()
    {
        var unitMock = new Mock<IUnitOfWork>();
        var raffleRepoMock = new Mock<IGenericRepository<Raffle>>();
        var ticketRepoMock = new Mock<IGenericRepository<Ticket>>();
        var mapperMock = CreateMapperMock();

        unitMock.Setup(u => u.RaffleRepository).Returns(raffleRepoMock.Object);
        unitMock.Setup(u => u.TicketRepository).Returns(ticketRepoMock.Object);
        unitMock.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);

        var dto = new CreateRaffleDTO
        {
            RaffleName = "Rifa Teste",
            Description = "Descrição válida",
            TicketPrice = 10,
            TotalTickets = 5,
            DrawDate = DateTime.UtcNow.AddDays(1)
        };

        var service = new RaffleService(unitMock.Object, mapperMock.Object);

        var result = await service.createRaffle(dto);

        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
    }

    [Fact(DisplayName = "createRaffle: Deve retornar erro se os dados estiverem errados")]
    public async Task CreateRaffle_DataNotCorrect_ShouldReturnError()
    {
        var unitMock = new Mock<IUnitOfWork>();
        var raffleRepoMock = new Mock<IGenericRepository<Raffle>>();
        var ticketRepoMock = new Mock<IGenericRepository<Ticket>>();
        var mapperMock = CreateMapperMock();
        
        unitMock.Setup(u => u.RaffleRepository).Returns(raffleRepoMock.Object);
        unitMock.Setup(u => u.TicketRepository).Returns(ticketRepoMock.Object);
        unitMock.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);

        var dto = new CreateRaffleDTO
        {
            RaffleName = "",
            Description = "Descrição válida",
            TicketPrice = 10,
            TotalTickets = -5,
            DrawDate = DateTime.UtcNow.AddDays(1)
        };

        var service = new RaffleService(unitMock.Object, mapperMock.Object);

        var result = await service.createRaffle(dto);

        Assert.NotNull(result);
        Assert.True(result.IsFailed);
    }

    [Fact(DisplayName = "updateRaffle: Deve retornar sucesso se os dados estiverem corretos")]
    public async Task UpdateRaffle_DataCorrect_ShouldReturnSuccess()
    {
        var unitMock = new Mock<IUnitOfWork>();
        var raffleRepoMock = new Mock<IGenericRepository<Raffle>>();
        var ticketRepoMock = new Mock<IGenericRepository<Ticket>>();
        var mapperMock = CreateMapperMock();

        const int id = 21;
        
        var existingRaffle = new Raffle
        {
            Id = id,
            RaffleName = "Antiga",
            Description = "Antiga",
            TotalTickets = 3,
            TicketPrice = 5,
            DrawDate = DateTime.UtcNow.AddDays(1),
            Status = 0
        };
        
        var dto = new UpdateRaffleDTO
        {
            RaffleName = "Rifa Teste",
            Description = "Descrição válida",
            TotalTickets = 5,
            TicketPrice = 10,
            DrawDate = DateTime.UtcNow.AddDays(1),
            Status = 0
        };

        raffleRepoMock
            .Setup(r => r.findById(id))
            .ReturnsAsync(existingRaffle);
        
        ticketRepoMock
            .Setup(r => r.AddRangeAsync(It.IsAny<List<Ticket>>()))
            .Returns(Task.CompletedTask);

        unitMock.Setup(u => u.RaffleRepository).Returns(raffleRepoMock.Object);
        unitMock.Setup(u => u.TicketRepository).Returns(ticketRepoMock.Object);
        unitMock.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
        
        mapperMock
            .Setup(m => m.Map(It.IsAny<UpdateRaffleDTO>(), It.IsAny<Raffle>()))
            .Callback<UpdateRaffleDTO, Raffle>((dto, raffle) =>
            {
                raffle.RaffleName = dto.RaffleName;
                raffle.Description = dto.Description;
                raffle.TotalTickets = dto.TotalTickets;
                raffle.TicketPrice = dto.TicketPrice;
                raffle.DrawDate = dto.DrawDate;
                raffle.Status = dto.Status;
            });
        
        var service = new RaffleService(unitMock.Object, mapperMock.Object);

        var result = await service.updateRaffle(id, dto);

        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
    }
    
    [Fact(DisplayName = "updateRaffle: Se o ID não existir, deve retornar erro")]
    public async Task UpdateRaffle_IdNotExist_ShouldReturnError()
    {
        var unitMock = new Mock<IUnitOfWork>();
        var raffleRepoMock = new Mock<IGenericRepository<Raffle>>();
        var mapperMock = CreateMapperMock();

        const int invalidId = 999;

        raffleRepoMock
            .Setup(r => r.findById(invalidId))
            .ReturnsAsync((Raffle?)null);

        unitMock
            .Setup(u => u.RaffleRepository)
            .Returns(raffleRepoMock.Object);

        var service = new RaffleService(unitMock.Object, mapperMock.Object);

        var result = await service.updateRaffle(invalidId, new UpdateRaffleDTO());

        Assert.NotNull(result);
        Assert.True(result.IsFailed);
    }
    
    [Fact(DisplayName = "updateRaffle: Deve retornar erro se os dados estiverem errados")]
    public async Task UpdateRaffle_DataNotCorrect_ShouldReturnError()
    {
        var unitMock = new Mock<IUnitOfWork>();
        var raffleRepoMock = new Mock<IGenericRepository<Raffle>>();
        var ticketRepoMock = new Mock<IGenericRepository<Ticket>>();
        var mapperMock = CreateMapperMock();
        
        unitMock.Setup(u => u.RaffleRepository).Returns(raffleRepoMock.Object);
        unitMock.Setup(u => u.TicketRepository).Returns(ticketRepoMock.Object);
        unitMock.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);

        const int id = 21;
        
        var existingRaffle = new Raffle
        {
            Id = id,
            RaffleName = "Antiga",
            Description = "Antiga",
            TotalTickets = -3,
            TicketPrice = 5,
            DrawDate = DateTime.UtcNow.AddDays(1),
            Status = 0
        };
        
        var dto = new UpdateRaffleDTO
        {
            RaffleName = "",
            Description = "Descrição válida",
            TotalTickets = 5,
            TicketPrice = 10,
            DrawDate = DateTime.UtcNow.AddDays(1),
            Status = 0
        };

        var service = new RaffleService(unitMock.Object, mapperMock.Object);

        var result = await service.updateRaffle(id, dto);

        Assert.NotNull(result);
        Assert.True(result.IsFailed);
    }
    
}