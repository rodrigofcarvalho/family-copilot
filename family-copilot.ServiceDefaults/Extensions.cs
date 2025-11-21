using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ServiceDiscovery;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Microsoft.Extensions.Hosting;

/// <summary>
/// Fornece extensões de configuração padrão para aplicações e serviços Aspire.
/// Inclui integração com descoberta de serviços, checagens de saúde, resiliência e OpenTelemetry.
/// Esta classe deve ser referenciada pelos projetos de serviço na solução para aplicar as configurações padrão.
/// </summary>
public static class Extensions
{
    private const string HealthEndpointPath = "/health";
    private const string AlivenessEndpointPath = "/alive";

    /// <summary>
    /// Aplica configurações padrão de serviço ao <see cref="IHostApplicationBuilder"/>, incluindo:
    /// OpenTelemetry, checagens de saúde, descoberta de serviços e padrões para HttpClient.
    /// </summary>
    /// <typeparam name="TBuilder">Tipo do builder que implementa <see cref="IHostApplicationBuilder"/>.</typeparam>
    /// <param name="builder">O builder da aplicação.</param>
    /// <returns>O mesmo builder para encadeamento.</returns>
    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.ConfigureOpenTelemetry();

        builder.AddDefaultHealthChecks();

        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            // Turn on resilience by default
            http.AddStandardResilienceHandler();

            // Turn on service discovery by default
            http.AddServiceDiscovery();
        });

        // Uncomment the following to restrict the allowed schemes for service discovery.
        // builder.Services.Configure<ServiceDiscoveryOptions>(options =>
        // {
        //     options.AllowedSchemes = ["https"];
        // });

        return builder;
    }

    /// <summary>
    /// Configura OpenTelemetry (logging, métricas e tracing) para a aplicação.
    /// Adiciona instrumentação para ASP.NET Core, HttpClient e runtime.
    /// </summary>
    /// <typeparam name="TBuilder">Tipo do builder que implementa <see cref="IHostApplicationBuilder"/>.</typeparam>
    /// <param name="builder">O builder da aplicação.</param>
    /// <returns>O mesmo builder para encadeamento.</returns>
    public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();
            })
            .WithTracing(tracing =>
            {
                tracing.AddSource(builder.Environment.ApplicationName)
                    .AddAspNetCoreInstrumentation(tracing =>
                        // Exclude health check requests from tracing
                        tracing.Filter = context =>
                            !context.Request.Path.StartsWithSegments(HealthEndpointPath)
                            && !context.Request.Path.StartsWithSegments(AlivenessEndpointPath)
                    )
                    // Uncomment the following line to enable gRPC instrumentation (requires the OpenTelemetry.Instrumentation.GrpcNetClient package)
                    //.AddGrpcClientInstrumentation()
                    .AddHttpClientInstrumentation();
            });

        builder.AddOpenTelemetryExporters();

        return builder;
    }

    /// <summary>
    /// Adiciona exportadores adicionais do OpenTelemetry com base na configuração.
    /// Atualmente ativa o exportador OTLP quando a variável de ambiente <c>OTEL_EXPORTER_OTLP_ENDPOINT</c> estiver definida.
    /// </summary>
    /// <typeparam name="TBuilder">Tipo do builder que implementa <see cref="IHostApplicationBuilder"/>.</typeparam>
    /// <param name="builder">O builder da aplicação.</param>
    /// <returns>O mesmo builder para encadeamento.</returns>
    private static TBuilder AddOpenTelemetryExporters<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter)
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }

        // Uncomment the following lines to enable the Azure Monitor exporter (requires the Azure.Monitor.OpenTelemetry.AspNetCore package)
        //if (!string.IsNullOrEmpty(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
        //{
        //    builder.Services.AddOpenTelemetry()
        //       .UseAzureMonitor();
        //}

        return builder;
    }

    /// <summary>
    /// Adiciona checagens de saúde padrão à coleção de serviços.
    /// Inclui uma verificação "self" marcada com a tag "live" para liveness.
    /// </summary>
    /// <typeparam name="TBuilder">Tipo do builder que implementa <see cref="IHostApplicationBuilder"/>.</typeparam>
    /// <param name="builder">O builder da aplicação.</param>
    /// <returns>O mesmo builder para encadeamento.</returns>
    public static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddHealthChecks()
            // Add a default liveness check to ensure app is responsive
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }

    /// <summary>
    /// Mapeia endpoints padrão, como readiness (/health) e liveness (/alive).
    /// Esses endpoints são habilitados somente em ambiente de desenvolvimento por padrão.
    /// </summary>
    /// <param name="app">A aplicação web.</param>
    /// <returns>A mesma instância de <see cref="WebApplication"/> para encadeamento.</returns>
    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        // Adding health checks endpoints to applications in non-development environments has security implications.
        // See https://aka.ms/dotnet/aspire/healthchecks for details before enabling these endpoints in non-development environments.
        if (app.Environment.IsDevelopment())
        {
            // All health checks must pass for app to be considered ready to accept traffic after starting
            app.MapHealthChecks(HealthEndpointPath);

            // Only health checks tagged with the "live" tag must pass for app to be considered alive
            app.MapHealthChecks(AlivenessEndpointPath, new HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains("live")
            });
        }

        return app;
    }
}
