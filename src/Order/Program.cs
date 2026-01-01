using Infrastructure;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Seeding;
using Microsoft.EntityFrameworkCore;
using Order;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// OpenAPI / Swagger service registrations
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// If your project/library provides an AddOpenApi() extension, keep it
builder.Services.AddOpenApi();

builder.Services.AddInfrastructure(builder.Configuration);
// Register middlewares before Build (DI registrations must happen before Build)
builder.Services.AddTransient<ExceptionHandlingMiddleware>();
builder.Services.AddTransient<CorrelationIdMiddleware>();
var app = builder.Build();

// Configure middleware pipeline after Build
if (app.Environment.IsDevelopment())
{
    // Enable Swagger generation and UI in Development
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Order API V1");
        // c.RoutePrefix = string.Empty; // uncomment to serve swagger UI at app root
    });

    // If your codebase requires MapOpenApi() for minimal APIs, keep it
    app.MapOpenApi();
}
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    await db.Database.MigrateAsync();

    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAsync();
}
//using (var scope = app.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();

//    // ✅ InMemory: EnsureCreated کافی است
//    // ✅ SQL: بهتر است Migrate (اگر migration داری)
//    await db.Database.EnsureCreatedAsync();

//    // اگر SQL و migration داری، این بهتره:
//    // await db.Database.MigrateAsync();

//    await DbInitializer.SeedAsync(db);
//}

app.UseMiddleware<CorrelationIdMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
