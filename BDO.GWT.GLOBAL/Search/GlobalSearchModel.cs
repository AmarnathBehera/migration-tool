using System;
using Azure.Search.Documents.Indexes;
using Kentico.Xperience.AzureSearch.Indexing;
using Azure.Search.Documents.Indexes.Models;

namespace BDO.GWT.GLOBAL.Search;

/// <summary>
/// The Azure AI Search index model produced by <see cref="GlobalAzureSearchIndexingStrategy"/>.
/// Each public property becomes a field in the index. The field NAME is the property name
/// (FieldBuilder ignores System.Text.Json attributes), so keep names stable.
///
/// Attributes:
///  - [SearchableField] : full-text analyzed (string only). Add IsFilterable/IsSortable/IsFacetable as needed.
///  - [SimpleField]     : stored/retrievable, not full-text. Use for ids, dates, bools, urls, alt text.
///
/// Base fields (already present via BaseAzureSearchModel): Url, ContentTypeName, LanguageName,
/// ItemGuid, ObjectID (key), Name. K13 equivalents (classname, sys_culture, nodealiaspath, sys_id,
/// documentname, documentculture) are intentionally NOT re-declared here.
/// </summary>
public class GlobalSearchModel : BaseAzureSearchModel
{
    // ---------- IDs ----------

    [SimpleField(IsFilterable = true)]
    public int? AboutPageID { get; set; }

    [SimpleField(IsFilterable = true)]
    public int? BlogID { get; set; }

    [SimpleField(IsFilterable = true)]
    public int? BlogPostID { get; set; }

    [SimpleField(IsFilterable = true)]
    public int? CareersID { get; set; }

    [SimpleField(IsFilterable = true)]
    public int? DealID { get; set; }

    [SimpleField(IsFilterable = true)]
    public int? EventID { get; set; }

    [SimpleField(IsFilterable = true)]
    public int? InsightID { get; set; }

    [SimpleField(IsFilterable = true)]
    public int? NewsArticleID { get; set; }

    [SimpleField(IsFilterable = true)]
    public int? WebinarID { get; set; }

    // ---------- Classification ----------
      
    [SimpleField(IsFilterable = true, IsFacetable = true)]
    public string ContentType { get; set; } = string.Empty;

    [SimpleField(IsFilterable = true, IsFacetable = true)]
    public string EventType { get; set; } = string.Empty;

     // ---------- Content ----------

    [SearchableField]
    public string Content { get; set; } = string.Empty;

    [SearchableField]
    public string ContentBody { get; set; } = string.Empty;

    [SearchableField]
    public string DescriptionBody { get; set; } = string.Empty;

    [SearchableField]
    public string DocumentContent { get; set; } = string.Empty;

    // ---------- Metadata ----------

    [SearchableField(IsSortable = true)]
    public string DocumentPageTitle { get; set; } = string.Empty;

    [SearchableField]
    public string DocumentPageDescription { get; set; } = string.Empty;

    [SearchableField]
    public string DocumentPageKeywords { get; set; } = string.Empty;

    [SearchableField]
    public string MetadataTitle { get; set; } = string.Empty;

    [SearchableField]
    public string MetadataDescription { get; set; } = string.Empty;

    [SimpleField]
    public string MetadataTeaserImage { get; set; } = string.Empty;

    [SimpleField]
    public string MetadataTeaserImageAltText { get; set; } = string.Empty;

    [SimpleField(IsFilterable = true)]
    public string DocumentTags { get; set; } = string.Empty;

    // ---------- Hero / Images ----------

    [SearchableField]
    public string HeroTitle { get; set; } = string.Empty;

    [SearchableField]
    public string HeroDescription { get; set; } = string.Empty;

    [SimpleField]
    public string HeroBackgroundImage { get; set; } = string.Empty;

    [SimpleField]
    public string Image { get; set; } = string.Empty;

    [SimpleField]
    public string ImageAltText { get; set; } = string.Empty;

    [SearchableField]
    public string SectionTitle { get; set; } = string.Empty;

    // ---------- Document Information ----------

    [SimpleField(IsFilterable = true)]
    public int? DocumentID { get; set; }

    [SimpleField(IsFilterable = true)]
    public int? DocumentNodeID { get; set; }

    [SimpleField(IsFilterable = true)]
    public int? DocumentForeignKeyValue { get; set; }

    [SimpleField(IsFilterable = true)]
    public int? DocumentTagGroupID { get; set; }

    [SimpleField(IsFilterable = true)]
    public int? DocumentCreatedByUserID { get; set; }

    [SimpleField(IsFilterable = true)]
    public int? DocumentModifiedByUserID { get; set; }

    [SimpleField(IsFilterable = true)]
    public bool? SearchExcluded { get; set; }

    

    // ---------- Dates ----------

    [SimpleField(IsFilterable = true, IsSortable = true)]
    public DateTimeOffset? CreatedWhen { get; set; }

    [SimpleField(IsFilterable = true, IsSortable = true)]
    public DateTimeOffset? LastPublished { get; set; }

    [SimpleField(IsFilterable = true, IsSortable = true)]
    public DateTimeOffset? PublishFrom { get; set; }

    [SimpleField(IsFilterable = true, IsSortable = true)]
    public DateTimeOffset? PublishTo { get; set; }

    [SimpleField(IsFilterable = true, IsSortable = true)]
    public DateTimeOffset? SortDate { get; set; }
     

    // ---------- Event ----------

    [SearchableField]
    public string EventContactEmail { get; set; } = string.Empty;

    [SearchableField]
    public string EventContactName { get; set; } = string.Empty;

    [SimpleField]
    public string EventContactPhone { get; set; } = string.Empty;

    [SimpleField(IsFilterable = true, IsSortable = true)]
    public DateTimeOffset? EventDateFrom { get; set; }

    [SimpleField(IsFilterable = true, IsSortable = true)]
    public DateTimeOffset? EventDateTo { get; set; }

    [SimpleField(IsFilterable = true)]
    public bool? EventIsPrivate { get; set; }

    [SearchableField]
    public string EventLocationTitle { get; set; } = string.Empty;

    [SearchableField]
    public string EventLocationCity { get; set; } = string.Empty;

    [SimpleField]
    public string EventLocationAddress1 { get; set; } = string.Empty;

    [SimpleField]
    public string EventLocationAddress2 { get; set; } = string.Empty;

    [SimpleField(IsFilterable = true, IsFacetable = true)]
    public string EventLocationState { get; set; } = string.Empty;

    [SimpleField(IsFilterable = true, IsFacetable = true)]
    public string EventLocationCountry { get; set; } = string.Empty;

    [SimpleField]
    public string EventLocationZip { get; set; } = string.Empty;

    // ---------- Person ----------

    [SearchableField]
    public string PersonFirstName { get; set; } = string.Empty;

    [SearchableField]
    public string PersonLastName { get; set; } = string.Empty;

    [SearchableField]
    public string PersonJobTitle { get; set; } = string.Empty;

    [SimpleField]
    public string PersonEmail { get; set; } = string.Empty;

    [SimpleField]
    public string PersonPhone { get; set; } = string.Empty;

    [SimpleField]
    public string PersonMobilePhone { get; set; } = string.Empty;

    [SimpleField]
    public string PersonDirectDialNumber { get; set; } = string.Empty;

    [SimpleField]
    public string PersonSurnamePrefixes { get; set; } = string.Empty;

    // ---------- Deal / Client ----------

    [SearchableField]
    public string ClientName { get; set; } = string.Empty;

    [SearchableField]
    public string DealContent { get; set; } = string.Empty;

    [SearchableField]
    public string CpeDescription { get; set; } = string.Empty;

    [SimpleField(IsFilterable = true)]
    public int? RemoteDealID { get; set; }

    [SimpleField(IsFilterable = true)]
    public int? SectionDealsID { get; set; }

    // ---------- Taxonomy ----------

    [SimpleField(IsFilterable = true, IsFacetable = true)]
    public string RegionName { get; set; } = string.Empty;

    [SimpleField(IsFilterable = true)]
    public int? LocationCityID { get; set; }

    // ---------- CMS ----------

    [SimpleField(IsFilterable = true)]
    public int? NodeID { get; set; }

    // ---------- Social ----------

    [SimpleField]
    public string Facebook { get; set; } = string.Empty;

    [SimpleField]
    public string Twitter { get; set; } = string.Empty;

    [SimpleField]
    public string LinkedIn { get; set; } = string.Empty;

    // Website channel (XbyK) — if you need to filter/facet by channel.
    // (Base model has no channel field, so this is a legitimate addition.)
    [SimpleField(IsFilterable = true, IsFacetable = true)]
    public string WebsiteChannelName { get; set; } = string.Empty;

    // Web page tree path (XbyK) — for path-based filtering, replaces NodeAliasPath's role.
    [SimpleField(IsFilterable = true)]
    public string WebPageItemTreePath { get; set; } = string.Empty;

    // Content item GUID (XbyK) — stable identifier, replaces the int NodeID role.
    [SimpleField(IsFilterable = true)]
    public string WebPageItemGuid { get; set; } = string.Empty;
}
