namespace OrderItemsReserver.Models;

/// <summary>
/// Represents an order request from the eShopOnWeb application
/// </summary>
public class OrderRequestDto
{
    /// <summary>
    /// The unique identifier of the order
    /// </summary>
    public int OrderId { get; set; }

    /// <summary>
    /// The buyer/customer identifier
    /// </summary>
    public string BuyerId { get; set; } = string.Empty;

    /// <summary>
    /// The date and time when the order was created
    /// </summary>
    public DateTimeOffset OrderDate { get; set; }

    /// <summary>
    /// List of items in the order
    /// </summary>
    public List<OrderItemDto> Items { get; set; } = new();
}
