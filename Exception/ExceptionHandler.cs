using Microsoft.Extensions.Hosting;

public class ExceptionHandler
{
    private ExceptionHandler() { }

    private void UnhandledException(object error)
    {
        if (error is OptionsValidationException OptionsValidationException)
        {
            Console.Error.WriteLine($"{OptionsValidationException.Message}\n{OptionsValidationException.StackTrace}");
            Console.Error.WriteLine("");
            Console.Error.WriteLine($"Configuration validation error caused the application to terminate: {OptionsValidationException.Message}\nValidate application settings and try again.");
        }
        else if (error is Error Error)
        {
            Console.Error.WriteLine($"{Error.Message}\n{Error.StackTrace}{Error.ErrorStackTrace}");
            Console.Error.WriteLine("");
            Console.Error.WriteLine($"An error caused the application to terminate: {Error.Message}");
        }
        else if (error is Exception Exception)
        {
            if(Exception.InnerException is Error e) {
                UnhandledException(e);
                return;
            }
            Console.Error.WriteLine($"{Exception}");
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
        var Host = Builder.Build();
        try { await fn(Host); }
        catch (Exception e) {
            await Host.StopAsync();
            new ExceptionHandler().UnhandledException(e); 
        }
    }
}