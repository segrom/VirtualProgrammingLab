using PythonCompilerService;

var runUser = new CodeRunner();

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