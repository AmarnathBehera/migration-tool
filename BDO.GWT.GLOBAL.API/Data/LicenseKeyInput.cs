using System.ComponentModel.DataAnnotations;

namespace BDO.GWT.GLOBAL.API.Data;

public class LicenseKeyInput
{
    [Required]
    public required string Key { get; set; }
}
