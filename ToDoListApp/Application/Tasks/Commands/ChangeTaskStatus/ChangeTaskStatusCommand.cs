using MediatR;
using TaskStatus = ToDo.Domain.Enums.TaskStatus;

namespace ToDo.Application.Tasks.Commands.ChangeTaskStatus;

public record ChangeTaskStatusCommand(Guid Id, TaskStatus Status) : IRequest;
