using System.ComponentModel.DataAnnotations;

[Config("HolidayProvider","Nager")]
public class NagerHolidayProviderConfiguration
{
    public required bool Enabled { get; init; } = true;

    [Required]
    public required string CountryCode { get; init; }
    public required string? CountyCode { get; init; }
}