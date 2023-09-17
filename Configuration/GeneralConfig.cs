using System.ComponentModel.DataAnnotations;

[Config]
public class GeneralConfig
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    public required string Password { get; set; }


    private string _TZ = TimeZoneInfo.Utc.Id;
    public string TZ
    {
        get => _TZ;
        init
        {
            // NOTE: throws for invalid/unkown timezone strings
            var info = Functional.Catch(() => TimeZoneInfo.FindSystemTimeZoneById(value)).GetOrThrow();
            _TZ = info.Id;
        }
    }




    public HostEnvironment Environment { get; set; } = HostEnvironment.Development;
}

public enum HostEnvironment
{
    Development,
    Production
}