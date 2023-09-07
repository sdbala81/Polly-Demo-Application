
using Domain;

namespace OrderService.Clients;

public interface IInventoryClient
{
    Task<Product?> GetProduct(string productId);
}
