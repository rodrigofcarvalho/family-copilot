namespace family_copilot.Web;

/// <summary>
/// Cliente HTTP para consumir o endpoint de previsão do tempo exposto pela API.
/// Encapsula chamadas para o endpoint "/weatherforecast" e converte a resposta para <see cref="WeatherForecast"/>[].
/// </summary>
public class WeatherApiClient
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Cria uma nova instância de <see cref="WeatherApiClient"/>.
    /// </summary>
    /// <param name="httpClient">Instância de <see cref="HttpClient"/> configurada para comunicar com o serviço de API.</param>
    public WeatherApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Recupera previsões do tempo do serviço remoto.
    /// </summary>
    /// <param name="maxItems">Número máximo de itens a retornar (padrão: 10).</param>
    /// <param name="cancellationToken">Token de cancelamento opcional.</param>
    /// <returns>Array de <see cref="WeatherForecast"/> com, no máximo, <paramref name="maxItems"/> elementos.</returns>
    public async Task<WeatherForecast[]> GetWeatherAsync(int maxItems = 10, CancellationToken cancellationToken = default)
    {
        List<WeatherForecast>? forecasts = null;

        await foreach (var forecast in _httpClient.GetFromJsonAsAsyncEnumerable<WeatherForecast>("/weatherforecast", cancellationToken))
        {
            if (forecasts?.Count >= maxItems)
            {
                break;
            }
            if (forecast is not null)
            {
                forecasts ??= new List<WeatherForecast>();
                forecasts.Add(forecast);
            }
        }

        return forecasts?.ToArray() ?? Array.Empty<WeatherForecast>();
    }
}

/// <summary>
/// Representa uma previsão do tempo para uma data especificada.
/// </summary>
/// <param name="Date">Data da previsão.</param>
/// <param name="TemperatureC">Temperatura em graus Celsius.</param>
/// <param name="Summary">Descrição breve da condição climática.</param>
public record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    /// <summary>
    /// Temperatura calculada em Fahrenheit.
    /// </summary>
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
