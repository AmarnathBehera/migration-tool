using System;

namespace BDO.GWT.Web.Core.DTOs;

public class EventWebinarDetailsRegistrationCardDto
{
    public string? Type { get; set; }

    public string? LocationTitle { get; set; }

    public DateTime DateFrom { get; set; }

    public DateTime DateTo { get; set; }

    public string? Country { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? PostalCode { get; set; }

    public string? AddressLine1 { get; set; }

    public string? AddressLine2 { get; set; }

    public string? GetDirectionsUrl { get; set; }

    public string? RegistrationButtonUrl { get; set; }

    public bool? DisableRegistrationButton { get; set; }

    public string? RecordingUrl { get; set; }

    public bool? DisableAddToCalendar { get; set; }

    public string? AddToCalendarUrl { get; set; }

    public string? ContactName { get; set; }

    public string? ContactEmail { get; set; }

    public string? ContactPhone { get; set; }

    public string? RegistrationType { get; set; }

    public string? RegistrationFormUrl { get; set; }

    public string? DateTextOverride { get; set; }

    public string? TimeTextOverride { get; set; }

    public bool? ZipPositionAfter { get; set; }

    public bool HideEventLocationZipComma { get; set; }
}
