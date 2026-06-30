using System.ComponentModel.DataAnnotations;

namespace BDO.GWT.GLOBAL.API.Options;

public class MasterDataApiOptions
{
    [Required]
    public string ApiUrl { get; set; } = null!;

    [Required]
    public string ApiSubscriptionKey { get; set; } = null!;
}
