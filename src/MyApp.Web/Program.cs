using MyApp.Web.Components;
using MyApp.Core.Interfaces;
using MyApp.Core.Services;
using MyApp.Data.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<IWeatherService, WeatherService>();

builder.Services.AddHttpClient<IWeatherRepository, WeatherRepository>(client =>
{
    // config values from appsettings.json
    var baseUrl = builder.Configuration["WeatherApi:BaseUrl"];
    var uriString = baseUrl ?? throw new InvalidOperationException("APIのURLが設定されていません。");
    client.BaseAddress = new Uri(uriString);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

await app.RunAsync();
