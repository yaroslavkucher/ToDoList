using MediatR;
using Microsoft.EntityFrameworkCore;
using ToDo.Application.Common.Exceptions;
using ToDo.Application.Common.Interfaces;

namespace ToDo.Application.Tasks.Commands.DeleteTask;

public class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand>
{
    private readonly IToDoDbContext _context;

    public DeleteTaskCommandHandler(IToDoDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _context.Tasks.FirstOrDefaultAsync(task => task.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Task", request.Id);

        _context.Tasks.Remove(task);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
