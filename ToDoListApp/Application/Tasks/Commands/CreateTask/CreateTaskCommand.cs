using MediatR;

namespace ToDo.Application.Tasks.Commands.CreateTask;

public record CreateTaskCommand(
    string Title,
    string? Description,
    DateTime? Deadline) : IRequest<Guid>;