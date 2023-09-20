using System.ComponentModel.DataAnnotations;

[Config("WeeklySchedule")]
public class WeeklyScheduleConfig
{
    public WeeklyScheduleItem? Monday { get; init; }
    public WeeklyScheduleItem? Tuesday { get; init; }
    public WeeklyScheduleItem? Wednesday { get; init; }
    public WeeklyScheduleItem? Thursday { get; init; }
    public WeeklyScheduleItem? Friday { get; init; }
    public WeeklyScheduleItem? Saturday { get; init; }
    public WeeklyScheduleItem? Sunday { get; init; }

    public IDictionary<DayOfWeek, (TimeOfDay, TimeOfDay)> ToDict()
    {
        var dict = new Dictionary<DayOfWeek, (TimeOfDay, TimeOfDay)>();
        if (Monday is not null) dict.Add(DayOfWeek.Monday, new(new(Monday.Start!.Hour, Monday.Start.Minute), new(Monday.End!.Hour, Monday.End.Minute)));
        if (Tuesday is not null) dict.Add(DayOfWeek.Tuesday, new(new(Tuesday.Start!.Hour, Tuesday.Start.Minute), new(Tuesday.End!.Hour, Tuesday.End.Minute)));
        if (Wednesday is not null) dict.Add(DayOfWeek.Wednesday, new(new(Wednesday.Start!.Hour, Wednesday.Start.Minute), new(Wednesday.End!.Hour, Wednesday.End.Minute)));
        if (Thursday is not null) dict.Add(DayOfWeek.Thursday, new(new(Thursday.Start!.Hour, Thursday.Start.Minute), new(Thursday.End!.Hour, Thursday.End.Minute)));
        if (Friday is not null) dict.Add(DayOfWeek.Friday, new(new(Friday.Start!.Hour, Friday.Start.Minute), new(Friday.End!.Hour, Friday.End.Minute)));
        if (Saturday is not null) dict.Add(DayOfWeek.Saturday, new(new(Saturday.Start!.Hour, Saturday.Start.Minute), new(Saturday.End!.Hour, Saturday.End.Minute)));
        if (Sunday is not null) dict.Add(DayOfWeek.Sunday, new(new(Sunday.Start!.Hour, Sunday.Start.Minute), new(Sunday.End!.Hour, Sunday.End.Minute)));
        return dict;
    }

    public Maybe<WeeklyScheduleItem> For(DayOfWeek d)
    {
        return (d switch
        {
            DayOfWeek.Monday => Monday,
            DayOfWeek.Tuesday => Tuesday,
            DayOfWeek.Wednesday => Wednesday,
            DayOfWeek.Thursday => Thursday,
            DayOfWeek.Friday => Friday,
            DayOfWeek.Saturday => Saturday,
            DayOfWeek.Sunday => Sunday,
            _ => null,
        }).AsMaybe();
    }
}

public class WeeklyScheduleItem
{
    [Required]
    public required DayTimePoint Start { get; set; }

    [Required]
    public required DayTimePoint End { get; set; }

    public string? Site { get; set; }
}

public class DayTimePoint
{
    [Required]
    [Range(0, 23)]
    public int Hour { get; set; }

    [Range(0, 59)]
    public int Minute { get; set; } = 0;
}