using InternTrack.Business.Services;
using InternTrack.DataAccess;
using InternTrack.DataAccess.Context;
using InternTrack.DataAccess.Repositories;
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

builder.Services.AddScoped<InternRepository>();
builder.Services.AddScoped<TaskRepository>();
builder.Services.AddScoped<DepartmentRepository>();

builder.Services.AddScoped<InternService>();
builder.Services.AddScoped<TaskService>();
builder.Services.AddScoped<DepartmentService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowFrontend");

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    await DbSeeder.SeedAsync(dbContext);
}

app.MapControllers();

app.Run();