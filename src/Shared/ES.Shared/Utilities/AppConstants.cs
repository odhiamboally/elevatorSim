using Microsoft.Extensions.Configuration;

namespace ES.Shared.Utilities;

public static class AppConstants
{

    public static string GetCurrentNodeBIC(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(nameof(configuration));
        return configuration["NodeConfig:CurrentNode"]!;
    }

    public static int ElevatorCapacity = 10;
}
