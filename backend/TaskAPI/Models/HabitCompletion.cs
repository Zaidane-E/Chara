namespace TaskAPI.Models;

public class HabitCompletion
{
    public int Id { get; set; }
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
    public DateOnly CompletedDate { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public int Week { get; set; }

    public int HabitId { get; set; }
    public Habit Habit { get; set; } = null!;
}
