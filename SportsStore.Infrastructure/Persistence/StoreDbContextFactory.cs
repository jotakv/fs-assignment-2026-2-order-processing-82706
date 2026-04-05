using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SportsStore.Infrastructure.Persistence;

public sealed class StoreDbContextFactory : IDesignTimeDbContextFactory<StoreDbContext>
{
    public StoreDbContext CreateDbContext(string[] args)
    {
        string connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__SportsStoreConnection")
            ?? "Server=(localdb)\\MSSQLLocalDB;Database=SportsStore;MultipleActiveResultSets=true";

        DbContextOptions<StoreDbContext> options = new DbContextOptionsBuilder<StoreDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new StoreDbContext(options);
    }
}
