# Usa a imagem oficial do SDK do .NET 9.0 como ambiente de build e teste
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# Define o diretório de trabalho dentro do container
WORKDIR /src

# 1. Copiamos os arquivos de projeto (csproj) primeiro.
# Por que? Para aproveitar o sistema de cache de camadas do Docker!
# Se os pacotes não mudarem, o Docker não baixa tudo de novo a cada build.
COPY ["RaffleHub.Tests.E2E/RaffleHub.Tests.E2E.csproj", "RaffleHub.Tests.E2E/"]
COPY ["RaffleHub.Api/RaffleHub.Api.csproj", "RaffleHub.Api/"]

# 2. Restaura as dependências (baixa os pacotes do NuGet)
RUN dotnet restore "RaffleHub.Tests.E2E/RaffleHub.Tests.E2E.csproj"

# 3. Copia o restante do código-fonte para dentro do container
COPY . .

# 4. Muda o diretório de trabalho para a pasta de testes
WORKDIR "/src/RaffleHub.Tests.E2E"

# 5. O comando padrão ao rodar o container será executar os testes
# O parâmetro --logger:trx serve para gerar um relatório de testes estruturado, útil em CI/CD
ENTRYPOINT ["dotnet", "test", "--no-restore", "--logger:trx"]
