namespace TaskAPI.Models;

public class Habit
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public ICollection<HabitCompletion> Completions { get; set; } = new List<HabitCompletion>();
}
