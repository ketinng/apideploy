using System.Net;
using System.Text;
using System.Text.Json;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OrderItemsReserver.Models;

namespace OrderItemsReserver;

/// <summary>
/// Azure Function that receives order details and uploads them as JSON to Blob Storage
/// for warehouse item reservation processing
/// </summary>
public class OrderItemsReserverFunction
{
    private readonly ILogger<OrderItemsReserverFunction> _logger;
    private readonly string _storageConnectionString;
    private readonly string _containerName;

    public OrderItemsReserverFunction(ILogger<OrderItemsReserverFunction> logger)
    {
        _logger = logger;
        _storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage") 
            ?? throw new InvalidOperationException("AzureWebJobsStorage connection string is not configured");
        _containerName = Environment.GetEnvironmentVariable("OrderRequestsContainerName") ?? "order-requests";
    }

    [Function("ReserveOrderItems")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "orders/reserve")] HttpRequestData req)
    {
        _logger.LogInformation("Processing order reservation request");

        try
        {
            // Parse the incoming order request with case-insensitive JSON
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var orderRequest = await JsonSerializer.DeserializeAsync<OrderRequestDto>(req.Body, jsonOptions);

            if (orderRequest == null)
            {
                _logger.LogWarning("Invalid request: Request body is null or empty");
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid request body");
            }

            if (orderRequest.Items == null || orderRequest.Items.Count == 0)
            {
                _logger.LogWarning("Invalid request: Order {OrderId} has no items", orderRequest.OrderId);
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Order must contain at least one item");
            }

            // Create simplified order request format for warehouse
            var warehouseOrderRequest = orderRequest.Items.Select(item => new
            {
                itemId = item.ItemId,
                quantity = item.Quantity,
                productName = item.ProductName
            }).ToList();

            // Generate unique filename
            var timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");
            var fileName = $"order_{orderRequest.OrderId}_{timestamp}.json";

            // Serialize to JSON
            var jsonContent = JsonSerializer.Serialize(warehouseOrderRequest, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });

            // Upload to Blob Storage
            await UploadToBlobStorageAsync(fileName, jsonContent);

            _logger.LogInformation(
                "Successfully uploaded order request for Order {OrderId} to blob storage. File: {FileName}",
                orderRequest.OrderId, 
                fileName);

            // Create success response
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new
            {
                success = true,
                message = "Order request uploaded successfully",
                fileName = fileName,
                orderId = orderRequest.OrderId,
                itemCount = orderRequest.Items.Count
            });

            return response;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize request body");
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid JSON format");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing order reservation request");
            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, "An error occurred while processing the request");
        }
    }

    private async Task UploadToBlobStorageAsync(string fileName, string content)
    {
        try
        {
            // Create BlobServiceClient
            var blobServiceClient = new BlobServiceClient(_storageConnectionString);

            // Get or create container
            var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync();

            // Get blob client
            var blobClient = containerClient.GetBlobClient(fileName);

            // Upload content
            var bytes = Encoding.UTF8.GetBytes(content);
            using var stream = new MemoryStream(bytes);
            await blobClient.UploadAsync(stream, overwrite: true);

            _logger.LogInformation("Successfully uploaded blob: {FileName} to container: {ContainerName}", 
                fileName, _containerName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload blob {FileName} to storage", fileName);
            throw;
        }
    }

    private static async Task<HttpResponseData> CreateErrorResponse(
        HttpRequestData req, 
        HttpStatusCode statusCode, 
        string message)
    {
        var response = req.CreateResponse(statusCode);
        await response.WriteAsJsonAsync(new
        {
            success = false,
            message = message
        });
        return response;
    }
}
