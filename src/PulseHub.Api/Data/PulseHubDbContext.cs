using Microsoft.EntityFrameworkCore;
using PulseHub.Api.Models;

namespace PulseHub.Api.Data;

public class PulseHubDbContext(DbContextOptions<PulseHubDbContext> options) : DbContext(options)
{
    public DbSet<Source> Sources => Set<Source>();

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
    }
}