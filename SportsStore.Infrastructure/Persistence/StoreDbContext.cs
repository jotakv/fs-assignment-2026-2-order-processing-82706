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

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    public DbSet<InventoryRecord> InventoryRecords => Set<InventoryRecord>();

    public DbSet<PaymentRecord> PaymentRecords => Set<PaymentRecord>();

    public DbSet<ShipmentRecord> ShipmentRecords => Set<ShipmentRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(product => product.Price).HasColumnType("decimal(8, 2)");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.Property(order => order.Status)
                .HasConversion<string>()
                .HasMaxLength(50);

            entity.HasOne(order => order.Customer)
                .WithMany(customer => customer.Orders)
                .HasForeignKey(order => order.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(order => order.Items)
                .WithOne(item => item.Order)
                .HasForeignKey(item => item.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(order => order.InventoryRecords)
                .WithOne(record => record.Order)
                .HasForeignKey(record => record.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(order => order.PaymentRecords)
                .WithOne(record => record.Order)
                .HasForeignKey(record => record.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(order => order.ShipmentRecords)
                .WithOne(record => record.Order)
                .HasForeignKey(record => record.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CartLine>(entity =>
        {
            entity.HasOne<Order>()
                .WithMany(order => order.Lines)
                .HasForeignKey("OrderID")
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(line => line.Product)
                .WithMany()
                .HasForeignKey("ProductID")
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.Property(customer => customer.Name).HasMaxLength(200);
            entity.Property(customer => customer.Email).HasMaxLength(320);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.Property(item => item.UnitPrice).HasColumnType("decimal(18,2)");
            entity.Property(item => item.LineTotal).HasColumnType("decimal(18,2)");
            entity.HasOne(item => item.Product)
                .WithMany()
                .HasForeignKey(item => item.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<InventoryRecord>(entity =>
        {
            entity.Property(record => record.ReservationReference).HasMaxLength(100);
            entity.Property(record => record.FailureReason).HasMaxLength(500);
        });

        modelBuilder.Entity<PaymentRecord>(entity =>
        {
            entity.Property(record => record.Provider).HasMaxLength(200);
            entity.Property(record => record.ExternalPaymentId).HasMaxLength(200);
            entity.Property(record => record.Status).HasMaxLength(100);
            entity.Property(record => record.FailureReason).HasMaxLength(500);
        });

        modelBuilder.Entity<ShipmentRecord>(entity =>
        {
            entity.Property(record => record.ShipmentReference).HasMaxLength(100);
            entity.Property(record => record.Carrier).HasMaxLength(100);
            entity.Property(record => record.TrackingNumber).HasMaxLength(100);
            entity.Property(record => record.FailureReason).HasMaxLength(500);
        });
    }
}
