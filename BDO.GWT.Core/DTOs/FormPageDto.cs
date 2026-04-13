namespace BDO.GWT.Web.Core.DTOs.Pages;

public class FormPageDto
{
    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public HeroDto Hero { get; set; } = null!;

    public string? FormType { get; set; }

    public string? Form { get; set; }

    public string? EmbedCode { get; set; }
}
