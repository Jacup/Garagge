using Domain.Entities.EnergyEntries;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DAL.Configurations;

public class ChargingEntryConfiguration : IEntityTypeConfiguration<ChargingEntry>
{
    public void Configure(EntityTypeBuilder<ChargingEntry> builder)
    {
        // Base properties (Date, Mileage, Cost, VehicleId, relationship) 
        // are configured in EnergyEntryConfiguration

        // ChargingEntry-specific properties only
        builder.Property(c => c.EnergyAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(c => c.Unit)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(c => c.PricePerUnit)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(c => c.ChargingDurationMinutes)
            .IsRequired(false);
    }
}
