using Commerce.Application.Features.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Commerce.Infrastructure.Persistence.Configurations;

public sealed class OrderPaidMessageConfiguration : IEntityTypeConfiguration<OrderPaidMessage>
{
    public void Configure(EntityTypeBuilder<OrderPaidMessage> builder)
    {
        builder.ToTable("OrderPaidOutbox");
        builder.HasKey(message => message.Id);
        builder.Property(message => message.Id).ValueGeneratedNever();
        builder.HasIndex(message => message.PaymentId).IsUnique();
        builder.HasIndex(message => message.OrderId).IsUnique();
        builder.Property(message => message.Amount).HasPrecision(18, 2);
        builder.Property(message => message.Currency).HasMaxLength(3).IsRequired();
    }
}
