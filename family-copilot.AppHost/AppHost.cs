/// <summary>
/// Ponto de entrada para composição distribuída da aplicação.
/// Utiliza o modelo "DistributedApplication" (Aspire tooling) para compor múltiplos projetos locais
/// e configurar dependências e health checks entre eles.
/// </summary>
var builder = DistributedApplication.CreateBuilder(args);

// Altera o nome da aplicação exibido nas ferramentas/telemetria para "Family Copilot".
builder.Environment.ApplicationName = "Family Copilot";

/// <summary>
/// Registra o projeto de API local e configura uma checagem HTTP para o endpoint "/health".
/// O nome lógico do projeto no ambiente de composição será "apiservice".
/// </summary>
var apiService = builder.AddProject<Projects.family_copilot_ApiService>("apiservice")
    .WithHttpHealthCheck("/health");

/// <summary>
/// Registra o frontend web como projeto externo que expõe endpoints HTTP e depende do serviço de API.
/// Também configura a checagem de saúde e aguarda o apiService estar disponível antes de iniciar o frontend.
/// </summary>
builder.AddProject<Projects.family_copilot_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
