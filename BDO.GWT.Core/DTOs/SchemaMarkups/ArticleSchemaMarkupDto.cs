using System;
using System.Collections.Generic;

namespace BDO.GWT.Core.DTOs.SchemaMarkups;

public class ArticleSchemaMarkupDto : BaseSchemaMarkupDto
{
    protected override string GetSchemaMarkupType() => "Article";

    /// <summary>
    /// Gets or sets the Headline of the item.
    /// </summary>
    /// <value>
    /// The Headline of the item.
    /// </value>
    public string? Headline { get; set; }

    /// <summary>
    /// Gets or sets the Description of the item.
    /// </summary>
    /// <value>
    /// The Description of the item.
    /// </value>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets a DateModified of the item.
    /// </summary>
    /// <value>
    /// A DateModified of the item.
    /// </value>
    public DateTime? DateModified { get; set; }

    /// <summary>
    /// Gets or sets a DatePublished of the item.
    /// </summary>
    /// <value>
    /// The DatePublished to the item.
    /// </value>
    public DateTime? DatePublished { get; set; }

    /// <summary>
    /// Gets or sets the Image of the item.
    /// </summary>
    /// <value>
    /// The Image of the item.
    /// </value>
    public string? Image { get; set; }

    /// <summary>
    /// Gets or sets the MainEntityOfPage of the item.
    /// </summary>
    /// <value>
    /// The MainEntityOfPage of the item.
    /// </value>
    public string? MainEntityOfPage {get; set;}

    /// <summary>
    /// Gets or sets the URL of the item.
    /// </summary>
    /// <value>
    /// The Url of the item.
    /// </value>
    public string? Url { get; set; }

    /// <summary>
    /// Gets or sets the Author of the item.
    /// </summary>
    /// <value>
    /// The Author of the item.
    /// </value>
    public List<PersonSchemaMarkupDto>? Author { get; set; }

    /// <summary>
    /// Gets or sets the Publisher of the item.
    /// </summary>
    /// <value>
    /// The Publisher of the item.
    /// </value>
    public OrganizationSchemaMarkupDto? Publisher { get; set; }
}
