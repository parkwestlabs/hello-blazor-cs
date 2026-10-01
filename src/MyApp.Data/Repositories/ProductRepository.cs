using Microsoft.EntityFrameworkCore;
using MyApp.Core.Interfaces;
using MyApp.Core.Models;
using MyApp.Data.Config;

namespace MyApp.Data.Repositories;

public class ProductRepository(IDbContextFactory<AppDbContext> contextFactory) : IProductRepository
{
    public async Task<List<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Products.AsNoTracking().OrderBy(p => p.Id).ToListAsync(cancellationToken);
    }

    public async Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        context.Products.Add(product);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        context.Products.Update(product);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var product = await context.Products.FindAsync([id], cancellationToken);
        if (product != null)
        {
            context.Products.Remove(product);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
