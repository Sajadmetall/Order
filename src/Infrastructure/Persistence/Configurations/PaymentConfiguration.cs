using Commerce.Domain.Orders;
using Commerce.Domain.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Commerce.Infrastructure.Persistence.Configurations;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        builder.HasKey(payment => payment.Id);
        builder.Property(payment => payment.Id)
            .HasConversion(id => id.Value, value => new PaymentId(value)).ValueGeneratedNever();
        builder.Property(payment => payment.OrderId)
            .HasConversion(id => id.Value, value => new OrderId(value));
        builder.HasIndex(payment => payment.OrderId).IsUnique();
        builder.HasOne<Order>().WithMany().HasForeignKey(payment => payment.OrderId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Property(payment => payment.Revision).IsConcurrencyToken();
        builder.Ignore(payment => payment.IsPaid);
        builder.OwnsOne(payment => payment.Amount, money =>
        {
            money.Property(amount => amount.Amount).HasColumnName("Amount").HasPrecision(18, 2);
            money.Property(amount => amount.Currency).HasColumnName("Currency").HasMaxLength(3);
        });
        builder.OwnsMany(payment => payment.Attempts, attempts =>
        {
            attempts.ToTable("PaymentAttempts");
            attempts.WithOwner().HasForeignKey("PaymentId");
            attempts.HasKey(attempt => attempt.Id);
            attempts.Property(attempt => attempt.Id).ValueGeneratedNever();
            attempts.Property(attempt => attempt.ProviderTransactionReference).HasMaxLength(200);
            attempts.HasIndex(attempt => attempt.ProviderTransactionReference)
                .IsUnique().HasFilter("[ProviderTransactionReference] IS NOT NULL");
        });
        builder.Navigation(payment => payment.Attempts)
            .HasField("_attempts").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
