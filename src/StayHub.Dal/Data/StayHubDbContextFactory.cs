using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace StayHub.Dal.Data;

public sealed class StayHubDbContextFactory : IDesignTimeDbContextFactory<StayHubDbContext>
{
    public StayHubDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<StayHubDbContext>()
            .UseSqlServer(
                "Server=localhost,1433;Database=StayHubDb;Integrated Security=True;TrustServerCertificate=True;")
            .Options;

        return new StayHubDbContext(options);
    }
}
