using Common;
using CompileResultsUpdaterService;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddDbContextFactory<ApplicationDbContext>(options => 
            options.UseNpgsql(Environment.GetEnvironmentVariable("DB_CONNECTIONSTRING")!)
            );
        
        services.AddHostedService<Worker>();
    });

IHost host =builder.Build();

using (var scope = host.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<IDbContextFactory<ApplicationDbContext>>().CreateDbContext();
        if (context.Database.GetPendingMigrations().Any())
        {
            await context.Database.MigrateAsync();
        }
        
    }
    catch (Exception e)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(e, "An error occurred while seeding the database.");
    }
}

await host.RunAsync();