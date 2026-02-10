using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;

namespace Microsoft.eShopWeb.ApplicationCore.Interfaces;

/// <summary>
/// Service responsible for communicating with the OrderItemsReserver Azure Function
/// to reserve items in the warehouse after an order is created
/// </summary>
public interface IOrderItemsReserverService
{
    /// <summary>
    /// Sends order details to the Azure Function which will generate and upload
    /// an order request JSON file to Blob Storage
    /// </summary>
    /// <param name="order">The order that was successfully created</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task ReserveOrderItemsAsync(Order order);
}
