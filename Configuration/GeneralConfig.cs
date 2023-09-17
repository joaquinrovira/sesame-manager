using System.ComponentModel.DataAnnotations;

[Config]
public class GeneralConfig
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    public required string Password { get; set; }

    public HostEnvironment Environment { get; set; } = HostEnvironment.Development;
}

public enum HostEnvironment
{
    Development,
    Production
}