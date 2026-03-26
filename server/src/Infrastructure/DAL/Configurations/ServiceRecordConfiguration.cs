using Domain.Entities.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DAL.Configurations;

public class ServiceRecordConfiguration : IEntityTypeConfiguration<ServiceRecord>
{
    public void Configure(EntityTypeBuilder<ServiceRecord> builder)
    {
        builder.HasKey(sr => sr.Id);

        builder.Property(sr => sr.Title)
            .IsRequired()
            .HasMaxLength(64);

        builder
            .Property(sr => sr.Type)
            .IsRequired()
            .HasConversion<string>();
        
        builder.Property(sr => sr.Notes)
            .HasMaxLength(500);

        builder.Property(sr => sr.ServiceDate)
            .IsRequired();
        
        builder.Property(sr => sr.ManualCost)
            .HasPrecision(18, 2);
        
        builder
            .HasMany(sr => sr.Items)
            .WithOne(si => si.ServiceRecord)
            .HasForeignKey(si => si.ServiceRecordId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}