using System.ComponentModel.DataAnnotations;

namespace BDO.GWT.GLOBAL.API.Data;

public class UserInput
{
    [Required]
    public UserGeneralInput General { get; set; } = null!;
    public RoleSummaryInput[] Roles { get; set; } = Array.Empty<RoleSummaryInput>();
}

public class UserGeneralInput
{
    [Required]
    public string UserName { get; set; } = null!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public bool Enabled { get; set; }
    public bool IsExternalUser { get; set; }
    public string? PreferredLanguage { get; set; }
}

public class RoleSummaryInput
{
    public Guid GUID { get; set; }
}