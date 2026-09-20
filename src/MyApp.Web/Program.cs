using Microsoft.AspNetCore.DataProtection;
using Microsoft.FluentUI.AspNetCore.Components;
using MyApp.Web.Components;
using MyApp.Core.Interfaces;
using MyApp.Core.Services;
using MyApp.Data.Repositories;
using MyApp.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();

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

string? secretKey = builder.Configuration["DataProtectionSettings:AppSecretKey"];

if (string.IsNullOrEmpty(secretKey))
{
    throw new InvalidOperationException("DataProtectionSettings__AppSecretKey not set");
}

builder.Services.AddDataProtection()
    .SetApplicationName("MyBlazorApp")
    .UseSimpleCryptoTokenProvider(secretKey);

builder.Services.AddFluentUIComponents();

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
