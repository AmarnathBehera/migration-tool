using BDO.GWT.Core.DTOs.SchemaMarkups;

namespace BDO.GWT.Web.Core.DTOs.SchemaMarkups;

public class OfferSchemaMarkupDto : BaseSchemaMarkupDto
{
    protected override string GetSchemaMarkupType() => "Offer";

    /// <summary>
    /// Gets or sets the URL of the item.
    /// </summary>
    /// <value>
    /// The Url of the item.
    /// </value>
    public string? Url { get; set; }
}
