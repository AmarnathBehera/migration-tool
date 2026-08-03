namespace BDO.GWT.GLOBAL.API.Data;
public class UserOutput
{
    public UserGeneralOutput General { get; set; } = null!;
    public ChannelSummaryOutput[] Channels { get; set; } = Array.Empty<ChannelSummaryOutput>();
    public RoleSummaryOutput[] Roles { get; set; } = Array.Empty<RoleSummaryOutput>();
}

public class UserGeneralOutput
{
    public Guid GUID { get; set; }
    public string UserName { get; set; } = null!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public bool Enabled { get; set; }
    public bool IsExternalUser { get; set; }
    public DateTime? Created { get; set; }
    public DateTime? LastSignIn { get; set; }
    public int InvalidSignInAttempts { get; set; }
    public string? PreferredLanguage { get; set; }
}

public class ChannelSummaryOutput
{
    public Guid GUID { get; set; }
    public string DisplayName { get; set; } = null!;
}

public class RoleSummaryOutput
{
    public Guid GUID { get; set; }
    public string DisplayName { get; set; } = null!;
}