namespace BDO.GWT.Common.Core.Helpers;

public static class GoogleMapsHelper
{
    public static string? GenerateGoogleMapsLink(string? latitude, string? longitude)
    {
        if (string.IsNullOrEmpty(latitude) || string.IsNullOrEmpty(longitude))
        {
            return null;
        }

        return $"https://www.google.com/maps/search/?api=1&query={latitude},{longitude}";
    }

    public static string? GenerateGoogleMapsLink(string? eventLocationTitle, string? eventLocationAddress1, string? eventLocationAddress2, string? eventLocationCity, string? eventLocationState, string? eventLocationZip)
    {
        if (string.IsNullOrEmpty(string.Concat(eventLocationTitle, eventLocationAddress1, eventLocationAddress2, eventLocationCity, eventLocationState, eventLocationZip)))
        {
            return null;
        }

        var locationComma = ((string.IsNullOrEmpty(eventLocationCity)) || (string.IsNullOrEmpty(eventLocationState))) ? string.Empty : ", ";

        return $"https://www.google.com/maps/search/{eventLocationTitle}, {eventLocationAddress1} {eventLocationAddress2} {eventLocationCity}{locationComma}{eventLocationState} {eventLocationZip}";
    }
}
