using Microsoft.EntityFrameworkCore;

namespace SagraPOS.Models;

public class MenuDB : DbContext
{
    public MenuDB(DbContextOptions options) : base(options) { }
    // TODO rename tables with uppercase letters
    public DbSet<MenuCategory> Categories { get; set; } = null!;
    public DbSet<MenuEntry> MenuEntries { get; set; } = null!;
    public DbSet<OrderLog> OrdersLog { get; set; } = null!;
    public DbSet<OrderLogItem> OrderLogItems { get; set; } = null!;
}