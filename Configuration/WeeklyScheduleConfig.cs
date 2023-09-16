using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

[Config("WeeklySchedule")]
public class WeeklyScheduleConfig
{
    [Required]
    public DayTimeRange? Monday { get; init; }
    public DayTimeRange? Tuesday { get; init; }
    public DayTimeRange? Wednesday { get; init; }
    public DayTimeRange? Thursday { get; init; }
    public DayTimeRange? Friday { get; init; }
    public DayTimeRange? Saturday { get; init; }
    public DayTimeRange? Sunday { get; init; }
}

public class DayTimeRange
{
    [Required]
    public DayTimePoint Start { get; set; } = new();

    [Required]
    public DayTimePoint End { get; set; } = new();
}

public class DayTimePoint
{
    [Required]
    [Range(0, 23)]
    public int Hour { get; set; }

    [Range(0, 59)]
    public int Minute { get; set; } = 0;
}