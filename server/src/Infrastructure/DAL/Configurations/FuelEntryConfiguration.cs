using Domain.Entities.EnergyEntries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DAL.Configurations;

public class FuelEntryConfiguration : IEntityTypeConfiguration<FuelEntry>
{
    public void Configure(EntityTypeBuilder<FuelEntry> builder)
    {
        // Base properties (Date, Mileage, Cost, VehicleId, relationship) 
        // are configured in EnergyEntryConfiguration

        // FuelEntry-specific properties only
        builder.Property(f => f.Volume)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(f => f.Unit)
            .IsRequired()
            .HasConversion<int>();

    }
}
