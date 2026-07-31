using Microsoft.EntityFrameworkCore;
using StayHub.Domain.Models;

namespace StayHub.Dal.Data;

public class StayHubDbContext(DbContextOptions<StayHubDbContext> options) : DbContext(options)
{
    public DbSet<Source> Sources => Set<Source>();
    public DbSet<Property> Properties => Set<Property>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<Guest> Guests => Set<Guest>();
    
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

            entity.HasIndex(x => new { x.SourceId, x.ExternalId })
                .IsUnique();

            entity.HasOne(x => x.Source)
                .WithMany(x => x.Reservations)
                .HasForeignKey(x => x.SourceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Guest)
                .WithMany(x => x.Reservations)
                .HasForeignKey(x => x.GuestId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(x => x.Status)
                .IsRequired();

            entity.HasOne(x => x.Property)
                .WithMany(x => x.Reservations)
                .HasForeignKey(x => x.PropertyId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        modelBuilder.Entity<Guest>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.LastName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(200);

            entity.HasIndex(x => x.Email)
                .IsUnique();

            entity.Property(x => x.Phone)
                .HasMaxLength(50);

            entity.Property(x => x.CreatedAtUtc)
                .IsRequired();
        });
    }
}
