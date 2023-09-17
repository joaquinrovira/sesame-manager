using System.ComponentModel.DataAnnotations;

[Config("AdditionalHolidays")]
public class AdditionalHolidaysConfig : List<YearDay> {}

public record class YearDay {
    [Range(1,31)]
    public int Day {get; init;}

    [Range(1,12)]
    public int Month {get; init;}
}