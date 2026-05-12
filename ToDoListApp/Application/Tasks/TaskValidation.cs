using ToDo.Application.Common.Exceptions;
using ToDo.Domain.Entities;
using TaskStatus = ToDo.Domain.Enums.TaskStatus;

namespace ToDo.Application.Tasks;

internal static class TaskValidation
{
    public static void ValidateDetails(string title, string? description, DateTime? deadline)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(title))
        {
            errors[nameof(title)] = ["Title is required."];
        }
        else if (title.Trim().Length > TaskItem.MaxTitleLength)
        {
            errors[nameof(title)] = [$"Title cannot exceed {TaskItem.MaxTitleLength} characters."];
        }

        if (description?.Trim().Length > TaskItem.MaxDescriptionLength)
        {
            errors[nameof(description)] = [$"Description cannot exceed {TaskItem.MaxDescriptionLength} characters."];
        }

        if (deadline == default(DateTime))
        {
            errors[nameof(deadline)] = ["Deadline must be a valid date."];
        }

        ThrowIfAny(errors);
    }

    public static void ValidateStatus(TaskStatus status)
    {
        if (!Enum.IsDefined(status))
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                [nameof(status)] = ["Status must be Todo, InProgress, or Done."]
            });
        }
    }

    private static void ThrowIfAny(Dictionary<string, string[]> errors)
    {
        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }
    }
}
