using System.ComponentModel.DataAnnotations;

[Config("WeeklySchedule")]
public class WeeklyScheduleConfig
{
    public DayTimeRange? Monday { get; init; }
    public DayTimeRange? Tuesday { get; init; }
    public DayTimeRange? Wednesday { get; init; }
    public DayTimeRange? Thursday { get; init; }
    public DayTimeRange? Friday { get; init; }
    public DayTimeRange? Saturday { get; init; }
    public DayTimeRange? Sunday { get; init; }

    public IDictionary<DayOfWeek, (TimeOfDay, TimeOfDay)> ToDict() 
    {
        var dict = new Dictionary<DayOfWeek, (TimeOfDay, TimeOfDay)>();
        if (Monday is not null) dict.Add(DayOfWeek.Monday, new(new(Monday.Start!.Hour, Monday.Start.Hour), new(Monday.End!.Hour, Monday.End.Hour)));
        if (Tuesday is not null) dict.Add(DayOfWeek.Tuesday, new(new(Tuesday.Start!.Hour, Tuesday.Start.Hour), new(Tuesday.End!.Hour, Tuesday.End.Hour)));
        if (Wednesday is not null) dict.Add(DayOfWeek.Wednesday, new(new(Wednesday.Start!.Hour, Wednesday.Start.Hour), new(Wednesday.End!.Hour, Wednesday.End.Hour)));
        if (Thursday is not null) dict.Add(DayOfWeek.Thursday, new(new(Thursday.Start!.Hour, Thursday.Start.Hour), new(Thursday.End!.Hour, Thursday.End.Hour)));
        if (Friday is not null) dict.Add(DayOfWeek.Friday, new(new(Friday.Start!.Hour, Friday.Start.Hour), new(Friday.End!.Hour, Friday.End.Hour)));
        if (Saturday is not null) dict.Add(DayOfWeek.Saturday, new(new(Saturday.Start!.Hour, Saturday.Start.Hour), new(Saturday.End!.Hour, Saturday.End.Hour)));
        if (Sunday is not null) dict.Add(DayOfWeek.Sunday, new(new(Sunday.Start!.Hour, Sunday.Start.Hour), new(Sunday.End!.Hour, Sunday.End.Hour)));
        return dict;
    }
}

public class DayTimeRange
{
    [Required]
    public DayTimePoint? Start { get; set; }

    [Required]
    public DayTimePoint? End { get; set; }
}

public class DayTimePoint
{
    [Required]
    [Range(0, 23)]
    public int Hour { get; set; }

    [Range(0, 59)]
    public int Minute { get; set; } = 0;
}