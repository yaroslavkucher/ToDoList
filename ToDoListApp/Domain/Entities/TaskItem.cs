using ToDo.Domain.Enums;
using TaskStatus = ToDo.Domain.Enums.TaskStatus;

namespace ToDo.Domain.Entities;

public class TaskItem
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public TaskStatus Status { get; private set; }
    public DateTime? Deadline { get; private set; }

    private TaskItem() { }

    public TaskItem(string title, string? description, DateTime? deadline)
    {
        Id = Guid.NewGuid();
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Description = description;
        Status = TaskStatus.Todo;
        Deadline = deadline;
    }

    public void UpdateDetails(string title, string? description, DateTime? deadline)
    {
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Description = description;
        Deadline = deadline;
    }

    public void ChangeStatus(TaskStatus newStatus)
    {
        Status = newStatus;
    }
}