using MediatR;
using Microsoft.EntityFrameworkCore;
using ToDo.Application.Common.Exceptions;
using ToDo.Application.Common.Interfaces;

namespace ToDo.Application.Tasks.Commands.ChangeTaskStatus;

public class ChangeTaskStatusCommandHandler : IRequestHandler<ChangeTaskStatusCommand>
{
    private readonly IToDoDbContext _context;

    public ChangeTaskStatusCommandHandler(IToDoDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ChangeTaskStatusCommand request, CancellationToken cancellationToken)
    {
        TaskValidation.ValidateStatus(request.Status);

        var task = await _context.Tasks.FirstOrDefaultAsync(task => task.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Task", request.Id);

        task.ChangeStatus(request.Status);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
