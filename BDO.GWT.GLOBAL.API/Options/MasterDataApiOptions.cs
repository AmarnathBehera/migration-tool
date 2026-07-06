using System.ComponentModel.DataAnnotations;

namespace BDO.GWT.GLOBAL.API.Options;

/// <summary>
/// Represents the configuration options for the Master Data API, including the API URL and subscription key required for authentication.
/// </summary>
public class MasterDataApiOptions
{
    /// <summary>
    /// Gets or sets the base URL of the Master Data API. This URL is used to make requests to the API endpoints.
    /// </summary>
    [Required]
    public string ApiUrl { get; set; } = null!;

    /// <summary>
    /// Gets or sets the subscription key for the Master Data API. This key is required for authenticating requests to the API.
    /// </summary>
    [Required]
    public string ApiSubscriptionKey { get; set; } = null!;
}
