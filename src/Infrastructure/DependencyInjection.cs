using Commerce.Application.Abstractions.Persistence;
using Commerce.Application.Abstractions.Messaging;
using Commerce.Infrastructure.Persistence;
using Commerce.Infrastructure.Persistence.Seeding;
using Commerce.Infrastructure.Messaging;
using MassTransit;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Commerce.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services)
        {
            services.AddOptions<ConnectionOptions>()
                .Configure<IConfiguration, IHostEnvironment>((options, configuration, environment) =>
                {
                    var development = environment.IsDevelopment();
                    options.SqlConnectionString = configuration.GetConnectionString("DefaultConnection")
                        ?? (development ? "Server=localhost\\SQLEXPRESS;Database=OrderDb;Trusted_Connection=True;TrustServerCertificate=True;" : "");
                    options.RabbitHost = configuration["RabbitMq:Host"] ?? (development ? "localhost" : "");
                    options.RabbitUsername = configuration["RabbitMq:Username"] ?? (development ? "guest" : "");
                    options.RabbitPassword = configuration["RabbitMq:Password"] ?? (development ? "guest" : "");
                    options.RabbitVirtualHost = configuration["RabbitMq:VirtualHost"] ?? "/";
                    options.RabbitPort = ushort.TryParse(configuration["RabbitMq:Port"] ?? "5672",
                        NumberStyles.Integer, CultureInfo.InvariantCulture, out var port) ? port : (ushort)0;
                })
                .Validate(options => !string.IsNullOrWhiteSpace(options.SqlConnectionString), "Configure ConnectionStrings:DefaultConnection.")
                .Validate(options => !string.IsNullOrWhiteSpace(options.RabbitHost), "Configure RabbitMq:Host.")
                .Validate(options => !string.IsNullOrWhiteSpace(options.RabbitUsername), "Configure RabbitMq:Username.")
                .Validate(options => !string.IsNullOrWhiteSpace(options.RabbitPassword), "Configure RabbitMq:Password.")
                .Validate(options => options.RabbitPort > 0, "RabbitMq:Port must be a number from 1 to 65535.")
                .ValidateOnStart();

            services.AddDbContext<OrderDbContext>((provider, options) =>
                options.UseSqlServer(provider.GetRequiredService<IOptions<ConnectionOptions>>().Value.SqlConnectionString));

            services.AddScoped<IApplicationDbContext>(sp =>
                      sp.GetRequiredService<OrderDbContext>());
            services.AddScoped<DatabaseSeeder>();

            services.AddScoped<IEventPublisher, MassTransitEventPublisher>();
            services.AddMassTransit(bus =>
            {
                bus.AddEntityFrameworkOutbox<OrderDbContext>(outbox =>
                {
                    outbox.UseSqlServer();
                    outbox.UseBusOutbox();
                });
                bus.UsingRabbitMq((context, transport) =>
                {
                    var settings = context.GetRequiredService<IOptions<ConnectionOptions>>().Value;
                    transport.Host(
                        settings.RabbitHost, settings.RabbitPort, settings.RabbitVirtualHost,
                        host =>
                        {
                            host.Username(settings.RabbitUsername);
                            host.Password(settings.RabbitPassword);
                        });
                });
            });

            return services;
        }

        private sealed class ConnectionOptions
        {
            public ConnectionOptions() { }

            public string SqlConnectionString { get; set; } = "";
            public string RabbitHost { get; set; } = "";
            public string RabbitUsername { get; set; } = "";
            public string RabbitPassword { get; set; } = "";
            public string RabbitVirtualHost { get; set; } = "";
            public ushort RabbitPort { get; set; }
        }
    }
}
