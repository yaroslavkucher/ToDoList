using MediatR;
using Microsoft.EntityFrameworkCore;
using ToDo.Application.Common.Exceptions;
using ToDo.Application.Common.Interfaces;

namespace ToDo.Application.Tasks.Queries.GetTaskById;

public class GetTaskByIdQueryHandler : IRequestHandler<GetTaskByIdQuery, TaskDto>
{
    private readonly IToDoDbContext _context;

    public GetTaskByIdQueryHandler(IToDoDbContext context)
    {
        _context = context;
    }

    public async Task<TaskDto> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
    {
        var task = await _context.Tasks
            .AsNoTracking()
            .Where(task => task.Id == request.Id)
            .Select(task => new TaskDto(
                task.Id,
                task.Title,
                task.Description,
                task.Status,
                task.Deadline))
            .FirstOrDefaultAsync(cancellationToken);

        return task ?? throw new NotFoundException("Task", request.Id);
    }
}
