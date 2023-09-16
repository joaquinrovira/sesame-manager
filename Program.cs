using Microsoft.Extensions.Hosting;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices((hostContext, services) =>
{
    services.AddLogging(); // ILogger<T>
    services.AddHttpClient(); // IHttpClientFactory
    services.AddQuartz(); // ISchedulerFactory, IJobFactory
    services.AutoRegister<Program>();
});

using IHost host = builder.Build();
await host.RunAsync();

// No persistence! :)

// Get custom calendar dates from https://date.nager.at/PublicHoliday/Spain for county ES-CV or global

// On reboot between 1 jan and 31 dec, inclusive => check for or download current year holiday data
// On reboot or on schedule on 31 dec => check for or download calendar for next year 
// On schedule 1 of jan => clear previous year calendar dates from memory

// Configurable timezone, use local server otherwise
// Possibility to add custom holidays { Day: int, Month: int }

/**

Configurable deafult schedule: 

{
    Monday: {
        Start:  { Hour: int, Minute: int}, 
        End:    { Hour: int, Minute: int}
    },
    Tuesday: {
        Start:  { Hour: int, Minute: int}, 
        End:    { Hour: int, Minute: int}
    },
    Wednesday: {
        Start:  { Hour: int, Minute: int}, 
        End:    { Hour: int, Minute: int}
    },
    Thursday: {
        Start:  { Hour: int, Minute: int}, 
        End:    { Hour: int, Minute: int}
    },
    Friday: {
        Start:  { Hour: int, Minute: int}, 
        End:    { Hour: int, Minute: int}
    }
}

Additionally, special dates can be added that override the schedule:

[
    {
        Day:    { Day: int, Month: int },
        Start:  { Hour: int, Minute: int}, 
        End:    { Hour: int, Minute: int}
    }
]

**/