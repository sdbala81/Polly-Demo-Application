using Microsoft.AspNetCore.Mvc;
using OrderService.Clients;

namespace OrderService.Controllers;

[Produces("application/json")]
[Route("api/[controller]s")]
public class OrderController : Controller
{
    private readonly IInventoryClient _inventoryClient;

    public OrderController(IInventoryClient inventoryClient)
    {
        _inventoryClient = inventoryClient;
    }

    [HttpGet("{productId}")]
    public async Task<IActionResult> Get(string productId)
    {
        var product = await _inventoryClient.GetProduct(productId);

        return Ok(product);
    }
}
