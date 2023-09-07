using Domain;
using OrderService.Options;

namespace OrderService.Clients;

public class InventoryClient : IInventoryClient
{
    private readonly HttpClient _httpClient;

    public InventoryClient(HttpClient httpClient, InventoryClientOptions inventoryClientOptions)
    {
        _httpClient = httpClient;
    }

    public async Task<Product?> GetProduct(string productId)
    {

        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, $"api/inventory/{productId}");

        using var response = await _httpClient.SendAsync(
            httpRequestMessage,
            HttpCompletionOption.ResponseHeadersRead,
            CancellationToken.None);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<Product>();
    }
}