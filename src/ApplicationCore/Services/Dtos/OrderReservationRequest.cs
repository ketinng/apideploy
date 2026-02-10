using System.Collections.Generic;

namespace Microsoft.eShopWeb.ApplicationCore.Services.Dtos;

/// <summary>
/// Represents an order reservation request to be sent to the OrderItemsReserver Azure Function
/// </summary>
public class OrderReservationRequest
{
    public int OrderId { get; set; }
    public List<OrderItemReservation> Items { get; set; } = new();
}

/// <summary>
/// Represents a single item in the order reservation request
/// </summary>
public class OrderItemReservation
{
    public int ItemId { get; set; }
    public int Quantity { get; set; }
}
