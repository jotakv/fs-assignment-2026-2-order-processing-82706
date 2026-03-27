using Microsoft.EntityFrameworkCore;
using SportsStore.Domain.Entities;

namespace SportsStore.Infrastructure.Persistence;

public class StoreDbContext : DbContext
{
    public StoreDbContext(DbContextOptions<StoreDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<CartLine> CartLines => Set<CartLine>();
}
