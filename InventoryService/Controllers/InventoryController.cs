using System.Net;
using Domain;
using Microsoft.AspNetCore.Mvc;

namespace InventoryService.Controllers;

[Produces("application/json")]
[Route("api/[controller]")]
public class InventoryController : Controller
{
    private static int _requestCount;

    private readonly Product[] _products =
    {
        new()
        {
            Id = "1",
            Name = "Apples",
            Quantity = 12
        },
        new()
        {
            Id = "2",
            Name = "Oranges",
            Quantity = 25
        },
        new()
        {
            Id = "3",
            Name = "Oranges",
            Quantity = 25
        }
    };

    [HttpGet("{productId}")]
    public async Task<IActionResult> Get(string productId)
    {
        await Task.Delay(1000); // simulate some data processing by delaying for 100 milliseconds 
        _requestCount++;

        var product = _products.FirstOrDefault(p => p.Id == productId);

        return _requestCount % 4 == 0
            ? // only one of out four requests will succeed
            Ok(product)
            : StatusCode((int)HttpStatusCode.InternalServerError, "Something went wrong");
    }

    [HttpGet("name/{productName}")]
    public async Task<IActionResult> GetByName(string productName)
    {
        await Task.Delay(1000); // simulate some data processing by delaying for 100 milliseconds 

        // var product = _products.FirstOrDefault(p => p.Name == productName);
        // return Ok(product);

        return StatusCode((int)HttpStatusCode.InternalServerError,
            $"Something went wrong when getting prodict by name when getting {productName}");
    }
}