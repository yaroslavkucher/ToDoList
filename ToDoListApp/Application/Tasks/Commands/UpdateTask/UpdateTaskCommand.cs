using MediatR;

namespace ToDo.Application.Tasks.Commands.UpdateTask;

public record UpdateTaskCommand(
    Guid Id,
    string Title,
    string? Description,
    DateTime? Deadline) : IRequest;
