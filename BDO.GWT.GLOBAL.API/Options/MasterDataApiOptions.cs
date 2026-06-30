using System.ComponentModel.DataAnnotations;

namespace BDO.GWT.GLOBAL.API.Options;

/// <summary>
/// Represents the configuration options for the Master Data API, including the API URL and subscription key required for authentication.
/// </summary>
public class MasterDataApiOptions
{
    [Required]
    public string ApiUrl { get; set; } = null!;

    [Required]
    public string ApiSubscriptionKey { get; set; } = null!;
}
