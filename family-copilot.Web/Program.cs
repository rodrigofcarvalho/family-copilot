using family_copilot.Web;
using family_copilot.Web.Components;

var builder = WebApplication.CreateBuilder(args);

// Adiciona configurações e integrações padrão (OpenTelemetry, service discovery, health checks, etc.).
/// <summary>
/// Aplica as configurações padrão do projeto, incluindo instrumentação e descoberta de serviços.
/// </summary>
builder.AddServiceDefaults();

// Add services to the container.
/// <summary>
/// Registra serviços necessários para componentes Razor e renderização interativa no servidor.
/// </summary>
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddOutputCache();

builder.Services.AddHttpClient<WeatherApiClient>(client =>
    {
        // This URL uses "https+http://" to indicate HTTPS is preferred over HTTP.
        // Learn more about service discovery scheme resolution at https://aka.ms/dotnet/sdschemes.
        client.BaseAddress = new("https+http://apiservice");
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.UseOutputCache();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();
