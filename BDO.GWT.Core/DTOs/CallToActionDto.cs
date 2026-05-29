namespace BDO.GWT.Core.DTOs;


public class CallToActionDto
{
    public string? Description { get; set; }

    public string ButtonTitle { get; set; } = null!;

    public string ButtonUrl { get; set; } = null!;

}

public class CallToActionWithPositionDto
{
    public CallToActionDto? CallToActionDto { get; set; }
    public string? Position { get; set; } = "left";
}