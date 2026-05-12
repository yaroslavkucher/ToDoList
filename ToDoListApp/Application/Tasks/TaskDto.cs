using TaskStatus = ToDo.Domain.Enums.TaskStatus;

namespace ToDo.Application.Tasks;

public record TaskDto(
    Guid Id,
    string Title,
    string? Description,
    TaskStatus Status,
    DateTime? Deadline);
