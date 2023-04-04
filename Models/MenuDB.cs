using Microsoft.EntityFrameworkCore;

namespace SagraPOS.Models;

public class MenuDB : DbContext
{
    public MenuDB(DbContextOptions options) : base(options) { }
    // TODO rename tables with uppercase letters
    public DbSet<MenuCategory> categories { get; set; } = null!;
    public DbSet<MenuEntry> menuEntries { get; set; } = null!;

}