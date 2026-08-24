using InternTrack.Business.Interfaces;
using InternTrack.Business.Services;
using InternTrack.DataAccess;
using InternTrack.DataAccess.Context;
using InternTrack.DataAccess.Interfaces;
using InternTrack.DataAccess.Repositories;
using InternTrack.Api.Middleware;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite("Data Source=interntrack.db");
});

builder.Services.AddScoped<IInternRepository, InternRepository>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();

builder.Services.AddScoped<IInternService, InternService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseCors("AllowFrontend");

using (var scope = app.Services.CreateScope())
{
    var dbContext =
        scope.ServiceProvider.GetRequiredService<AppDbContext>();

    await DbSeeder.SeedAsync(dbContext);
}

app.MapControllers();

app.Run();