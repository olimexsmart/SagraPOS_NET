using Microsoft.EntityFrameworkCore;

namespace SagraPOS.Models;

public class MenuDB : DbContext
{
    public MenuDB(DbContextOptions options) : base(options) { }

    // Tables
    public DbSet<MenuCategory> Categories { get; set; } = null!;
    public DbSet<MenuEntry> MenuEntries { get; set; } = null!;
    public DbSet<OrderLog> OrdersLog { get; set; } = null!;
    public DbSet<OrderLogItem> OrderLogItems { get; set; } = null!;
    public DbSet<Setting> Settings { get; set; } = null!;
    public DbSet<SettingCategory> SettingCategories { get; set; } = null!;
    public DbSet<Printer> Printers { get; set; } = null!;
}