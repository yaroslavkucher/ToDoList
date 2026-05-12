using MediatR;
using Microsoft.EntityFrameworkCore;
using ToDo.Application.Common.Interfaces;

namespace ToDo.Application.Tasks.Queries.GetTasks;

public class GetTasksQueryHandler : IRequestHandler<GetTasksQuery, IReadOnlyCollection<TaskDto>>
{
    private readonly IToDoDbContext _context;

    public GetTasksQueryHandler(IToDoDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<TaskDto>> Handle(GetTasksQuery request, CancellationToken cancellationToken)
    {
        return await _context.Tasks
            .AsNoTracking()
            .OrderBy(task => task.Status)
            .ThenBy(task => task.Deadline)
            .ThenBy(task => task.Title)
            .Select(task => new TaskDto(
                task.Id,
                task.Title,
                task.Description,
                task.Status,
                task.Deadline))
            .ToListAsync(cancellationToken);
    }
}
