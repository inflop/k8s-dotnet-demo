using System.Data.SqlClient;
using Kube.Database.Api;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton(sp =>
{
    var connectionString = builder.Configuration.GetValue<string>("ConnectionString");
    return new TodoItemRepository(connectionString);
});

builder.Services.AddHealthChecks()
    .AddCheck(
        name: "Sql Server HealthCheck",
        instance: new SqlConnectionHealthCheck(builder.Configuration.GetValue<string>("ConnectionString")),
        failureStatus: HealthStatus.Unhealthy,
        tags: new string[] { "db" });

var app = builder.Build();

// Initialize database on startup
var connectionString = builder.Configuration.GetValue<string>("ConnectionString");
if (!string.IsNullOrEmpty(connectionString))
{
    var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger<DatabaseInitializer>();
    var dbInitializer = new DatabaseInitializer(connectionString, logger);
    await dbInitializer.InitializeAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthChecks("/hc");

app.UseRouting();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
