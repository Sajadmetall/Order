using Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasConversion(
                    id => id.Value,
                    value => new OrderId(value))
                .ValueGeneratedNever();

            builder.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.OwnsMany(x => x.Items, item =>
            {
                item.ToTable("OrderItems");
            
                item.WithOwner().HasForeignKey("OrderId");
            
                item.Property<Guid>("Id");
                item.HasKey("Id");
            
                item.Property(i => i.ProductName).HasMaxLength(100);
                item.Property(i => i.Quantity);
            });


            // ✅ backing field access (good if you use private List<OrderItem> _items)
            builder.Navigation(x => x.Items)
                   .Metadata.SetPropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
