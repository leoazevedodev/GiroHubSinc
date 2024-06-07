using HubSincronizacao.Apis.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddSingleton<AlphaBuyTwoServices>();
        services.AddSingleton<AlphabiServices>();
        services.AddSingleton<AlphaBuyServices>();
    })
    .Build();

host.Run();
