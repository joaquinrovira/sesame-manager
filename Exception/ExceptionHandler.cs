using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

public class ExceptionHandler
{
    IHost Host;
    private ExceptionHandler(IHost host) { Host = host; }

    private async Task UnhandledException(object exception) 
    {
        await Host.StopAsync();
        if(exception is OptionsValidationException e)
        {
            Console.Error.WriteLine($"{e.Message}\n{e.StackTrace}");
            Console.Error.WriteLine("");
            Console.Error.WriteLine($"Configuration validation error caused the application to terminate: {e.Message}\nValidate application settings and try again.");
        }
        else {
            Console.Error.WriteLine(exception.ToString());
            Console.Error.WriteLine("");
            Console.Error.WriteLine("Unknown error caused application to terminate.");
        }
        Environment.Exit(1);
    }

    public static async Task Manage(IHostBuilder Builder, Func<IHost, Task> fn) {
        var host = Builder.Build();
        try { await fn(host); }
        catch (Exception e) { await new ExceptionHandler(host).UnhandledException(e); }
    }
}