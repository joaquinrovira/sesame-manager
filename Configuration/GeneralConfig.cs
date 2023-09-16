using System.ComponentModel.DataAnnotations;

[Config]
public class GeneralConfig
{
    [Required]
    [EmailAddress]
    public string? Email { get; set; }

    [Required]
    public string? Password { get; set; }

    public HostEnvironment Environment { get; set; } = HostEnvironment.Development;
}

public enum HostEnvironment {
    Development,
    Production
}