using BDO.GWT.Common.Exceptions;
using BDO.GWT.Core.DTOs;

namespace BDO.GWT.Core.Helpers;

public static class PageHelper
{
    public static ImageDto? GetImageDto(String? imageLink, string? imageAltText)
    {
        if (imageLink == null)
        {
            return null;
        }

        var dto = new ImageDto
        {
            Url = imageLink,
            AltText = imageAltText ?? throw new InvalidStateException("Image alt text must be set when image link is present."),
        };

        return dto;
    }

    public static CallToActionDto? GetCallToActionDto(string? callToActionDescription, string? callToActionButtonText, String? callToActionButtonLink, string? callToActionPosition)
    {
        if (callToActionButtonText == null || callToActionButtonLink == null)
        {
            return null;
        }

        var dto = new CallToActionDto
        {
            Description = callToActionDescription,
            ButtonTitle = callToActionButtonText,
            ButtonUrl = callToActionButtonLink,
            Position = callToActionPosition,
        };

        return dto;
    }
}
