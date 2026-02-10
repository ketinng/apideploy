namespace OrderItemsReserver.Models;

/// <summary>
/// Represents a single item in an order with simplified details for warehouse reservation
/// </summary>
public class OrderItemDto
{
    /// <summary>
    /// The catalog item identifier
    /// </summary>
    public int ItemId { get; set; }

    /// <summary>
    /// The quantity of items ordered
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Optional: Product name for reference
    /// </summary>
    public string? ProductName { get; set; }
}
