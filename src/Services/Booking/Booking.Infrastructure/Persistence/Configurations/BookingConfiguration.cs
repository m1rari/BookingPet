using Booking.Domain.Enums;
using Booking.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Booking.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for Booking aggregate with optimistic concurrency.
/// </summary>
public class BookingConfiguration : IEntityTypeConfiguration<Domain.Aggregates.Booking>
{
    public void Configure(EntityTypeBuilder<Domain.Aggregates.Booking> builder)
    {
        builder.ToTable("Bookings");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.ResourceId)
            .IsRequired();

        builder.Property(b => b.UserId)
            .IsRequired();

        builder.Property(b => b.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(b => b.CancellationReason)
            .HasMaxLength(1000);

        builder.Property(b => b.CreatedAt)
            .IsRequired();

        builder.Property(b => b.ConfirmedAt);

        builder.Property(b => b.CancelledAt);

        // Optimistic Concurrency Token
        builder.Property(b => b.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        // Configure BookingPeriod value object as owned entity
        builder.OwnsOne(b => b.Period, period =>
        {
            period.Property(p => p.StartTime)
                .HasColumnName("StartTime")
                .IsRequired();

            period.Property(p => p.EndTime)
                .HasColumnName("EndTime")
                .IsRequired();
        });

        // Configure Money value object as owned entity
        builder.OwnsOne(b => b.TotalPrice, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("TotalPrice")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("Currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        // Ignore domain events collection
        builder.Ignore(b => b.DomainEvents);

        // Indexes
        builder.HasIndex(b => b.ResourceId);
        builder.HasIndex(b => b.UserId);
        builder.HasIndex(b => b.Status);
        builder.HasIndex(b => b.CreatedAt);
    }
}


