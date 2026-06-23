namespace BDO.GWT.Core.DTOs.Cards;

public class KeyContactCardDto : PersonCardDto
{
    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public Guid ContentItemGuid { get; set; }
}
