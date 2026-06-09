using BDO.GWT.Core.DTOs.SchemaMarkups;
using System;

namespace BDO.GWT.Web.Core.DTOs.SchemaMarkups;

public class EventSchemaMarkupDto : BaseSchemaMarkupDto
{
    protected override string GetSchemaMarkupType() => "Event";

    /// <summary>
    /// Gets or sets the Name of the item.
    /// </summary>
    /// <value>
    /// The Name of the item.
    /// </value>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the Description of the item.
    /// </summary>
    /// <value>
    /// The Description of the item.
    /// </value>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the Image of the item.
    /// </summary>
    /// <value>
    /// The Image of the item.
    /// </value>
    public string? Image { get; set; }

    /// <summary>
    /// Gets or sets a StartDate of the item.
    /// </summary>
    /// <value>
    /// A StartDate of the item.
    /// </value>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Gets or sets a EndDate of the item.
    /// </summary>
    /// <value>
    /// The EndDate to the item.
    /// </value>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Gets or sets a Location of the item.
    /// </summary>
    /// <value>
    /// The Location to the item.
    /// </value>
    public PlaceSchemaMarkupDto? Location { get; set; }

    /// <summary>
    /// Gets or sets a Offers of the item.
    /// </summary>
    /// <value>
    /// The Offers to the item.
    /// </value>
    public OfferSchemaMarkupDto? Offers { get; set; }

    /// <summary>
    /// Gets or sets the Organizer of the item.
    /// </summary>
    /// <value>
    /// The Organizer of the item.
    /// </value>
    public OrganizationSchemaMarkupDto? Organizer { get; set; }
}
