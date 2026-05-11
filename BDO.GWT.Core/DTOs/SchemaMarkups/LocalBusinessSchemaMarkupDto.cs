namespace BDO.GWT.Core.DTOs.SchemaMarkups;

public class LocalBusinessSchemaMarkupDto : OrganizationSchemaMarkupDto
{
    /// <inheritdoc />
    protected override string GetSchemaMarkupType() => "LocalBusiness";

    /// <summary>
    /// Gets or sets the Longitude.
    /// </summary>
    public string? Longitude { get; set; }

    /// <summary>
    /// Gets or sets the Latitude.
    /// </summary>
    public string? Latitude { get; set; }

    /// <summary>
    /// Gets or sets the Address.
    /// </summary>
    public PostalAddressSchemaMarkupDto? Address { get; set; }

    /// <summary>
    /// Gets or sets the Brand of the item.
    /// </summary>
    public OrganizationSchemaMarkupDto? Brand { get; set; }
}
