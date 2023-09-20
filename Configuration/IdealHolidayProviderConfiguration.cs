using System.ComponentModel.DataAnnotations;

[Config("HolidayProvider", "Ideal")]
public class IdealHolidayProviderConfiguration
{
    public required bool Enabled { get; init; } = true;

    [Required]
    public required string CalendarPath { get; init; }
}