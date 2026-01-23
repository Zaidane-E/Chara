namespace TaskAPI.DTOs.Habits;

public class HabitCompletionResponseDto
{
    public int Id { get; set; }
    public int HabitId { get; set; }
    public DateTime CompletedAt { get; set; }
    public DateOnly CompletedDate { get; set; }
}
