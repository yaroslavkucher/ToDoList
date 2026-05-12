using MediatR;

namespace ToDo.Application.Tasks.Queries.GetTasks;

public record GetTasksQuery : IRequest<IReadOnlyCollection<TaskDto>>;
