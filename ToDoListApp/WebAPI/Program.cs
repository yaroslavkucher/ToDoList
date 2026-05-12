using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ToDo.Application.Common.Exceptions;
using ToDo.Application.DependencyInjection;
using ToDo.Infrastructure.DependencyInjection;
using ValidationException = ToDo.Application.Common.Exceptions.ValidationException;

const string FrontendCorsPolicy = "Frontend";

var builder = WebApplication.CreateBuilder(args);

if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddInfrastructure(builder.Configuration);
}

builder.Services.AddApplication();

var frontendOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:5173", "http://localhost:3000"];

builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        policy
            .WithOrigins(frontendOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false));
    });

builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerPathFeature>()?.Error;

        var problemDetails = exception switch
        {
            ValidationException validationException => CreateValidationProblemDetails(validationException),
            NotFoundException notFoundException => CreateProblemDetails(StatusCodes.Status404NotFound, "Not Found", notFoundException.Message),
            _ => CreateProblemDetails(
                StatusCodes.Status500InternalServerError,
                "Unexpected Error",
                app.Environment.IsDevelopment()
                    ? exception?.ToString() ?? "An unexpected error occurred."
                    : "An unexpected error occurred.")
        };

        context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync((object)problemDetails, new JsonSerializerOptions(JsonSerializerDefaults.Web));
    });
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors(FrontendCorsPolicy);

app.UseAuthorization();

app.MapControllers();

app.Run();

static ValidationProblemDetails CreateValidationProblemDetails(ValidationException exception)
{
    return new ValidationProblemDetails(exception.Errors.ToDictionary(error => error.Key, error => error.Value))
    {
        Status = StatusCodes.Status400BadRequest,
        Title = "Validation Error",
        Detail = exception.Message
    };
}

static ProblemDetails CreateProblemDetails(int statusCode, string title, string detail)
{
    return new ProblemDetails
    {
        Status = statusCode,
        Title = title,
        Detail = detail
    };
}

public partial class Program;
