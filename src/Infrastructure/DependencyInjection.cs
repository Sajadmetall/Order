using Commerce.Application.Abstractions.Persistence;
using Commerce.Domain.Orders;
using Commerce.Infrastructure.Persistence;
using Commerce.Infrastructure.Persistence.Repositories;
using Commerce.Infrastructure.Persistence.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Commerce.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Configure EF Core DbContext. Update connection string in appsettings.json before using a real database.
            services.AddDbContext<OrderDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection") ?? "Server=localhost\\SQLEXPRESS;Database=OrderDb;Trusted_Connection=True;TrustServerCertificate=True;"));

            //EF InMemory
            //builder.Services.AddDbContext<OrderDbContext>(opt =>
            //    opt.UseInMemoryDatabase("OrdersDb"));

            // Use EF repository
            services.AddScoped<IOrderRepository, EfOrderRepository>();
            services.AddScoped<IApplicationDbContext>(sp =>
                      sp.GetRequiredService<OrderDbContext>());
            services.AddScoped<DatabaseSeeder>();

            return services;
        }
    }
}
