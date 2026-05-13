namespace BDO.GWT.Web.Core.DTOs;
public class CareerCardDto : CareerDetailsDto
{
    public string? ImageURL { get; set; }

    public string? ImageAltText { get; set; }

    public DateTime? PublishDate { get; set; }

    public string ApplyURL { get; set; } = null!;

    public string? JobTitleURL { get; set; }
}
