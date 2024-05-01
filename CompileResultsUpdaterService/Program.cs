using Common;
using CompileResultsUpdaterService;
using Microsoft.EntityFrameworkCore;

bool isLocal = Environment.GetEnvironmentVariable("MODE") is null;
var connectionString = isLocal 
    ? "Data Source=../test.sqlite;" 
    : Environment.GetEnvironmentVariable("DB_CONNECTIONSTRING");

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        
        services.AddDbContextFactory<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(connectionString!, b => 
                    b.MigrationsAssembly("CompileResultsUpdaterService"));
            }
            );
        
        services.AddHostedService<Worker>();
    });

IHost host = builder.Build();

using (var scope = host.Services.CreateScope()) // TODO: obviously, i think we need a migration service...
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<IDbContextFactory<ApplicationDbContext>>().CreateDbContext();
        context.Database.EnsureCreated();
        /*if (context.Database.GetPendingMigrations().Any())
        {
            await context.Database.MigrateAsync();
        }*/

    }
    catch (Exception e)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(e, "Some issues with db!");
    }
}

await host.RunAsync();