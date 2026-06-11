using Microsoft.EntityFrameworkCore;
using RentalPro.Domain.Customers;
using RentalPro.Domain.Manufacturers;
using RentalPro.Domain.Orders;
using RentalPro.Domain.Payments;
using RentalPro.Domain.Roles;
using RentalPro.Domain.Suppliers;
using RentalPro.Domain.Tools;
using RentalPro.Domain.Users;

namespace RentalPro.Infrastructure;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Role> Roles { get; set; }

    public DbSet<User> Users { get; set; }

    public DbSet<Customer> Customers { get; set; }

    public DbSet<Tool> Tools { get; set; }

    public DbSet<ToolCategory> ToolCategories { get; set; }

    public DbSet<ToolStatus> ToolStatuses { get; set; }

    public DbSet<ToolStatusHistory> ToolStatusHistories { get; set; }

    public DbSet<Manufacturer> Manufacturers { get; set; }

    public DbSet<Supplier> Suppliers { get; set; }

    public DbSet<Order> Orders { get; set; }

    public DbSet<OrderItem> OrderItems { get; set; }

    public DbSet<OrderStatus> OrderStatuses { get; set; }

    public DbSet<OrderStatusHistory> OrderStatusHistories { get; set; }

    public DbSet<Payment> Payments { get; set; }

    public DbSet<PaymentType> PaymentTypes { get; set; }

    public DbSet<PaymentMethod> PaymentMethods { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}