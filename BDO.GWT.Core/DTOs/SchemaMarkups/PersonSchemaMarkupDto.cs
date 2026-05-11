using System.Collections.Generic;

namespace BDO.GWT.Core.DTOs.SchemaMarkups;

/// <summary>
/// The DTO for Person schema markup data.
/// </summary>
/// <seealso cref="BDO.GWT.Web.Core.DTOs.SchemaMarkups.BaseSchemaMarkupDto" />
public class PersonSchemaMarkupDto : BaseSchemaMarkupDto
{
    /// <inheritdoc />
    protected override string GetSchemaMarkupType() => "Person";

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
    /// Gets or sets the job title of the person (for example, Financial Manager).
    /// </summary>
    /// <value>
    /// The job title of the person (for example, Financial Manager).
    /// </value>
    public string? JobTitle { get; set; }

    /// <summary>
    /// Gets or sets the description of the item.
    /// </summary>
    /// <value>
    /// The description of the item.
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
    /// Gets or sets the URLs of a reference Web page that unambiguously indicates the item's identity.
    /// E.g. the URL of the item's Wikipedia page, Wikidata entry, or official website.
    /// </summary>
    /// <value>
    /// The URLs of a reference Web page that unambiguously indicates the item's identity.
    /// E.g. the URL of the item's Wikipedia page, Wikidata entry, LinkedIn profile or official website.
    /// </value>
    public IEnumerable<string>? SameAs { get; set; }

    /// <summary>
    /// Gets or sets the Organization the person works for.
    /// </summary>
    public OrganizationSchemaMarkupDto? WorksFor { get; set; }
}
