using System;
using Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure; // Add this using directive for DbContextOptions

namespace Infrastructure.Persistence
{
    public sealed class OrderDbContext : DbContext
    {
        public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options)
        {
        }

        public DbSet<Order> Orders => Set<Order>();
        //public DbSet<OrderItem> OrderItems { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Order>(order =>
            {
                order.HasKey(o => o.Id);

                order.Property(o => o.Id)
                     .HasConversion(
                         id => id.Value,
                         value => new OrderId(value));

                order.OwnsMany(o => o.Items, item =>
                {
                    // 🔑 FK to Order
                    item.WithOwner()
                        .HasForeignKey("OrderId");

                    // 🔑 PRIMARY KEY for OrderItem (THIS FIXES THE ERROR)
                    item.HasKey(i => i.Id);

                    item.Property(i => i.Id)
                        .ValueGeneratedNever();

                    item.Property(i => i.ProductName)
                        .IsRequired();

                    item.Property(i => i.Quantity)
                        .IsRequired();

                    // 💰 Value Object mapping
                    item.OwnsOne(i => i.UnitPrice, money =>
                    {
                        money.Property(m => m.Amount)
                             .HasColumnName("UnitPriceAmount");

                        money.Property(m => m.Currency)
                             .HasColumnName("UnitPriceCurrency");
                    });
                });
            });

        }
    }
}
