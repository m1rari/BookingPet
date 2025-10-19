using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payment.Domain.Enums;
using Payment.Domain.ValueObjects;

namespace Payment.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for Payment aggregate.
/// </summary>
public class PaymentConfiguration : IEntityTypeConfiguration<Domain.Aggregates.Payment>
{
    public void Configure(EntityTypeBuilder<Domain.Aggregates.Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.BookingId).IsRequired();
        builder.Property(p => p.UserId).IsRequired();
        builder.Property(p => p.ExternalTransactionId).HasMaxLength(200);
        builder.Property(p => p.PaymentMethod).HasMaxLength(50);
        builder.Property(p => p.FailureReason).HasMaxLength(1000);
        builder.Property(p => p.CreatedAt).IsRequired();

        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        // Configure Money value object as owned entity
        builder.OwnsOne(p => p.Amount, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("Amount")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("Currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        // Ignore domain events
        builder.Ignore(p => p.DomainEvents);

        // Indexes
        builder.HasIndex(p => p.BookingId);
        builder.HasIndex(p => p.UserId);
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.ExternalTransactionId);
    }
}

