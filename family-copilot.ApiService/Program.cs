/// <summary>
/// Configuração e inicialização do serviço de API (endpoints e pipeline).
/// </summary>
var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
/// <summary>
/// Aplica configurações padrão do projeto (telemetria, health checks, descoberta de serviços, etc.).
/// </summary>
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

string[] summaries = new[] { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };

/// <summary>
/// Endpoint de exemplo que retorna uma lista de previsões do tempo.
/// </summary>
app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.MapDefaultEndpoints();

app.Run();

/// <summary>
/// Modelo de dados que representa a previsão do tempo retornada pelo endpoint.
/// </summary>
/// <param name="Date">Data da previsão.</param>
/// <param name="TemperatureC">Temperatura em graus Celsius.</param>
/// <param name="Summary">Descrição breve da condição climática.</param>
record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    /// <summary>
    /// Temperatura calculada em Fahrenheit.
    /// </summary>
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
