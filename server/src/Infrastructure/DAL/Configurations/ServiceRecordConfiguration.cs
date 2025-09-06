using Domain.Entities.ServiceRecords;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DAL.Configurations;

public class ServiceRecordConfiguration : IEntityTypeConfiguration<ServiceRecord>
{
    public void Configure(EntityTypeBuilder<ServiceRecord> builder)
    {
        builder.HasKey(sr => sr.Id);

        builder.Property(sr => sr.Category)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(sr => sr.ServiceDate)
            .IsRequired();

        builder.Property(sr => sr.Mileage)
            .IsRequired(false);

        builder.Property(sr => sr.Cost)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(sr => sr.Notes)
            .HasMaxLength(1000);

        builder.HasOne(sr => sr.Vehicle)
            .WithMany(v => v.ServiceRecords)
            .HasForeignKey(sr => sr.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}