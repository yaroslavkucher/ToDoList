using MediatR;
using Microsoft.EntityFrameworkCore;
using ToDo.Application.Common.Exceptions;
using ToDo.Application.Common.Interfaces;

namespace ToDo.Application.Tasks.Commands.UpdateTask;

public class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand>
{
    private readonly IToDoDbContext _context;

    public UpdateTaskCommandHandler(IToDoDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        TaskValidation.ValidateDetails(request.Title, request.Description, request.Deadline);

        var task = await _context.Tasks.FirstOrDefaultAsync(task => task.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Task", request.Id);

        task.UpdateDetails(request.Title, request.Description, request.Deadline);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
