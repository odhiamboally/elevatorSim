// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Web.Client.Console.ApiClients;
using Web.Client.Console.Configurations;
using Web.Client.Console.Dtos;
using Web.Client.Console.Enums;


#region Services and DI

var serviceCollection = new ServiceCollection();

// Build configuration
var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)  // Set the base path to the current directory
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

// Bind configuration settings to a POCO
var appSettings = configuration.GetSection("AppSettings").Get<ApiConfig>();

serviceCollection.AddHttpClient("ES", client =>
{
    client.BaseAddress = new Uri(appSettings!.BaseUrl!);
    client.Timeout = TimeSpan.FromSeconds(appSettings.TimeoutSeconds);

});

serviceCollection.AddSingleton<IConfiguration>(configuration);
serviceCollection.AddScoped<IApiClient, ApiClient>();

var serviceProvider = serviceCollection.BuildServiceProvider();

var config = serviceProvider.GetRequiredService<IConfiguration>();
var apiClient = serviceProvider.GetRequiredService<IApiClient>();

#endregion

#region Call Elevator

int inputRequestedFloor = 5;
string inputDirection = "UP";
int inputPeopleCount = 10;



ElevatorRequest elevatorRequest = new()
{
    RequestedFloor = inputRequestedFloor,
    PeopleCount = inputPeopleCount,
    Direction = Methods.GetDirection(inputDirection),
};

Stopwatch stopwatch = Stopwatch.StartNew();

var apiEndPoint = config["AppSettings:EndPoints:Elevator:FindNearestElevator"];
var reponse = await Methods.CallElevator(apiClient, elevatorRequest, apiEndPoint!);

stopwatch.Stop();

Console.WriteLine($"Elevator Capacity: {reponse.Capacity} Elevator Load: {reponse.CurrentLoad} Elevator Direction: {reponse.Direction}");
Console.WriteLine();

Console.WriteLine();
Console.WriteLine($"Time taken: {stopwatch.ElapsedMilliseconds} ms");
Console.ReadKey();


#endregion

internal class Methods
{
    public async static Task<ElevatorInfo> CallElevator(IApiClient apiClient, ElevatorRequest request, string apiEndPoint)
    {
        var elevator = await apiClient.CallElevator(request, apiEndPoint);
        return elevator;
    }

    public static Direction GetDirection(string inputDirection)
    {
        Direction direction;
        switch (inputDirection)
        {
            case string s when s.Equals("UP", StringComparison.OrdinalIgnoreCase):
                direction = Direction.Up;
                break;

            case string s when s.Equals("DOWN", StringComparison.OrdinalIgnoreCase):
                direction = Direction.Down;
                break;

            default:
                direction = Direction.Idle;
                break;
        }

        return direction;

    }
}
