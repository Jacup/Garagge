using Domain.Entities.EnergyEntries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DAL.Configurations;

public class EnergyEntryConfiguration : IEntityTypeConfiguration<EnergyEntry>
{
    public void Configure(EntityTypeBuilder<EnergyEntry> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Date)
            .IsRequired();

        builder.Property(e => e.Mileage)
            .IsRequired();

        builder.Property(e => e.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.EnergyUnit)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.Volume)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(e => e.Cost)
            .HasPrecision(18, 2);

        builder.Property(c => c.PricePerUnit)
            .HasPrecision(18, 2);

        builder.HasOne(e => e.Vehicle)
            .WithMany(v => v.EnergyEntries)
            .HasForeignKey(e => e.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
