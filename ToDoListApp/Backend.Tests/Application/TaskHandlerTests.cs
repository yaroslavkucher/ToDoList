using Microsoft.EntityFrameworkCore;
using ToDo.Application.Common.Exceptions;
using ToDo.Application.Tasks.Commands.ChangeTaskStatus;
using ToDo.Application.Tasks.Commands.CreateTask;
using ToDo.Application.Tasks.Commands.DeleteTask;
using ToDo.Application.Tasks.Commands.UpdateTask;
using ToDo.Application.Tasks.Queries.GetTaskById;
using ToDo.Application.Tasks.Queries.GetTasks;
using ToDo.Domain.Entities;
using ToDo.Infrastructure;
using TaskStatus = ToDo.Domain.Enums.TaskStatus;
using ValidationException = ToDo.Application.Common.Exceptions.ValidationException;

namespace Todo.Backend.Tests.Application;

public class TaskHandlerTests
{
    [Fact]
    public async Task CreateTask_WithValidCommand_PersistsTaskWithTodoStatus()
    {
        await using var context = CreateContext();
        var handler = new CreateTaskCommandHandler(context);
        var deadline = new DateTime(2026, 5, 20, 12, 0, 0, DateTimeKind.Utc);

        var taskId = await handler.Handle(
            new CreateTaskCommand("  Prepare CRUD  ", "  Add handlers  ", deadline),
            CancellationToken.None);

        var task = await context.Tasks.SingleAsync();
        Assert.Equal(task.Id, taskId);
        Assert.Equal("Prepare CRUD", task.Title);
        Assert.Equal("Add handlers", task.Description);
        Assert.Equal(TaskStatus.Todo, task.Status);
        Assert.Equal(deadline, task.Deadline);
    }

    [Fact]
    public async Task CreateTask_WithInvalidTitle_ThrowsValidationException()
    {
        await using var context = CreateContext();
        var handler = new CreateTaskCommandHandler(context);

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            handler.Handle(new CreateTaskCommand(" ", null, null), CancellationToken.None));

        Assert.Contains("title", exception.Errors.Keys);
        Assert.Empty(context.Tasks);
    }

    [Fact]
    public async Task GetTasks_ReturnsTasksOrderedForListView()
    {
        await using var context = CreateContext();
        var doneTask = new TaskItem("Done task", null, null);
        doneTask.ChangeStatus(TaskStatus.Done);
        var todoTask = new TaskItem("Todo task", null, new DateTime(2026, 5, 20, 12, 0, 0, DateTimeKind.Utc));
        var inProgressTask = new TaskItem("In progress task", null, null);
        inProgressTask.ChangeStatus(TaskStatus.InProgress);

        context.Tasks.AddRange(doneTask, todoTask, inProgressTask);
        await context.SaveChangesAsync(CancellationToken.None);
        var handler = new GetTasksQueryHandler(context);

        var tasks = await handler.Handle(new GetTasksQuery(), CancellationToken.None);

        Assert.Equal(
            [TaskStatus.Todo, TaskStatus.InProgress, TaskStatus.Done],
            tasks.Select(task => task.Status));
    }

    [Fact]
    public async Task GetTaskById_WithExistingTask_ReturnsTask()
    {
        await using var context = CreateContext();
        var task = new TaskItem("Read one task", "Details", null);
        context.Tasks.Add(task);
        await context.SaveChangesAsync(CancellationToken.None);
        var handler = new GetTaskByIdQueryHandler(context);

        var result = await handler.Handle(new GetTaskByIdQuery(task.Id), CancellationToken.None);

        Assert.Equal(task.Id, result.Id);
        Assert.Equal(task.Title, result.Title);
        Assert.Equal(task.Description, result.Description);
    }

    [Fact]
    public async Task GetTaskById_WithMissingTask_ThrowsNotFoundException()
    {
        await using var context = CreateContext();
        var handler = new GetTaskByIdQueryHandler(context);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new GetTaskByIdQuery(Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task UpdateTask_WithExistingTask_UpdatesDetails()
    {
        await using var context = CreateContext();
        var task = new TaskItem("Old title", "Old description", null);
        context.Tasks.Add(task);
        await context.SaveChangesAsync(CancellationToken.None);
        var handler = new UpdateTaskCommandHandler(context);
        var deadline = new DateTime(2026, 5, 21, 12, 0, 0, DateTimeKind.Utc);

        await handler.Handle(
            new UpdateTaskCommand(task.Id, "  New title  ", "  New description  ", deadline),
            CancellationToken.None);

        var updatedTask = await context.Tasks.SingleAsync();
        Assert.Equal("New title", updatedTask.Title);
        Assert.Equal("New description", updatedTask.Description);
        Assert.Equal(deadline, updatedTask.Deadline);
    }

    [Fact]
    public async Task UpdateTask_WithMissingTask_ThrowsNotFoundException()
    {
        await using var context = CreateContext();
        var handler = new UpdateTaskCommandHandler(context);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new UpdateTaskCommand(Guid.NewGuid(), "Title", null, null), CancellationToken.None));
    }

    [Fact]
    public async Task ChangeTaskStatus_WithExistingTask_UpdatesStatus()
    {
        await using var context = CreateContext();
        var task = new TaskItem("Change status", null, null);
        context.Tasks.Add(task);
        await context.SaveChangesAsync(CancellationToken.None);
        var handler = new ChangeTaskStatusCommandHandler(context);

        await handler.Handle(new ChangeTaskStatusCommand(task.Id, TaskStatus.InProgress), CancellationToken.None);

        var updatedTask = await context.Tasks.SingleAsync();
        Assert.Equal(TaskStatus.InProgress, updatedTask.Status);
    }

    [Fact]
    public async Task ChangeTaskStatus_WithUnsupportedStatus_ThrowsValidationException()
    {
        await using var context = CreateContext();
        var task = new TaskItem("Change status", null, null);
        context.Tasks.Add(task);
        await context.SaveChangesAsync(CancellationToken.None);
        var handler = new ChangeTaskStatusCommandHandler(context);

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            handler.Handle(new ChangeTaskStatusCommand(task.Id, (TaskStatus)999), CancellationToken.None));

        Assert.Contains("status", exception.Errors.Keys);
    }

    [Fact]
    public async Task DeleteTask_WithExistingTask_RemovesTask()
    {
        await using var context = CreateContext();
        var task = new TaskItem("Delete task", null, null);
        context.Tasks.Add(task);
        await context.SaveChangesAsync(CancellationToken.None);
        var handler = new DeleteTaskCommandHandler(context);

        await handler.Handle(new DeleteTaskCommand(task.Id), CancellationToken.None);

        Assert.Empty(context.Tasks);
    }

    [Fact]
    public async Task DeleteTask_WithMissingTask_ThrowsNotFoundException()
    {
        await using var context = CreateContext();
        var handler = new DeleteTaskCommandHandler(context);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new DeleteTaskCommand(Guid.NewGuid()), CancellationToken.None));
    }

    private static ToDoDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ToDoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ToDoDbContext(options);
    }
}
