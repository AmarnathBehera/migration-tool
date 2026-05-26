namespace BDO.GWT.Core.DTOs.SchemaMarkups;

public class ServiceSchemaMarkupDto : BaseSchemaMarkupDto
{
    protected override string GetSchemaMarkupType() => "Service";

    /// <summary>
    /// Gets or sets the Name of the item.
    /// </summary>
    /// <value>
    /// The Name of the item.
    /// </value>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the AlternateName of the item.
    /// </summary>
    /// <value>
    /// The AlternateName of the item.
    /// </value>
    public string? AlternateName { get; set; } = "BDO";

    /// <summary>
    /// Gets or sets the URL of the item.
    /// </summary>
    /// <value>
    /// The Url of the item.
    /// </value>
    public string? Url { get; set; }

    /// <summary>
    /// Gets or sets the Logo of the item.
    /// </summary>
    /// <value>
    /// The Logo of the item.
    /// </value>
    public string? Logo { get; set; }

    /// <summary>
    /// Gets or sets the Description of the item.
    /// </summary>
    /// <value>
    /// The Description of the item.
    /// </value>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the DisambiguatingDescription of the item. https://schema.org/disambiguatingDescription
    /// </summary>
    /// <value>
    /// The DisambiguatingDescription of the item.
    /// </value>
    public string? DisambiguatingDescription { get; set; }

    /// <summary>
    /// Gets or sets the pointer to another, somehow related service (or multiple service) of the item. https://schema.org/isRelatedTo
    /// </summary>
    /// <value>
    /// The IsRelatedTo of the item.
    /// </value>
    public ServiceSchemaMarkupDto? IsRelatedTo { get; set; }


    /// <summary>
    /// Gets or sets the Provider (Organization) of the service.
    /// </summary>
    public OrganizationSchemaMarkupDto? Provider { get; set; }
}
