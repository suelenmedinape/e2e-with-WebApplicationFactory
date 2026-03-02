using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RaffleHub.Api.Data;
using RaffleHub.Api.DTOs.Raffle;
using RaffleHub.Api.Enums;
using RaffleHub.Api.Utils.ExceptionHandler;

using Microsoft.AspNetCore.TestHost;

namespace RaffleHub.Tests.E2E;

public class RaffleE2ETests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;
    private static readonly string DbName = Guid.NewGuid().ToString();

    public RaffleE2ETests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                // Remove all DbContext related registrations
                var descriptors = services.Where(
                    d => d.ServiceType.Name.Contains("DbContextOptions") || 
                         d.ServiceType == typeof(AppDbContext) ||
                         d.ServiceType.Name.Contains("IDatabaseProvider")).ToList();

                foreach (var descriptor in descriptors)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase(DbName);
                });
            });
        });
        _client = _factory.CreateClient();
    }

    private CreateRaffleDto CreateValidRaffleDto() => new()
    {
        RaffleName = "Rifa de Teste",
        Description = "Descrição da Rifa de Teste",
        TotalTickets = 100,
        TicketPrice = 10.0m,
        DrawDate = DateTime.UtcNow.AddDays(7),
        Status = RaffleStatus.ACTIVE
    };

    private MultipartFormDataContent CreateMultipartContent(CreateRaffleDto dto)
    {
        var content = new MultipartFormDataContent();
        content.Add(new StringContent(dto.RaffleName), nameof(dto.RaffleName));
        content.Add(new StringContent(dto.Description), nameof(dto.Description));
        content.Add(new StringContent(dto.TotalTickets.ToString()), nameof(dto.TotalTickets));
        content.Add(new StringContent(dto.TicketPrice.ToString()), nameof(dto.TicketPrice));
        content.Add(new StringContent(dto.DrawDate.ToString("o")), nameof(dto.DrawDate));
        content.Add(new StringContent(dto.Status.ToString()), nameof(dto.Status));
        return content;
    }

    [Fact]
    public async Task CreateRaffle_ShouldReturnCreated_WhenDataIsValid()
    {
        // Arrange
        var dto = CreateValidRaffleDto();
        var content = CreateMultipartContent(dto);

        // Act
        var response = await _client.PostAsync("/Raffle", content);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Data);
        Assert.Equal("Rifa criada com sucesso!", result.Message);
    }

    [Fact]
    public async Task CreateRaffle_ShouldReturnBadRequest_WhenDataIsInvalid()
    {
        // Arrange
        var dto = new CreateRaffleDto
        {
            RaffleName = "", // Invalid
            Description = "Desc",
            TotalTickets = 0, // Invalid
            TicketPrice = 0, // Invalid
            DrawDate = DateTime.UtcNow
        };
        var content = CreateMultipartContent(dto);

        // Act
        var response = await _client.PostAsync("/Raffle", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        Assert.NotNull(result);
        Assert.Equal("Alguns erros de validação ocorreram", result.Message);
    }

    [Fact]
    public async Task ListAll_ShouldReturnOk_AndContainCreatedRaffles()
    {
        // Arrange
        var dto = CreateValidRaffleDto();
        var content = CreateMultipartContent(dto);
        await _client.PostAsync("/Raffle", content);

        // Act
        var response = await _client.GetAsync("/Raffle");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<ListAllRaffleDto>>>();
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.NotEmpty(result.Data);
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenRaffleExists()
    {
        // Arrange
        var dto = CreateValidRaffleDto();
        var content = CreateMultipartContent(dto);
        var postResponse = await _client.PostAsync("/Raffle", content);
        var createResult = await postResponse.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
        var raffleId = createResult!.Data;

        // Act
        var response = await _client.GetAsync($"/Raffle/{raffleId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<ListAllRaffleDto>>();
        Assert.NotNull(result);
        Assert.Equal(raffleId, result.Data!.Id);
    }

    [Fact]
    public async Task GetById_ShouldReturnBadRequest_WhenRaffleDoesNotExist()
    {
        // Arrange
        var randomId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/Raffle/{randomId}");

        // Assert
        // According to ResultExtensions.ToResponse, Result.Fail returns BadRequest
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateRaffle_ShouldReturnOk_WhenUpdateIsSuccessful()
    {
        // Arrange
        var createDto = CreateValidRaffleDto();
        var content = CreateMultipartContent(createDto);
        var postResponse = await _client.PostAsync("/Raffle", content);
        var createResult = await postResponse.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
        var raffleId = createResult!.Data;

        var updateDto = new UpdateRaffleDto
        {
            RaffleName = "Rifa Atualizada",
            Description = "Descrição Atualizada",
            TotalTickets = 200,
            TicketPrice = 20.0m,
            DrawDate = DateTime.UtcNow.AddDays(14),
            Status = RaffleStatus.ACTIVE
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/Raffle/{raffleId}", updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        Assert.NotNull(result);
        Assert.Equal("Rifa atualizada com sucesso!", result.Message);
    }

    [Fact]
    public async Task ChangeStatus_ShouldReturnOk_WhenStatusIsUpdated()
    {
        // Arrange
        var createDto = CreateValidRaffleDto();
        var content = CreateMultipartContent(createDto);
        var postResponse = await _client.PostAsync("/Raffle", content);
        var createResult = await postResponse.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
        var raffleId = createResult!.Data;

        var statusDto = new UpdateStatusDto
        {
            Status = RaffleStatus.COMPLETED
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/Raffle/ChangeStatus/{raffleId}", statusDto);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        // Verify update
        var getResponse = await _client.GetAsync($"/Raffle/{raffleId}");
        var getResult = await getResponse.Content.ReadFromJsonAsync<ApiResponse<ListAllRaffleDto>>();
        Assert.Equal(RaffleStatus.COMPLETED, getResult!.Data!.Status);
    }
}
