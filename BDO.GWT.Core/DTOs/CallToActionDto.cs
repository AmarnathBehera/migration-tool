namespace BDO.GWT.Core.DTOs;


public class CallToActionDto
{
    public string? Description { get; set; }

    public string ButtonTitle { get; set; } = null!;

    public string ButtonUrl { get; set; } = null!;

}

//to be used for Insights page where Position field for Call to action is required
public class CallToActionWithPositionDto
{
    public CallToActionDto? CallToActionDto { get; set; }
    public string? Position { get; set; } = "left";
}