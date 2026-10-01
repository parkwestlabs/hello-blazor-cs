using MyApp.Core.Interfaces;
using MyApp.Core.Models;

namespace MyApp.Core.Services;

public class ProductService(IProductRepository repository) : IProductService
{
    public Task<List<Product>> GetAllProductsAsync(CancellationToken cancellationToken = default)
    {
        return repository.GetAllAsync(cancellationToken);
    }

    public Task<Product?> GetProductByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return repository.GetByIdAsync(id, cancellationToken);
    }

    public async Task SaveProductAsync(Product product, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(product);

        if (product.Id == 0)
        {
            await repository.AddAsync(product, cancellationToken);
        }
        else
        {
            await repository.UpdateAsync(product, cancellationToken);
        }
    }

    public Task DeleteProductAsync(int id, CancellationToken cancellationToken = default)
    {
        return repository.DeleteAsync(id, cancellationToken);
    }
}
