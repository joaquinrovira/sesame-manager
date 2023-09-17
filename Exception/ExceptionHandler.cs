using Microsoft.Extensions.Hosting;

public class ExceptionHandler
{
    IHost Host;
    private ExceptionHandler(IHost host) { Host = host; }

    private async Task UnhandledException(object error)
    {
        await Host.StopAsync();
        if (error is OptionsValidationException OptionsValidationException)
        {
            Console.Error.WriteLine($"{OptionsValidationException.Message}\n{OptionsValidationException.StackTrace}");
            Console.Error.WriteLine("");
            Console.Error.WriteLine($"Configuration validation error caused the application to terminate: {OptionsValidationException.Message}\nValidate application settings and try again.");
        }
        else if (error is Error Error)
        {
            Console.Error.WriteLine($"{Error.Message}\n{Error.StackTrace}");
            Console.Error.WriteLine("");
            Console.Error.WriteLine($"An error caused the application to terminate: {Error.Message}");
        }
        else if (error is Exception Exception)
        {
            Console.Error.WriteLine($"{Exception.Message}\n{Exception.StackTrace}");
            Console.Error.WriteLine("");
            Console.Error.WriteLine($"Unknown Exception caused the application to terminate: {Exception.Message}");
        }
        else
        {
            Console.Error.WriteLine(error.ToString());
            Console.Error.WriteLine("");
            Console.Error.WriteLine("Unknown error caused the application to terminate.");
        }
        Environment.Exit(1);
    }

    public static async Task Manage(IHostBuilder Builder, Func<IHost, Task> fn)
    {
        var host = Builder.Build();
        try { await fn(host); }
        catch (Exception e) { await new ExceptionHandler(host).UnhandledException(e); }
    }
}