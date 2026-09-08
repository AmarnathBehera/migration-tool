using System.ComponentModel.DataAnnotations;

namespace BDO.GWT.GLOBAL.API.Data;

public class ChannelInput
{
    [Required]
    public ChannelGeneralInput General { get; set; } = null!;

    [Required]
    public ChannelDomainAliasesInput DomainAliases { get; set; } = null!;

    public ChannelCultureInput[] Cultures { get; set; } = [];
}

public class ChannelGeneralInput
{
    [Required]
    public string DisplayName { get; set; } = null!;

    [Required]
    public string CodeName { get; set; } = null!;

    [Required]
    public string PresentationURL { get; set; } = null!;

    public string? Description { get; set; }

    public string? DefaultContentCulture { get; set; }

    public string? DefaultVisitorCulture { get; set; }
}

public class ChannelDomainAliasesInput
{
    public ChannelDomainAliasInput[] LiveSite { get; set; } = [];
}

public class ChannelDomainAliasInput
{
    [Required]
    public string PresentationURL { get; set; } = null!;
}

public class ChannelCultureInput
{
    [Required]
    public string Code { get; set; } = null!;
}
