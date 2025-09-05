using Domain.Entities.Vehicles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DAL.Configurations;
internal class VehicleEnergyTypeConfiguration : IEntityTypeConfiguration<VehicleEnergyType>
{
    public void Configure(EntityTypeBuilder<VehicleEnergyType> builder)
    {
        builder.HasKey(vet => vet.Id);

        builder.Property(vet => vet.EnergyType)
            .IsRequired()
            .HasConversion<int>();

        builder
            .HasOne(vet => vet.Vehicle)
            .WithMany(v => v.VehicleEnergyTypes)
            .HasForeignKey(vet => vet.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasIndex(vet => new { vet.VehicleId, vet.EnergyType })
            .IsUnique();
    }
}
