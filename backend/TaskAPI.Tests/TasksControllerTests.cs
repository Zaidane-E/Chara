using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskAPI.Controllers;

namespace TaskAPI.Tests;

public class TasksControllerTests
{
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    #region GET Tests

    [Fact]
    public async Task Get_ReturnsEmptyList_WhenNoTasks()
    {
        // Arrange
        using var context = CreateContext();
        var controller = new TasksController(context);

        // Act
        var result = await controller.Get();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var tasks = Assert.IsAssignableFrom<IEnumerable<TaskItem>>(okResult.Value);
        Assert.Empty(tasks);
    }

    [Fact]
    public async Task Get_ReturnsAllTasks_WhenTasksExist()
    {
        // Arrange
        using var context = CreateContext();
        context.Tasks.AddRange(
            new TaskItem { Title = "Task 1", CreatedAt = DateTime.UtcNow },
            new TaskItem { Title = "Task 2", CreatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var controller = new TasksController(context);

        // Act
        var result = await controller.Get();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var tasks = Assert.IsAssignableFrom<IEnumerable<TaskItem>>(okResult.Value);
        Assert.Equal(2, tasks.Count());
    }

    [Fact]
    public async Task Get_FiltersBy_IsCompleted()
    {
        // Arrange
        using var context = CreateContext();
        context.Tasks.AddRange(
            new TaskItem { Title = "Completed Task", IsCompleted = true, CreatedAt = DateTime.UtcNow },
            new TaskItem { Title = "Pending Task", IsCompleted = false, CreatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var controller = new TasksController(context);

        // Act
        var result = await controller.Get(isCompleted: true);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var tasks = Assert.IsAssignableFrom<IEnumerable<TaskItem>>(okResult.Value).ToList();
        Assert.Single(tasks);
        Assert.Equal("Completed Task", tasks[0].Title);
    }

    [Fact]
    public async Task Get_FiltersBy_Priority()
    {
        // Arrange
        using var context = CreateContext();
        context.Tasks.AddRange(
            new TaskItem { Title = "High Priority", Priority = Priority.High, CreatedAt = DateTime.UtcNow },
            new TaskItem { Title = "Low Priority", Priority = Priority.Low, CreatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var controller = new TasksController(context);

        // Act
        var result = await controller.Get(priority: Priority.High);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var tasks = Assert.IsAssignableFrom<IEnumerable<TaskItem>>(okResult.Value).ToList();
        Assert.Single(tasks);
        Assert.Equal("High Priority", tasks[0].Title);
    }

    [Fact]
    public async Task GetById_ReturnsTask_WhenTaskExists()
    {
        // Arrange
        using var context = CreateContext();
        var task = new TaskItem { Title = "Test Task" };
        context.Tasks.Add(task);
        await context.SaveChangesAsync();

        var controller = new TasksController(context);

        // Act
        var result = await controller.GetById(task.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedTask = Assert.IsType<TaskItem>(okResult.Value);
        Assert.Equal("Test Task", returnedTask.Title);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenTaskDoesNotExist()
    {
        // Arrange
        using var context = CreateContext();
        var controller = new TasksController(context);

        // Act
        var result = await controller.GetById(999);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    #endregion

    #region POST Tests

    [Fact]
    public async Task Create_AddsTaskWithDefaultValues()
    {
        // Arrange
        using var context = CreateContext();
        var controller = new TasksController(context);
        var dto = new CreateTaskDto { Title = "New Task" };

        // Act
        var result = await controller.Create(dto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var task = Assert.IsType<TaskItem>(createdResult.Value);
        Assert.Equal("New Task", task.Title);
        Assert.False(task.IsCompleted);
        Assert.Equal(Priority.Medium, task.Priority);
        Assert.Null(task.DueDate);
        Assert.Single(context.Tasks);
    }

    [Fact]
    public async Task Create_AddsTaskWithPriorityAndDueDate()
    {
        // Arrange
        using var context = CreateContext();
        var controller = new TasksController(context);
        var dueDate = DateTime.UtcNow.AddDays(7);
        var dto = new CreateTaskDto
        {
            Title = "High Priority Task",
            Priority = Priority.High,
            DueDate = dueDate
        };

        // Act
        var result = await controller.Create(dto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var task = Assert.IsType<TaskItem>(createdResult.Value);
        Assert.Equal("High Priority Task", task.Title);
        Assert.Equal(Priority.High, task.Priority);
        Assert.NotNull(task.DueDate);
        Assert.NotEqual(default, task.CreatedAt);
        Assert.NotEqual(default, task.UpdatedAt);
    }

    #endregion

    #region Toggle Tests

    [Fact]
    public async Task ToggleComplete_TogglesFromFalseToTrue()
    {
        // Arrange
        using var context = CreateContext();
        var task = new TaskItem { Title = "Task", IsCompleted = false };
        context.Tasks.Add(task);
        await context.SaveChangesAsync();

        var controller = new TasksController(context);

        // Act
        var result = await controller.ToggleComplete(task.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var toggledTask = Assert.IsType<TaskItem>(okResult.Value);
        Assert.True(toggledTask.IsCompleted);
    }

    [Fact]
    public async Task ToggleComplete_TogglesFromTrueToFalse()
    {
        // Arrange
        using var context = CreateContext();
        var task = new TaskItem { Title = "Task", IsCompleted = true };
        context.Tasks.Add(task);
        await context.SaveChangesAsync();

        var controller = new TasksController(context);

        // Act
        var result = await controller.ToggleComplete(task.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var toggledTask = Assert.IsType<TaskItem>(okResult.Value);
        Assert.False(toggledTask.IsCompleted);
    }

    [Fact]
    public async Task ToggleComplete_ReturnsNotFound_WhenTaskDoesNotExist()
    {
        // Arrange
        using var context = CreateContext();
        var controller = new TasksController(context);

        // Act
        var result = await controller.ToggleComplete(999);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    #endregion

    #region PUT Tests

    [Fact]
    public async Task Update_ModifiesAllFields()
    {
        // Arrange
        using var context = CreateContext();
        var task = new TaskItem
        {
            Title = "Original",
            IsCompleted = false,
            Priority = Priority.Low,
            DueDate = null
        };
        context.Tasks.Add(task);
        await context.SaveChangesAsync();

        var controller = new TasksController(context);
        var dueDate = DateTime.UtcNow.AddDays(3);
        var dto = new UpdateTaskDto
        {
            Title = "Updated",
            IsCompleted = true,
            Priority = Priority.High,
            DueDate = dueDate
        };

        // Act
        var result = await controller.Update(task.Id, dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var updatedTask = Assert.IsType<TaskItem>(okResult.Value);
        Assert.Equal("Updated", updatedTask.Title);
        Assert.True(updatedTask.IsCompleted);
        Assert.Equal(Priority.High, updatedTask.Priority);
        Assert.NotNull(updatedTask.DueDate);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenTaskDoesNotExist()
    {
        // Arrange
        using var context = CreateContext();
        var controller = new TasksController(context);
        var dto = new UpdateTaskDto { Title = "Title", IsCompleted = false };

        // Act
        var result = await controller.Update(999, dto);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    #endregion

    #region DELETE Tests

    [Fact]
    public async Task Delete_RemovesTask_WhenTaskExists()
    {
        // Arrange
        using var context = CreateContext();
        var task = new TaskItem { Title = "Task to Delete" };
        context.Tasks.Add(task);
        await context.SaveChangesAsync();

        var controller = new TasksController(context);

        // Act
        var result = await controller.Delete(task.Id);

        // Assert
        Assert.IsType<NoContentResult>(result);
        Assert.Empty(context.Tasks);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenTaskDoesNotExist()
    {
        // Arrange
        using var context = CreateContext();
        var controller = new TasksController(context);

        // Act
        var result = await controller.Delete(999);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    #endregion
}
