using Domain.Entities.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DAL.Configurations;

public class ServiceItemConfiguration : IEntityTypeConfiguration<ServiceItem>
{
    public void Configure(EntityTypeBuilder<ServiceItem> builder)
    {
        builder.HasKey(si => si.Id);
        
        builder.Property(si => si.Name)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(si => si.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(si => si.UnitPrice)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(si => si.Quantity)
            .IsRequired()
            .HasPrecision(10, 3);
        
        builder.Property(si => si.PartNumber)
            .HasMaxLength(64);
        
        builder.Property(si => si.Notes)
            .HasMaxLength(500);
        
    }
}