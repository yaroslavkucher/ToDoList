using ToDo.Domain.Entities;
using TaskStatus = ToDo.Domain.Enums.TaskStatus;

namespace Todo.Backend.Tests.Domain;

public class TaskItemTests
{
    [Fact]
    public void Constructor_WithValidDetails_TrimsValuesAndSetsTodoStatus()
    {
        var deadline = new DateTime(2026, 5, 20, 12, 0, 0, DateTimeKind.Utc);

        var task = new TaskItem("  Write tests  ", "  Backend CRUD  ", deadline);

        Assert.NotEqual(Guid.Empty, task.Id);
        Assert.Equal("Write tests", task.Title);
        Assert.Equal("Backend CRUD", task.Description);
        Assert.Equal(TaskStatus.Todo, task.Status);
        Assert.Equal(deadline, task.Deadline);
    }

    [Fact]
    public void Constructor_WithWhitespaceDescription_StoresNullDescription()
    {
        var task = new TaskItem("Write tests", "   ", null);

        Assert.Null(task.Description);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithBlankTitle_ThrowsArgumentException(string title)
    {
        var exception = Assert.Throws<ArgumentException>(() => new TaskItem(title, null, null));

        Assert.Equal("title", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithTooLongTitle_ThrowsArgumentException()
    {
        var title = new string('a', TaskItem.MaxTitleLength + 1);

        var exception = Assert.Throws<ArgumentException>(() => new TaskItem(title, null, null));

        Assert.Equal("title", exception.ParamName);
    }

    [Fact]
    public void UpdateDetails_WithValidDetails_UpdatesTask()
    {
        var task = new TaskItem("Old title", "Old description", null);
        var deadline = new DateTime(2026, 5, 21, 12, 0, 0, DateTimeKind.Utc);

        task.UpdateDetails("  New title  ", "  New description  ", deadline);

        Assert.Equal("New title", task.Title);
        Assert.Equal("New description", task.Description);
        Assert.Equal(deadline, task.Deadline);
    }

    [Fact]
    public void ChangeStatus_WithUnsupportedStatus_ThrowsArgumentOutOfRangeException()
    {
        var task = new TaskItem("Write tests", null, null);

        Assert.Throws<ArgumentOutOfRangeException>(() => task.ChangeStatus((TaskStatus)999));
    }
}
