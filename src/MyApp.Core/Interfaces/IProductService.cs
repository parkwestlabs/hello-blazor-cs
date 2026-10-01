using MyApp.Core.Models;

namespace MyApp.Core.Interfaces;

public interface IProductService
{
    Task<List<Product>> GetAllProductsAsync(CancellationToken cancellationToken = default);
    Task<Product?> GetProductByIdAsync(int id, CancellationToken cancellationToken = default);
    Task SaveProductAsync(Product product, CancellationToken cancellationToken = default);
    Task DeleteProductAsync(int id, CancellationToken cancellationToken = default);
}
