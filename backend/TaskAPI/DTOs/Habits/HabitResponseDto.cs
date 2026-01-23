namespace TaskAPI.DTOs.Habits;

public class HabitResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsCompletedToday { get; set; }
    public DateTime? LastCompletedAt { get; set; }
    public int CurrentStreak { get; set; }
    public int TotalCompletions { get; set; }
    public double CompletionRate { get; set; }
}
