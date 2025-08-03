using Domain.Entities.Vehicles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DAL.Configurations;

public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Brand)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(v => v.Model)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(v => v.PowerType)
            .IsRequired();
        
        builder.Property(v => v.VIN)
            .HasMaxLength(17);

        builder
            .HasMany(v => v.EnergyEntries)
            .WithOne(e => e.Vehicle)
            .HasForeignKey(e => e.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}