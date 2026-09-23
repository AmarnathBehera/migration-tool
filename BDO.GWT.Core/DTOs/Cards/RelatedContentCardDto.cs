using BDO.GWT.Core.DTOs;
using System;

namespace BDO.GWT.Core.DTOs.Cards;

public class RelatedContentCardDto : ContentPageCardDto
{

public string? DateTextOverride { get; set; }

public DateTime? DateTo { get; set; }
}
