using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Services.Dtos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.eShopWeb.Infrastructure.Services;

/// <summary>
/// Implementation of the OrderItemsReserver service that communicates with the Azure Function
/// </summary>
public class OrderItemsReserverService : IOrderItemsReserverService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OrderItemsReserverService> _logger;
    private readonly OrderItemsReserverSettings _settings;

    public OrderItemsReserverService(
        HttpClient httpClient, 
        ILogger<OrderItemsReserverService> logger,
        IOptions<OrderItemsReserverSettings> settings)
    {
        _httpClient = httpClient;
        _logger = logger;
        _settings = settings.Value;
    }

    public async Task ReserveOrderItemsAsync(Order order)
    {
        try
        {
            // Check if endpoint is configured
            if (string.IsNullOrWhiteSpace(_settings.FunctionEndpoint))
            {
                _logger.LogWarning(
                    "OrderItemsReserver Azure Function endpoint is not configured. Skipping reservation for Order ID: {OrderId}",
                    order.Id);
                return;
            }

            // Build the request payload with order details
            var reservationRequest = new OrderReservationRequest
            {
                OrderId = order.Id,
                Items = order.OrderItems.Select(item => new OrderItemReservation
                {
                    ItemId = item.ItemOrdered.CatalogItemId,
                    Quantity = item.Units
                }).ToList()
            };

            // Serialize to JSON
            var jsonContent = JsonSerializer.Serialize(reservationRequest, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            _logger.LogInformation(
                "Sending order reservation request for Order ID: {OrderId} with {ItemCount} items to {Endpoint}",
                order.Id,
                order.OrderItems.Count,
                _settings.FunctionEndpoint);

            // Build request URL with optional function key
            var requestUrl = _settings.FunctionEndpoint;
            if (!string.IsNullOrWhiteSpace(_settings.FunctionKey))
            {
                var separator = requestUrl.Contains('?') ? "&" : "?";
                requestUrl = $"{requestUrl}{separator}code={_settings.FunctionKey}";
            }

            // TODO: Uncomment when Azure Function is deployed and configured
            // Make HTTP POST request to Azure Function
            // var response = await _httpClient.PostAsync(requestUrl, content);
            
            // if (!response.IsSuccessStatusCode)
            // {
            //     var errorContent = await response.Content.ReadAsStringAsync();
            //     _logger.LogError(
            //         "Failed to reserve order items. Order ID: {OrderId}, Status Code: {StatusCode}, Error: {Error}",
            //         order.Id,
            //         response.StatusCode,
            //         errorContent);
            //     throw new HttpRequestException($"Order items reservation failed with status code: {response.StatusCode}");
            // }

            _logger.LogInformation(
                "Successfully sent order reservation request for Order ID: {OrderId}",
                order.Id);

            // Placeholder: Simulating successful call
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error occurred while reserving order items for Order ID: {OrderId}",
                order.Id);
            
            // Depending on business requirements, you might want to:
            // - Rethrow the exception to fail the order creation
            // - Log and continue (order created but reservation failed)
            // - Implement retry logic or queue for later processing
            throw;
        }
    }
}
