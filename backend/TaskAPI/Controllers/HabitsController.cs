using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskAPI.DTOs.Habits;
using TaskAPI.Models;

namespace TaskAPI.Controllers;

[ApiController]
[Route("api/habits")]
[Authorize]
public class HabitsController : ControllerBase
{
    private readonly AppDbContext _context;

    public HabitsController(AppDbContext context)
    {
        _context = context;
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] bool? isActive = null)
    {
        var userId = GetUserId();
        var query = _context.Habits
            .Include(h => h.Completions)
            .Where(h => h.UserId == userId);

        if (isActive.HasValue)
            query = query.Where(h => h.IsActive == isActive.Value);

        var habits = await query.OrderBy(h => h.SortOrder).ThenByDescending(h => h.CreatedAt).ToListAsync();
        var response = habits.Select(h => MapToResponseDto(h)).ToList();
        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userId = GetUserId();
        var habit = await _context.Habits
            .Include(h => h.Completions)
            .FirstOrDefaultAsync(h => h.Id == id && h.UserId == userId);

        if (habit == null)
            return NotFound();

        return Ok(MapToResponseDto(habit));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateHabitDto dto)
    {
        var userId = GetUserId();

        // Get the max sort order for this user
        var maxSortOrder = await _context.Habits
            .Where(h => h.UserId == userId)
            .MaxAsync(h => (int?)h.SortOrder) ?? -1;

        var habit = new Habit
        {
            Title = dto.Title,
            UserId = userId,
            SortOrder = maxSortOrder + 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Habits.Add(habit);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = habit.Id }, MapToResponseDto(habit));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateHabitDto dto)
    {
        var userId = GetUserId();
        var habit = await _context.Habits
            .Include(h => h.Completions)
            .FirstOrDefaultAsync(h => h.Id == id && h.UserId == userId);

        if (habit == null)
            return NotFound();

        habit.Title = dto.Title;
        habit.IsActive = dto.IsActive;
        habit.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(MapToResponseDto(habit));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetUserId();
        var habit = await _context.Habits.FirstOrDefaultAsync(h => h.Id == id && h.UserId == userId);

        if (habit == null)
            return NotFound();

        _context.Habits.Remove(habit);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("reorder")]
    public async Task<IActionResult> Reorder(ReorderHabitsDto dto)
    {
        var userId = GetUserId();
        var habits = await _context.Habits
            .Where(h => h.UserId == userId && dto.HabitIds.Contains(h.Id))
            .ToListAsync();

        if (habits.Count != dto.HabitIds.Count)
            return BadRequest(new { message = "Some habit IDs are invalid" });

        for (int i = 0; i < dto.HabitIds.Count; i++)
        {
            var habit = habits.First(h => h.Id == dto.HabitIds[i]);
            habit.SortOrder = i;
            habit.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        var updatedHabits = await _context.Habits
            .Include(h => h.Completions)
            .Where(h => h.UserId == userId)
            .OrderBy(h => h.SortOrder)
            .ToListAsync();

        return Ok(updatedHabits.Select(h => MapToResponseDto(h)).ToList());
    }

    [HttpPost("{id}/complete")]
    public async Task<IActionResult> CompleteHabit(int id)
    {
        var userId = GetUserId();
        var habit = await _context.Habits
            .Include(h => h.Completions)
            .FirstOrDefaultAsync(h => h.Id == id && h.UserId == userId);

        if (habit == null)
            return NotFound();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        if (IsCompletedToday(habit, today))
            return BadRequest(new { message = "Habit already completed today" });

        var now = DateTime.UtcNow;
        var completion = new HabitCompletion
        {
            HabitId = habit.Id,
            CompletedAt = now,
            CompletedDate = today,
            Year = today.Year,
            Month = today.Month,
            Week = ISOWeek.GetWeekOfYear(now)
        };

        _context.HabitCompletions.Add(completion);
        habit.Completions.Add(completion);
        await _context.SaveChangesAsync();

        return Ok(MapToResponseDto(habit));
    }

    [HttpDelete("{id}/complete")]
    public async Task<IActionResult> UncompleteHabit(int id)
    {
        var userId = GetUserId();
        var habit = await _context.Habits
            .Include(h => h.Completions)
            .FirstOrDefaultAsync(h => h.Id == id && h.UserId == userId);

        if (habit == null)
            return NotFound();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var completion = habit.Completions?.FirstOrDefault(c => c.CompletedDate == today);

        if (completion == null)
            return BadRequest(new { message = "Habit not completed today" });

        _context.HabitCompletions.Remove(completion);
        await _context.SaveChangesAsync();

        habit.Completions.Remove(completion);
        return Ok(MapToResponseDto(habit));
    }

    [HttpGet("{id}/completions")]
    public async Task<IActionResult> GetCompletions(int id, [FromQuery] int days = 30)
    {
        var userId = GetUserId();
        var habit = await _context.Habits.FirstOrDefaultAsync(h => h.Id == id && h.UserId == userId);

        if (habit == null)
            return NotFound();

        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-days));
        var completions = await _context.HabitCompletions
            .Where(c => c.HabitId == id && c.CompletedDate >= startDate)
            .OrderByDescending(c => c.CompletedDate)
            .Select(c => new HabitCompletionResponseDto
            {
                Id = c.Id,
                HabitId = c.HabitId,
                CompletedAt = c.CompletedAt,
                CompletedDate = c.CompletedDate
            })
            .ToListAsync();

        return Ok(completions);
    }

    [HttpGet("{id}/stats")]
    public async Task<IActionResult> GetStats(int id, [FromQuery] int days = 30)
    {
        var userId = GetUserId();
        var habit = await _context.Habits
            .Include(h => h.Completions)
            .FirstOrDefaultAsync(h => h.Id == id && h.UserId == userId);

        if (habit == null)
            return NotFound();

        var stats = CalculateStats(habit, days);
        return Ok(stats);
    }

    private HabitResponseDto MapToResponseDto(Habit habit)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var createdDate = DateOnly.FromDateTime(habit.CreatedAt);
        var daysSinceCreation = today.DayNumber - createdDate.DayNumber + 1;
        var totalCompletions = habit.Completions?.Count ?? 0;
        var completionRate = daysSinceCreation > 0 ? (double)totalCompletions / daysSinceCreation * 100 : 0;

        return new HabitResponseDto
        {
            Id = habit.Id,
            Title = habit.Title,
            IsActive = habit.IsActive,
            SortOrder = habit.SortOrder,
            CreatedAt = habit.CreatedAt,
            UpdatedAt = habit.UpdatedAt,
            IsCompletedToday = IsCompletedToday(habit, today),
            LastCompletedAt = habit.Completions?.MaxBy(c => c.CompletedAt)?.CompletedAt,
            CurrentStreak = CalculateCurrentStreak(habit),
            TotalCompletions = totalCompletions,
            CompletionRate = Math.Round(completionRate, 1)
        };
    }

    private bool IsCompletedToday(Habit habit, DateOnly today)
    {
        return habit.Completions?.Any(c => c.CompletedDate == today) ?? false;
    }

    private int CalculateCurrentStreak(Habit habit)
    {
        if (habit.Completions == null || !habit.Completions.Any())
            return 0;

        var sortedDates = habit.Completions
            .Select(c => c.CompletedDate)
            .Distinct()
            .OrderByDescending(d => d)
            .ToList();

        if (!sortedDates.Any())
            return 0;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var streak = 0;
        var expectedDate = today;

        foreach (var date in sortedDates)
        {
            if (date == expectedDate || date == expectedDate.AddDays(-1))
            {
                streak++;
                expectedDate = date.AddDays(-1);
            }
            else if (streak == 0 && date == today.AddDays(-1))
            {
                streak++;
                expectedDate = date.AddDays(-1);
            }
            else
            {
                break;
            }
        }

        return streak;
    }

    private HabitStatsDto CalculateStats(Habit habit, int days)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var startDate = today.AddDays(-days);

        var completionDates = habit.Completions?
            .Where(c => c.CompletedDate >= startDate)
            .Select(c => c.CompletedDate)
            .ToHashSet() ?? new HashSet<DateOnly>();

        var history = new List<DailyCompletionDto>();
        for (var date = startDate; date <= today; date = date.AddDays(1))
        {
            history.Add(new DailyCompletionDto
            {
                Date = date,
                Completed = completionDates.Contains(date)
            });
        }

        return new HabitStatsDto
        {
            HabitId = habit.Id,
            HabitTitle = habit.Title,
            TotalCompletions = habit.Completions?.Count ?? 0,
            CurrentStreak = CalculateCurrentStreak(habit),
            LongestStreak = 0,
            CompletionRateLastMonth = days > 0 ? (double)completionDates.Count / days * 100 : 0,
            CompletionHistory = history
        };
    }
}
