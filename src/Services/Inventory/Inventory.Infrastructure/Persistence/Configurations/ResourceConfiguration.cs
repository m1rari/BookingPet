using Inventory.Domain.Aggregates;
using Inventory.Domain.Enums;
using Inventory.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for Resource aggregate.
/// </summary>
public class ResourceConfiguration : IEntityTypeConfiguration<Resource>
{
    public void Configure(EntityTypeBuilder<Resource> builder)
    {
        builder.ToTable("Resources");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(r => r.Description)
            .HasMaxLength(2000);

        builder.Property(r => r.Type)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(r => r.PricePerHour)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(r => r.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        // Configure Location value object as owned entity
        builder.OwnsOne(r => r.Location, location =>
        {
            location.Property(l => l.Address)
                .HasColumnName("Address")
                .HasMaxLength(500)
                .IsRequired();

            location.Property(l => l.City)
                .HasColumnName("City")
                .HasMaxLength(100)
                .IsRequired();

            location.Property(l => l.Country)
                .HasColumnName("Country")
                .HasMaxLength(100)
                .IsRequired();

            location.Property(l => l.PostalCode)
                .HasColumnName("PostalCode")
                .HasMaxLength(20);

            location.Property(l => l.Latitude)
                .HasColumnName("Latitude");

            location.Property(l => l.Longitude)
                .HasColumnName("Longitude");
        });

        // Configure Capacity value object as owned entity
        builder.OwnsOne(r => r.Capacity, capacity =>
        {
            capacity.Property(c => c.MaxPeople)
                .HasColumnName("MaxPeople")
                .IsRequired();

            capacity.Property(c => c.MinPeople)
                .HasColumnName("MinPeople")
                .IsRequired();
        });

        // Configure AvailableSlots collection as owned entities
        builder.OwnsMany(r => r.AvailableSlots, slot =>
        {
            slot.ToTable("TimeSlots");

            slot.Property(s => s.StartTime)
                .IsRequired();

            slot.Property(s => s.EndTime)
                .IsRequired();

            slot.Property(s => s.Status)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            slot.WithOwner().HasForeignKey("ResourceId");
        });

        // Ignore domain events collection
        builder.Ignore(r => r.DomainEvents);

        // Indexes
        builder.HasIndex(r => r.Type);
        builder.HasIndex(r => r.Status);
    }
}

