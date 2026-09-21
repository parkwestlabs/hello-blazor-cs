using Microsoft.AspNetCore.DataProtection;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.Extensions.Options;
using MyApp.Web.Components;
using MyApp.Core.Interfaces;
using MyApp.Core.Services;
using MyApp.Data.Repositories;
using MyApp.Web.Extensions;
using MyApp.Web.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<IWeatherService, WeatherService>();

// Optionsパターン: appsettings.json からクラスを使って読み出す方法
builder.Services.AddOptions<WeatherApiOptions>()
    .Bind(builder.Configuration.GetSection(WeatherApiOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// builder.Services.AddTransient<I,T>() を裏側でしてくれる
builder.Services.AddHttpClient<IWeatherRepository, WeatherRepository>((sp, client) =>
{
    // config values from appsettings.json
    var options = sp.GetRequiredService<IOptions<WeatherApiOptions>>().Value;

    client.BaseAddress = options.BaseUrl;
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
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

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

await app.RunAsync();
