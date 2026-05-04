using Ambev.DeveloperEvaluation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambev.DeveloperEvaluation.ORM.Mapping;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasColumnType("uuid").HasDefaultValueSql("gen_random_uuid()");

        builder.Property(u => u.Username).IsRequired().HasMaxLength(50);
        builder.Property(u => u.Password).IsRequired().HasMaxLength(100);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(100);
        builder.Property(u => u.Phone).HasMaxLength(20);

        builder.Property(u => u.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(u => u.Role)
            .HasConversion<string>()
            .HasMaxLength(20);

        // Configure Name as owned entity
        builder.OwnsOne(u => u.Name, name =>
        {
            name.Property(n => n.Firstname)
                .HasColumnName("NameFirstname")
                .HasMaxLength(50);

            name.Property(n => n.Lastname)
                .HasColumnName("NameLastname")
                .HasMaxLength(50);
        });

        builder.Navigation(x => x.Name).IsRequired(false);

        // Configure Address as owned entity
        builder.OwnsOne(u => u.Address, address =>
        {
            address.Property(a => a.City)
                .HasColumnName("AddressCity")
                .HasMaxLength(100);

            address.Property(a => a.Street)
                .HasColumnName("AddressStreet")
                .HasMaxLength(200);

            address.Property(a => a.Number)
                .HasColumnName("AddressNumber");

            address.Property(a => a.Zipcode)
                .HasColumnName("AddressZipcode")
                .HasMaxLength(20);

            builder.Navigation(x => x.Address).IsRequired(false);

            // Configure Geolocation as nested owned entity
            address.OwnsOne(a => a.Geolocation, geo =>
            {
                geo.Property(g => g.Lat)
                    .HasColumnName("AddressGeolocationLat")
                    .HasMaxLength(50);

                geo.Property(g => g.Long)
                    .HasColumnName("AddressGeolocationLong")
                    .HasMaxLength(50);
            });

            address.Navigation(x => x.Geolocation).IsRequired(false);
        });
    }
}
