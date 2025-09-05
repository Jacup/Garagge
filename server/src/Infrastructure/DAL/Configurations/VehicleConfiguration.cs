using Domain.Entities;
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
            .IsRequired()
            .HasConversion<int>();

        builder.Property(v => v.Type)
            .HasConversion<int?>();

        builder.Property(v => v.VIN)
            .HasMaxLength(17);

        builder.HasOne(v => v.User)
             .WithMany(u => u.Vehicles)
             .HasForeignKey(v => v.UserId)
             .OnDelete(DeleteBehavior.Cascade);
    }
}