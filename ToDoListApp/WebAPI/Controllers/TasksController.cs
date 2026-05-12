using MediatR;
using Microsoft.AspNetCore.Mvc;
using ToDo.Application.Tasks;
using ToDo.Application.Tasks.Commands.ChangeTaskStatus;
using ToDo.Application.Tasks.Commands.CreateTask;
using ToDo.Application.Tasks.Commands.DeleteTask;
using ToDo.Application.Tasks.Commands.UpdateTask;
using ToDo.Application.Tasks.Queries.GetTaskById;
using ToDo.Application.Tasks.Queries.GetTasks;
using TaskStatus = ToDo.Domain.Enums.TaskStatus;

namespace ToDo.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly IMediator _mediator;

    public TasksController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<TaskDto>>> GetTasks(CancellationToken cancellationToken)
    {
        var tasks = await _mediator.Send(new GetTasksQuery(), cancellationToken);

        return Ok(tasks);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TaskDto>> GetTaskById(Guid id, CancellationToken cancellationToken)
    {
        var task = await _mediator.Send(new GetTaskByIdQuery(id), cancellationToken);

        return Ok(task);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> CreateTask(
        [FromBody] CreateTaskCommand command,
        CancellationToken cancellationToken)
    {
        var taskId = await _mediator.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetTaskById), new { id = taskId }, taskId);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateTask(
        Guid id,
        [FromBody] UpdateTaskRequest request,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new UpdateTaskCommand(id, request.Title, request.Description, request.Deadline),
            cancellationToken);

        return NoContent();
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> ChangeTaskStatus(
        Guid id,
        [FromBody] ChangeTaskStatusRequest request,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new ChangeTaskStatusCommand(id, request.Status), cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteTask(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteTaskCommand(id), cancellationToken);

        return NoContent();
    }
}

public record UpdateTaskRequest(
    string Title,
    string? Description,
    DateTime? Deadline);

public record ChangeTaskStatusRequest(TaskStatus Status);
