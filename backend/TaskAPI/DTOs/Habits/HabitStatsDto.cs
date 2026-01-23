namespace TaskAPI.DTOs.Habits;

public class HabitStatsDto
{
    public int HabitId { get; set; }
    public string HabitTitle { get; set; } = string.Empty;
    public int TotalCompletions { get; set; }
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public double CompletionRateLastMonth { get; set; }
    public List<DailyCompletionDto> CompletionHistory { get; set; } = new();
}

public class DailyCompletionDto
{
    public DateOnly Date { get; set; }
    public bool Completed { get; set; }
}
