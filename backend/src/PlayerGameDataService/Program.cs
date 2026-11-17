using Microsoft.EntityFrameworkCore;
using PlayerGameDataService.Data;
using PlayerGameDataService.Endpoints;
using PlayerGameDataService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddScoped<ScoreService>();

builder.Services.AddProblemDetails();
builder.Services.AddDbContext<LeaderboardDbContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("Leaderboard")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "v1"));
}

app.UseHttpsRedirection();

app.MapScoreEndpoints();
app.Run();