using Microsoft.Extensions.Hosting;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices((context, services) =>
{
    services.AddLogging();                                              // ILogger<T>
    services.AddHttpClient();                                           // IHttpClientFactory
    services.AddQuartz();                                               // ISchedulerFactory, IJobFactory
    services.AutoRegisterServices<Program>();                           // Classes with the [Service] attribute
    services.AutoRegisterConfiguration<Program>(context.Configuration); // Classes with the [Config] attribute
});

await ExceptionHandler.Manage(builder, (host) => host.RunAsync());