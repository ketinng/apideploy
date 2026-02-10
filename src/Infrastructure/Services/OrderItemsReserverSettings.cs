namespace Microsoft.eShopWeb.Infrastructure.Services;

/// <summary>
/// Configuration settings for the OrderItemsReserver Azure Function
/// </summary>
public class OrderItemsReserverSettings
{
    public const string ConfigSectionName = "OrderItemsReserver";
    
    /// <summary>
    /// The endpoint URL for the Azure Function
    /// </summary>
    public string FunctionEndpoint { get; set; } = string.Empty;
    
    /// <summary>
    /// Optional function key for authentication
    /// </summary>
    public string? FunctionKey { get; set; }
    
    /// <summary>
    /// Timeout in seconds for HTTP requests
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;
}
