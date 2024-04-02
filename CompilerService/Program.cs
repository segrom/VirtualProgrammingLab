using CompilerService;

var runUser = new RunUser();

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices(
        services =>
        {
            services.AddHostedService<RabbitListener>();
        })
    .ConfigureAppConfiguration(builder =>
    {
        builder.Build();
    });


var host = builder.Build();

await host.RunAsync();