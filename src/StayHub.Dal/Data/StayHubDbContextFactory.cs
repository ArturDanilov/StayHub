using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace StayHub.Dal.Data;

public sealed class StayHubDbContextFactory : IDesignTimeDbContextFactory<StayHubDbContext>
{
    public StayHubDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<StayHubDbContext>()
            .UseSqlServer(
                "Server=localhost,1433;Database=StayHubDb;User Id=sa;Password=DesignTimeOnly123!;TrustServerCertificate=True;")
            .Options;

        return new StayHubDbContext(options);
    }
}
