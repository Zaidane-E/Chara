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
            new TaskItem { Title = "Task 1" },
            new TaskItem { Title = "Task 2" }
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

    [Fact]
    public async Task Create_AddsTaskAndReturnsCreatedResult()
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
        Assert.Single(context.Tasks);
    }

    [Fact]
    public async Task Update_ModifiesTask_WhenTaskExists()
    {
        // Arrange
        using var context = CreateContext();
        var task = new TaskItem { Title = "Original Title", IsCompleted = false };
        context.Tasks.Add(task);
        await context.SaveChangesAsync();

        var controller = new TasksController(context);
        var dto = new UpdateTaskDto { Title = "Updated Title", IsCompleted = true };

        // Act
        var result = await controller.Update(task.Id, dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var updatedTask = Assert.IsType<TaskItem>(okResult.Value);
        Assert.Equal("Updated Title", updatedTask.Title);
        Assert.True(updatedTask.IsCompleted);
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
}
