using Microsoft.EntityFrameworkCore;
using PulseHub.Domain.Models;

namespace PulseHub.Dal.Data;

public class PulseHubDbContext(DbContextOptions<PulseHubDbContext> options) : DbContext(options)
{
    public DbSet<Source> Sources => Set<Source>();
    public DbSet<Property> Properties => Set<Property>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Source>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(x => x.SourceType)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.Url)
                .HasMaxLength(1000);

            entity.Property(x => x.CreatedAtUtc)
                .IsRequired();
        });
        
        modelBuilder.Entity<Property>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(x => x.City)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.CountryCode)
                .IsRequired()
                .HasMaxLength(2);
        });

        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.ExternalId)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasIndex(x => x.ExternalId)
                .IsUnique();

            entity.Property(x => x.GuestName)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(x => x.Status)
                .IsRequired();

            entity.HasOne(x => x.Property)
                .WithMany(x => x.Reservations)
                .HasForeignKey(x => x.PropertyId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}