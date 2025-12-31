using Domain.Orders;
using Infrastructure;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Order;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));

// OpenAPI / Swagger service registrations
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// If your project/library provides an AddOpenApi() extension, keep it
builder.Services.AddOpenApi();

// Use EF repository
builder.Services.AddScoped<IOrderRepository, EfOrderRepository>();

//// Configure EF Core DbContext. Update connection string in appsettings.json before using a real database.
//builder.Services.AddDbContext<OrderDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Server=localhost\\SQLEXPRESS;Database=OrderDb;Trusted_Connection=True;"));

// EF InMemory
builder.Services.AddDbContext<OrderDbContext>(opt =>
    opt.UseInMemoryDatabase("OrdersDb"));

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
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();

    // ✅ InMemory: EnsureCreated کافی است
    // ✅ SQL: بهتر است Migrate (اگر migration داری)
    await db.Database.EnsureCreatedAsync();

    // اگر SQL و migration داری، این بهتره:
    // await db.Database.MigrateAsync();

    await DbInitializer.SeedAsync(db);
}

app.UseMiddleware<CorrelationIdMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
