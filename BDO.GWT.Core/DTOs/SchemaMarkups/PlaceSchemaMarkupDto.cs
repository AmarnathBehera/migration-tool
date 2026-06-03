using BDO.GWT.Core.DTOs.SchemaMarkups;

namespace BDO.GWT.Web.Core.DTOs.SchemaMarkups;

public class PlaceSchemaMarkupDto : BaseSchemaMarkupDto
{
    protected override string GetSchemaMarkupType() => "Place";

    /// <summary>
    /// Gets or sets the Name of the item.
    /// </summary>
    /// <value>
    /// The Name of the item.
    /// </value>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the Address of the item.
    /// </summary>
    /// <value>
    /// The Address of the item.
    /// </value>
    public PostalAddressSchemaMarkupDto? Address { get; set; }
}
