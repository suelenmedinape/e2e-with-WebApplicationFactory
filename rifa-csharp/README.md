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

1.  **Clonagem e Dependências**:
    ```bash
    dotnet restore
    ```
2.  **Banco de Dados**:
    Configure a ConnectionString no `appsettings.json` e execute:
    ```bash
    dotnet ef database update
    ```
3.  **Execução**:
    ```bash
    dotnet run --project RaffleHub.Api
    ```

## 🗂️ Estrutura do Repositório
-   `RaffleHub.Api`: O core da aplicação (Controllers, Services, Repositories).
-   `RaffleHub.Tests.E2E`: Suite de testes que garante que nenhuma alteração de código quebre as funcionalidades existentes.

---

> **Nota Técnica sobre os Testes**: Cada execução de teste utiliza um identificador único de banco de dados (`Guid.NewGuid().ToString()`), garantindo que um teste nunca interfira nos dados de outro, permitindo execução paralela e determinística.
