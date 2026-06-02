// IEventBase.cs
using CMS.ContentEngine;
using CMS.Websites;
using System;
using System.Collections.Generic;
namespace BDO.GWT.Core.Entities;

public interface IEventBase
{
    // Dates
    DateTime EventDateFrom { get; set; }
    DateTime EventDateTo { get; set; }
    string EventDateOverride { get; set; }
    string EventTimeOverride { get; set; }
    bool EventAllDay { get; set; }

    // Content
    string EventPostDescription { get; set; }
    string CustomBodyText { get; set; }
    string CpeDescription { get; set; }
    bool CpeEvent { get; set; }
    string CpeCount { get; set; }
    string CpeCreditNotes { get; set; }
    string ProgramLevel { get; set; }

    // Registration
    bool AllowRegistration { get; set; }
    string RegistrationType { get; set; }
    string RegistrationFormLink { get; set; }
    bool DisableAddToCalendar { get; set; }
    bool DisablePrintAndShare { get; set; }
    string EventVideo { get; set; }

    // Location
    string EventLocationTitle { get; set; }
    string EventLocationAddress1 { get; set; }
    string EventLocationAddress2 { get; set; }
    string EventLocationCity { get; set; }
    string EventLocationState { get; set; }
    string EventLocationZip { get; set; }
    string EventLocationZipPosition { get; set; }
    bool HideEventLocationZipComma { get; set; }
    string EventLocationCountry { get; set; }
    bool ShowEventLocation { get; set; }
    string EventLocationLongitude { get; set; }
    string EventLocationLatitude { get; set; }

    // Contact
    string EventContactName { get; set; }
    string EventContactPhone { get; set; }
    string EventContactEmail { get; set; }

    // Metadata
    string MetadataTitle { get; set; }
    string MetadataDescription { get; set; }
    IEnumerable<ContentAttachment> MetadataTeaserImage { get; set; }
    string MetadataTeaserImageAltText { get; set; }

    // Privacy
    bool EventIsPrivate { get; set; }
    bool ShowPastEvent { get; set; }

    // System — needed for URL / organizer lookups
    WebPageFields SystemFields { get; set; }

    // Computed helpers (implemented in partials)
    bool ShowInWidget { get; }
    DateTime? RelatedContentDate { get; }
    DateTime? RelatedContentDateTo { get; }
    string? RelatedContentDateOverride { get; }
    IEnumerable<ContentAttachment> Image { get; set; }
    string? ImageAltText { get; set; }
}