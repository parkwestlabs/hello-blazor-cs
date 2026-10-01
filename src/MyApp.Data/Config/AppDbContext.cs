using Microsoft.EntityFrameworkCore;
using MyApp.Core.Models;

namespace MyApp.Data.Config
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Product> Products => Set<Product>();
    }
}
