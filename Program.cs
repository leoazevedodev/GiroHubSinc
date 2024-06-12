using HubSincronizacao;
using HubSincronizacao.Apis.Services;
using HubSincronizacao.Data;
using HubSincronizacao.SeedWork;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        // Depency Injection

        services.AddSingleton<AlphaBuyTwoServices>();
        services.AddSingleton<AlphabiServices>();
        services.AddSingleton<AlphaBuyServices>();
        services.AddScoped<BatchExecution>();

        // Entity Framework
        services.AddDbContext<DataContext>(options =>
            options.UseSqlServer("Data Source=tcp:alphabeto.database.windows.net,1433;Initial Catalog=BDBI;User ID=alphabeto;Password=N13tzsche;Command Timeout=300; Max Pool Size=100"));
        })

    .Build();

host.Run();
