using Quartz;
using Confluent.Kafka;
using MassTransit;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using ES.Application.Abstractions.Interfaces;
using ES.Application.Abstractions.IRepositories;
using ES.Application.Abstractions.IServices;
using ES.Infrastructure.Implementations.Interfaces;
using ES.Infrastructure.Implementations.Repositories;
using ES.Infrastructure.Implementations.Services;
using Refit;
using DbContext = ES.Persistence.DataContext.DbContext;
using ES.Application.Abstractions.Hubs;
using ES.Infrastructure.Implementations.Hubs;

namespace ES.Infrastructure.Utilities;
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connString = configuration.GetConnectionString("ES");
        services.AddDbContext<DbContext>(options => options.UseSqlServer(connString!));

        services.Configure<QuartzOptions>(configuration.GetSection("Quartz"));

        // if you are using persistent job store, you might want to alter some options
        services.Configure<QuartzOptions>(options =>
        {
            options.Scheduling.IgnoreDuplicates = true; // default: false
            options.Scheduling.OverWriteExistingData = true; // default: true
        });

        //services.AddQuartz(q =>
        //{
        //    var jobKey = new JobKey("DHTMaintenanceJob");
        //    q.AddJob<DhtMaintenanceJob>(opts => opts
        //        .WithIdentity(jobKey));

        //    q.AddTrigger(opts => opts
        //        .ForJob(jobKey)
        //        .WithIdentity("DHTMaintenanceJob-trigger")
        //        //.WithCronSchedule("0 0 * * * ?")); // Every hour
        //        .WithSimpleSchedule(x => x
        //            .WithInterval(TimeSpan.FromMinutes(5))
        //            .RepeatForever())
        //    );

        //});

        ////Add the Quartz hosted service
        //services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

        services.AddSignalR();

        services.AddScoped<IServiceManager, ServiceManager>();
        services.AddScoped<ILogService, LogService>();
        services.AddScoped<IElevatorService, ElevatorService>();
        services.AddScoped<IElevatorStateManager, ElevatorStateManager>();
        services.AddScoped<IFloorService, FloorService>();
        services.AddScoped<IFloorQueueManager, FloorQueueManager>();

        services.AddScoped<IHubManager, HubManager>();
        services.AddScoped<IElevatorHub, ElevatorHub>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddTransient(typeof(IBaseRepository<>), typeof(BaseRepository<>));
        services.AddScoped<ILogRepository, LogRepository>();

        return services;
    }
}

