namespace BDO.GWT.Core.DTOs.Cards;
public class CareerCardDto 
{
    public string? ImageURL { get; set; }

    public string? ImageAltText { get; set; }

    public DateTime? PublishDate { get; set; }

    public string ApplyURL { get; set; } = null!;

    public string? JobTitleURL { get; set; }

    public string Title { get; set; } = null!;

    public string? JobTitle { get; set; }

    public string? Reference { get; set; }

    public string? EmploymentType { get; set; }

    public string? Level { get; set; }

    public string[] Locations { get; set; } = Array.Empty<string>();
}
