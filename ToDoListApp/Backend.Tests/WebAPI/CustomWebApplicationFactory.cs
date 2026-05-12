using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToDo.Application.Common.Interfaces;
using ToDo.Infrastructure;

namespace Todo.Backend.Tests.WebAPI;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<ToDoDbContext>();
            services.RemoveAll<DbContextOptions<ToDoDbContext>>();
            services.RemoveAll<IDatabaseProvider>();
            services.RemoveAll<IToDoDbContext>();

            services.AddDbContext<ToDoDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));

            services.AddScoped<IToDoDbContext>(provider => provider.GetRequiredService<ToDoDbContext>());
        });
    }
}
