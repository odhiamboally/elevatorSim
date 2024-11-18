using StackExchange.Redis;
using Web.Client.Blazor.Components;
using Web.Client.Blazor.Configurations;
using Web.Client.Blazor.Hubs;
using Web.Client.Blazor.Utilities.Api;
using Web.Client.Blazor.Utilities.Caching;
using Web.Client.Blazor.Utilities.SignalR;
using Serilog;
using Serilog.Formatting.Compact;
using Serilog.Formatting.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
var appSettings = builder.Configuration.GetSection("AppSettings").Get<AppSettings>();

builder.Services.AddHttpClient("ES", client =>
{
    client.BaseAddress = new Uri(appSettings!.ApiBaseUrl!);
    client.Timeout = TimeSpan.FromSeconds(appSettings.TimeoutSeconds);
});

var cacheSettings = new CacheSetting();
builder.Configuration.GetSection("CacheSettings").Bind(cacheSettings);
builder.Services.AddSingleton(cacheSettings);

builder.Services.AddMemoryCache();
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = cacheSettings.Redis!.Configuration;
    options.InstanceName = cacheSettings.Redis.InstanceName;
});

builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(cacheSettings.Redis!.Configuration!));
builder.Services.AddScoped(sp =>
{
    var connectionMultiplexer = sp.GetRequiredService<IConnectionMultiplexer>();
    return connectionMultiplexer.GetDatabase();
});

builder.Services.AddScoped<IApiClient, ApiClient>();
builder.Services.AddSingleton<ICacheService, RedisCacheService>();
builder.Services.AddScoped<ISignalRService, SignalRService>();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Set up Serilog with file sink
//Log.Logger = new LoggerConfiguration()
//    .WriteTo.Async(s => s.Console(new CompactJsonFormatter()))
//    .WriteTo.Async(s => s.File(new CompactJsonFormatter(), "Logs/log.txt", rollingInterval: RollingInterval.Day))
//    .CreateLogger();

// Add Serilog to read from appsettings.json
builder.Host.UseSerilog((context, services, configuration) =>
    configuration
        .ReadFrom.Configuration(context.Configuration)  // Reads from appsettings.json
        .Enrich.FromLogContext()
        .WriteTo.Async(s => s.Console(new CompactJsonFormatter()))
        .WriteTo.Async(s => s.File(new CompactJsonFormatter(), "Logs/log.txt", rollingInterval: RollingInterval.Day))
);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapHub<ElevatorHub>(builder.Configuration["AppSettings:EndPoints:Elevator:BroadCastState"]!);

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
