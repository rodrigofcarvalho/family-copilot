# family-copilot

Este repositório contém a solução "family-copilot", uma aplicação .NET 10 com componentes backend e frontend (Blazor). A documentação abaixo está em Português (Brasil) e descreve a estrutura do projeto, como executar localmente, e pontos importantes para desenvolvimento.

## Visão geral

- `family-copilot.Web` — Aplicação web Blazor (frontend).
- `family-copilot.ApiService` — Serviço API (backend).
- `family-copilot.AppHost` — Host de aplicação (ponto de entrada para deploy/execução centralizada).
- `family-copilot.ServiceDefaults` — Biblioteca com configurações padrão: descoberta de serviço, saúde (health checks), resiliência e OpenTelemetry.

A intenção é fornecer uma base com padrões e melhores práticas para serviços .NET (telemetria, health checks e service discovery) pronta para ser referenciada por outros projetos.

## Requisitos

- .NET 10 SDK instalado (verifique com `dotnet --version`).
- Git (para clonar o repositório).

## Como executar localmente

1. Restaurar e compilar:

   `dotnet restore`

   `dotnet build`

2. Executar um projeto específico (exemplo: Web):

   `dotnet run --project family-copilot.Web/family-copilot.Web.csproj`

   Para executar a API:

   `dotnet run --project family-copilot.ApiService/family-copilot.ApiService.csproj`

3. Quando em desenvolvimento, os endpoints de health estão habilitados e podem ser acessados em:

   - `/health` — readiness (somente em Development)
   - `/alive` — liveness (somente em Development)

Observação: esses endpoints são ativados somente quando o ambiente é `Development` por motivos de segurança.

## Configurações e variáveis de ambiente importantes

- `OTEL_EXPORTER_OTLP_ENDPOINT` — Ativa o exportador OTLP do OpenTelemetry quando definido.
- `APPLICATIONINSIGHTS_CONNECTION_STRING` — Pode ser usada para ativar o exportador do Azure Monitor (comentado por padrão no projeto `ServiceDefaults`).

## Serviço `ServiceDefaults`

A biblioteca `family-copilot.ServiceDefaults` inclui extensões para facilitar a configuração comum entre serviços:

- Configuração de OpenTelemetry (tracing e métricas).
- Health checks padrão (`self` com tag `live`).
- Integração com descoberta de serviços (`AddServiceDiscovery`).
- Configuração padrão de `HttpClient` com resiliência e descoberta de serviços.

Use a extensão `AddServiceDefaults` no `Program.cs` dos serviços para aplicar essas configurações rapidamente.

## Desenvolvimento

- Sugestão: execute cada serviço em terminais separados durante desenvolvimento.
- Para debugging no Visual Studio / VS Code, abra a solução e inicie o projeto que deseja debugar.

## Contribuição

- Abra issues para bugs ou sugestões.
- Crie PRs com commits pequenos e descrições claras.

## Licença

Arquivo de licença não adicionado — inclua um `LICENSE` conforme necessário.

## Contato

Para dúvidas sobre o código, abra uma issue no repositório.
