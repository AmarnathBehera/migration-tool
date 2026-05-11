namespace BDO.GWT.Core.DTOs.SchemaMarkups;

public class PostalAddressSchemaMarkupDto : BaseSchemaMarkupDto
{
    protected override string GetSchemaMarkupType() => "PostalAddress";

    /// <summary>
    /// Gets or sets the StreetAddress of the item.
    /// </summary>
    /// <value>
    /// The StreetAddress of the item.
    /// </value>
    public string? StreetAddress { get; set; }

    /// <summary>
    /// Gets or sets the ExtendedAddress of the item (An address extension such as an apartment number, C/O or alternative name).
    /// </summary>
    /// <value>
    /// The ExtendedAddress of the item.
    /// </value>
    public string? ExtendedAddress { get; set; }

    /// <summary>
    /// Gets or sets the AddressLocality of the item.
    /// </summary>
    /// <value>
    /// The AddressLocality of the item.
    /// </value>
    public string? AddressLocality { get; set; }

    /// <summary>
    /// Gets or sets the PostalCode of the item.
    /// </summary>
    /// <value>
    /// The PostalCode of the item.
    /// </value>
    public string? PostalCode { get; set; }

    /// <summary>
    /// Gets or sets the AddressRegion of the item.
    /// </summary>
    /// <value>
    /// The AddressRegion of the item.
    /// </value>
    public string? AddressRegion { get; set; }

    /// <summary>
    /// Gets or sets the AddressCountry of the item.
    /// </summary>
    /// <value>
    /// The AddressCountry of the item.
    /// </value>
    public string? AddressCountry { get; set; }
}
