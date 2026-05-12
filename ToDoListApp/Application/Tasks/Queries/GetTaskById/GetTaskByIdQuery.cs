using MediatR;

namespace ToDo.Application.Tasks.Queries.GetTaskById;

public record GetTaskByIdQuery(Guid Id) : IRequest<TaskDto>;
