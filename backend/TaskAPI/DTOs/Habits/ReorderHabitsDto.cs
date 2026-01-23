using System.ComponentModel.DataAnnotations;

namespace TaskAPI.DTOs.Habits;

public class ReorderHabitsDto
{
    [Required]
    public List<int> HabitIds { get; set; } = new();
}
