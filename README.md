# RaffleHub API 🎟️

RaffleHub é uma solução robusta desenvolvida em **ASP.NET Core 9** para o gerenciamento completo de rifas online. O projeto utiliza uma arquitetura moderna, focada em performance, escalabilidade e, acima de tudo, **confiabilidade através de uma cobertura rigorosa de testes End-to-End (E2E)**.

## 🧪 Estratégia de Testes E2E (End-to-End)

O diferencial deste projeto é a sua camada de testes automatizados, localizada no projeto `RaffleHub.Tests.E2E`. Diferente de testes unitários que validam métodos isolados, nossos testes E2E validam o fluxo completo da requisição: desde o recebimento do payload no Controller, passando pela lógica de serviço e validações do FluentResults, até a persistência simulada.

### 🛠️ Tecnologias de Teste
- **xUnit**: Framework principal para execução dos testes.
- **WebApplicationFactory**: Utilizada para subir o servidor da API em memória, garantindo que o teste reflita o comportamento real do ambiente de produção.
- **Entity Framework InMemory**: Substituímos o banco de dados PostgreSQL por um banco em memória durante os testes, permitindo isolamento total entre as execuções e velocidade extrema.
- **MultipartFormDataContent**: Suporte nativo para testar endpoints que recebem formulários complexos (incluindo upload de imagens).

### 🔍 O que é validado?
Os testes cobrem os cenários críticos do ciclo de vida de uma rifa:

1.  **Criação de Rifa**:
    -   **Sucesso**: Valida se a API retorna `201 Created` e um `Guid` válido ao enviar dados corretos.
    -   **Falha**: Valida se a API retorna `400 BadRequest` e as mensagens de erro corretas ao enviar dados inválidos (ex: nome vazio, preço zero).
2.  **Consulta e Listagem**:
    -   Valida a integridade da listagem total de rifas e a recuperação de uma rifa específica por ID.
3.  **Atualização de Dados**:
    -   Garante que alterações em nomes, descrições e preços são persistidas corretamente.
4.  **Gerenciamento de Status**:
    -   Valida a transição de estados da rifa (ex: de `ACTIVE` para `COMPLETED`).

### 🚀 Como executar os testes
Para garantir que a API está funcionando perfeitamente, execute o comando abaixo na pasta `rifa-csharp`:

```bash
dotnet test
```

Este comando irá restaurar as dependências, compilar os projetos e executar todos os cenários de teste, fornecendo um relatório detalhado de sucesso/falha.

---

## 🛠️ Tecnologias Principais
-   **Backend**: .NET 9 (C#)
-   **Database**: PostgreSQL + EF Core
-   **Result Pattern**: FluentResults para tratamento elegante de erros de domínio.
-   **Mapping**: AutoMapper para conversão de Entidades/DTOs.
-   **Background Jobs**: Hangfire para processamentos assíncronos.
-   **Storage**: Integração com Supabase para galeria de fotos.

## ⚙️ Configuração do Ambiente

O projeto agora está totalmente configurado para rodar facilmente com o **Docker Compose**, que cuida tanto do banco de dados PostgreSQL quanto da API.

### 🐳 Rodando com Docker (Recomendado)

1. **Subir os containers**:
   Na pasta `rifa-csharp` onde está o arquivo `compose.yaml`, execute:
   ```bash
   docker compose up -d
   ```
   Isso irá iniciar o banco de dados na porta `5433` (para não conflitar com bancos locais) e iniciar a API. As migrações são aplicadas automaticamente, então não é preciso rodá-las na mão.

2. **Acessar a API**:
   A API estará disponível na porta `8080` rodando no modo de Desenvolvimento. Você pode testar os endpoints através do Swagger:
   - 👉 **http://localhost:8080/swagger/index.html**

3. **Parar os containers**:
   ```bash
   docker compose down
   ```

### 💻 Rodando Localmente (API via .NET CLI)

Se preferir rodar a API de forma local para depuração e apenas hospedar o banco de dados via Docker:

1. **Subir apenas o banco de dados**:
   ```bash
   docker compose up -d postgres
   ```
2. **Aplicar as Migrations** (na pasta `rifa-csharp`):
   ```bash
   dotnet ef database update --project RaffleHub.Api --startup-project RaffleHub.Api
   ```
3. **Rodar a API**:
   ```bash
   dotnet run --project RaffleHub.Api
   ```

## 🗂️ Estrutura do Repositório
-   `RaffleHub.Api`: O core da aplicação (Controllers, Services, Repositories).
-   `RaffleHub.Tests.E2E`: Suite de testes que garante que nenhuma alteração de código quebre as funcionalidades existentes.

---

> **Nota Técnica sobre os Testes**: Cada execução de teste utiliza um identificador único de banco de dados (`Guid.NewGuid().ToString()`), garantindo que um teste nunca interfira nos dados de outro, permitindo execução paralela e determinística.
