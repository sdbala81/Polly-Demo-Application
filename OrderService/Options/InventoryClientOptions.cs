using System.ComponentModel.DataAnnotations;

namespace OrderService.Options;

public class InventoryClientOptions
{
    [Required]
    public Uri BaseAddress { get; set; }

    [Required]
    public TimeSpan Timeout { get; set; }
}
