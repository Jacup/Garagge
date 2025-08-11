using Domain.Entities.EnergyEntries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DAL.Configurations;

public class EnergyEntryConfiguration : IEntityTypeConfiguration<EnergyEntry>
{
    public void Configure(EntityTypeBuilder<EnergyEntry> builder)
    {
        builder.HasKey(e => e.Id);

        // Common properties from EnergyEntry base class
        builder.Property(e => e.Date)
            .IsRequired();

        builder.Property(e => e.Mileage)
            .IsRequired();

        builder.Property(e => e.Cost)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(e => e.VehicleId)
            .IsRequired();
        
        // Configure Table Per Type (TPT) inheritance
        builder.UseTptMappingStrategy();
    }
}
