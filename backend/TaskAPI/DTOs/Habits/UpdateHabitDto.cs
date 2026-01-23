using System.ComponentModel.DataAnnotations;

namespace TaskAPI.DTOs.Habits;

public class UpdateHabitDto
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 200 characters")]
    public string Title { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
