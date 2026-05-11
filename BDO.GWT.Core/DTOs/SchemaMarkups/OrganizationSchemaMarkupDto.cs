namespace BDO.GWT.Core.DTOs.SchemaMarkups;

public class OrganizationSchemaMarkupDto : BaseSchemaMarkupDto
{
    /// <inheritdoc />
    protected override string GetSchemaMarkupType() => "Organization";
    
    /// <summary>
    /// Gets or sets the URL of the item.
    /// </summary>
    /// <value>
    /// The URL of the item.
    /// </value>
    public string? Url { get; set; }

    /// <summary>
    /// Gets or sets the name of the item.
    /// </summary>
    /// <value>
    /// The name of the item.
    /// </value>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets a description of the item.
    /// </summary>
    /// <value>
    /// A description of the item.
    /// </value>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the image of the item.
    /// </summary>
    /// <value>
    /// The URL to the image.
    /// </value>
    public string? Image { get; set; }

    /// <summary>
    /// Gets or sets an associated logo.
    /// </summary>
    /// <value>
    /// An associated logo.
    /// </value>
    public string? Logo { get; set; }

    /// <summary>
    /// Gets or sets the official name of the organization, e.g. the registered company name.
    /// </summary>
    /// <value>
    /// The official name of the organization, e.g. the registered company name.
    /// </value>
    public string? LegalName { get; set; }
}
