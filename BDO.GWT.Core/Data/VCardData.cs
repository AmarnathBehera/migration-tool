using System;
using System.Collections.Generic;

namespace BDO.GWT.Core.Data;

public class VCardData
{
    /// <summary>
    /// The formatted name string associated with the vCard object.
    /// </summary>
    public string FullName { get; set; } = null!;

    /// <summary>
    /// A list of "tags" that can be used to describe the object represented by this vCard.
    /// </summary>
    public IEnumerable<string>? Categories { get; set; }

    /// <summary>
    /// The address for electronic mail communication with the vCard object.
    /// </summary>
    public string? EMail { get; set; }

    /// <summary>
    /// Image format (PNG, JPEG or others)
    /// </summary>
    public string? LogoFormat { get; set; }

    /// <summary>
    /// Image encoded in base64 format string
    /// </summary>
    public string? LogoBase64 { get; set; }

    /// <summary>
    /// The name and optionally the unit(s) of the organization associated with the vCard object. This property is based on
    /// the X.520 Organization Name attribute and the X.520 Organization Unit attribute.
    /// </summary>
    public string? Organization { get; set; }

    /// <summary>
    /// Image format (PNG, JPEG or others)
    /// </summary>
    public string? PhotoFormat { get; set; }

    /// <summary>
    /// Image encoded in base64 format string
    /// </summary>
    public string? PhotoBase64 { get; set; }

    /// <summary>
    /// A URL that can be used to get the latest version of this vCard.
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// The canonical number string with type for a telephone number for telephony communication with the vCard object.
    /// </summary>
    public IEnumerable<(string? Type, string? Number)>? TelephoneNumbers { get; set; }

    /// <summary>
    /// Specifies the job title, functional position or function of the individual associated with the vCard object within
    /// an organization.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// A URL pointing to a website that represents the person in some way.
    /// </summary>
    public string? URL { get; set; }

    /// <summary>
    /// A timestamp for the last time the vCard was updated.
    /// </summary>
    public DateTime? RevisionDate { get; set; }

    /// <summary>
    /// A structured representation of the physical delivery address for the vCard object.
    /// </summary>
    public IEnumerable<(string locationType, VCardLocationData locationData)>? Locations { get; set; }
}
