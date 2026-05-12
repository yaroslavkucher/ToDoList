using Microsoft.EntityFrameworkCore;
using ToDo.Domain.Entities;
using ToDo.Application.Common.Interfaces;
using ToDo.Infrastructure.Configurations;

namespace ToDo.Infrastructure;

public class ToDoDbContext : DbContext, IToDoDbContext
{
    public ToDoDbContext(DbContextOptions<ToDoDbContext> options) : base(options)
    {
    }

    public DbSet<TaskItem> Tasks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ToDoDbContext).Assembly);
    }
}