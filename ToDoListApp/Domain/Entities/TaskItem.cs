using TaskStatus = ToDo.Domain.Enums.TaskStatus;

namespace ToDo.Domain.Entities;

public class TaskItem
{
    public const int MaxTitleLength = 200;
    public const int MaxDescriptionLength = 1000;

    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public TaskStatus Status { get; private set; }
    public DateTime? Deadline { get; private set; }

    private TaskItem() { }

    public TaskItem(string title, string? description, DateTime? deadline)
    {
        Id = Guid.NewGuid();
        Title = ValidateTitle(title);
        Description = ValidateDescription(description);
        Status = TaskStatus.Todo;
        Deadline = ValidateDeadline(deadline);
    }

    public void UpdateDetails(string title, string? description, DateTime? deadline)
    {
        Title = ValidateTitle(title);
        Description = ValidateDescription(description);
        Deadline = ValidateDeadline(deadline);
    }

    public void ChangeStatus(TaskStatus newStatus)
    {
        if (!Enum.IsDefined(newStatus))
        {
            throw new ArgumentOutOfRangeException(nameof(newStatus), "Task status is not supported.");
        }

        Status = newStatus;
    }

    private static string ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Task title is required.", nameof(title));
        }

        var trimmedTitle = title.Trim();
        if (trimmedTitle.Length > MaxTitleLength)
        {
            throw new ArgumentException($"Task title cannot exceed {MaxTitleLength} characters.", nameof(title));
        }

        return trimmedTitle;
    }

    private static string? ValidateDescription(string? description)
    {
        if (description is null)
        {
            return null;
        }

        var trimmedDescription = description.Trim();
        if (trimmedDescription.Length > MaxDescriptionLength)
        {
            throw new ArgumentException($"Task description cannot exceed {MaxDescriptionLength} characters.", nameof(description));
        }

        return trimmedDescription.Length == 0 ? null : trimmedDescription;
    }

    private static DateTime? ValidateDeadline(DateTime? deadline)
    {
        if (deadline == default(DateTime))
        {
            throw new ArgumentException("Task deadline must be a valid date.", nameof(deadline));
        }

        return deadline;
    }
}
