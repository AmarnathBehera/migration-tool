using System;
using System.Text.Json.Serialization;

namespace BDO.GWT.Core.DTOs.AzureCognitiveSearch.Models
{
    /// <summary>
    /// Query/result DTO for the XbyK Azure AI Search index produced by GlobalSearchModel.
    /// IMPORTANT: Kentico's FieldBuilder uses the C# PROPERTY NAME as the Azure field name,
    /// so every [JsonPropertyName] here must match a property name in GlobalSearchModel
    /// (or a BaseAzureSearchModel field: Url, ContentTypeName, LanguageName, ItemGuid, ObjectID, Name).
    /// </summary>
    public partial class AzureSearchDocument
    {
        // ---------- Base fields (BaseAzureSearchModel) ----------

        [JsonPropertyName("ObjectID")]
        public string? SysId { get; set; }   // key field

        [JsonPropertyName("Name")]
        public string? DocumentName { get; set; }

        [JsonPropertyName("Url")]
        public string? Url { get; set; }

        [JsonPropertyName("ContentTypeName")]   // was "classname"
        public string? ClassName { get; set; }

        [JsonPropertyName("LanguageName")]      // was "sys_culture"
        public string? SysCulture { get; set; }

        [JsonPropertyName("ItemGuid")]
        public string? ItemGuid { get; set; }

        // ---------- IDs ----------

        [JsonPropertyName("AboutPageID")]
        public int? AboutPageID { get; set; }

        [JsonPropertyName("BlogID")]
        public int? BlogID { get; set; }

        [JsonPropertyName("BlogPostID")]
        public int? BlogPostID { get; set; }

        [JsonPropertyName("CareersID")]
        public int? CareersID { get; set; }

        [JsonPropertyName("DealID")]
        public int? DealID { get; set; }

        [JsonPropertyName("EventID")]
        public int? EventID { get; set; }

        [JsonPropertyName("InsightID")]
        public int? InsightID { get; set; }

        [JsonPropertyName("NewsArticleID")]
        public int? NewsArticleID { get; set; }

        [JsonPropertyName("WebinarID")]
        public int? WebinarID { get; set; }

        // ---------- Classification ----------

        [JsonPropertyName("ContentType")]
        public string? ContentType { get; set; }

        [JsonPropertyName("EventType")]
        public string? EventType { get; set; }

        // ---------- Content ----------

        [JsonPropertyName("Content")]
        public string? Content { get; set; }

        [JsonPropertyName("ContentBody")]
        public string? ContentBody { get; set; }

        [JsonPropertyName("DescriptionBody")]
        public string? DescriptionBody { get; set; }

        [JsonPropertyName("DocumentContent")]
        public string? DocumentContent { get; set; }

        // ---------- Metadata ----------

        [JsonPropertyName("DocumentPageTitle")]
        public string? DocumentPageTitle { get; set; }

        [JsonPropertyName("DocumentPageDescription")]
        public string? DocumentPageDescription { get; set; }

        [JsonPropertyName("DocumentPageKeywords")]
        public string? DocumentPageKeyWords { get; set; }

        [JsonPropertyName("MetadataTitle")]
        public string? MetadataTitle { get; set; }

        [JsonPropertyName("MetadataDescription")]
        public string? MetadataDescription { get; set; }

        [JsonPropertyName("MetadataTeaserImage")]
        public string? MetadataTeaserImage { get; set; }

        [JsonPropertyName("MetadataTeaserImageAltText")]
        public string? MetadataTeaserImageAltText { get; set; }

        [JsonPropertyName("DocumentTags")]
        public string? DocumentTags { get; set; }

        // ---------- Hero / Images ----------

        [JsonPropertyName("HeroTitle")]
        public string? HeroTitle { get; set; }

        [JsonPropertyName("HeroDescription")]
        public string? HeroDescription { get; set; }

        [JsonPropertyName("HeroBackgroundImage")]
        public string? HeroBackgroundImage { get; set; }

        [JsonPropertyName("Image")]
        public string? Image { get; set; }

        [JsonPropertyName("ImageAltText")]
        public string? ImageAltText { get; set; }

        [JsonPropertyName("SectionTitle")]
        public string? SectionTitle { get; set; }

        // ---------- Document information ----------

        [JsonPropertyName("DocumentID")]
        public int? DocumentID { get; set; }

        [JsonPropertyName("DocumentNodeID")]
        public int? DocumentNodeID { get; set; }

        [JsonPropertyName("DocumentForeignKeyValue")]
        public int? DocumentForeignKeyValue { get; set; }

        [JsonPropertyName("DocumentTagGroupID")]
        public int? DocumentTagGroupID { get; set; }

        [JsonPropertyName("DocumentCreatedByUserID")]
        public int? DocumentCreatedByUserID { get; set; }

        [JsonPropertyName("DocumentModifiedByUserID")]
        public int? DocumentModifiedByUserID { get; set; }

        [JsonPropertyName("SearchExcluded")]    // was "documentsearchexcluded"
        public bool? DocumentSearchExcluded { get; set; }

        // ---------- Dates ----------

        [JsonPropertyName("CreatedWhen")]       // was "documentcreatedwhen" / "sys_created"
        public DateTimeOffset? DocumentCreatedWhen { get; set; }

        [JsonPropertyName("LastPublished")]     // was "documentlastpublished"
        public DateTimeOffset? DocumentLastPublished { get; set; }

        [JsonPropertyName("PublishFrom")]       // was "documentpublishfrom"
        public DateTimeOffset? PublishFrom { get; set; }

        [JsonPropertyName("PublishTo")]         // was "documentpublishto"
        public DateTimeOffset? PublishTo { get; set; }

        [JsonPropertyName("SortDate")]
        public DateTimeOffset? SortDate { get; set; }

        // ---------- Event ----------

        [JsonPropertyName("EventContactEmail")]
        public string? EventContactEmail { get; set; }

        [JsonPropertyName("EventContactName")]
        public string? EventContactName { get; set; }

        [JsonPropertyName("EventContactPhone")]
        public string? EventContactPhone { get; set; }

        [JsonPropertyName("EventDateFrom")]
        public DateTimeOffset? EventDateFrom { get; set; }

        [JsonPropertyName("EventDateTo")]
        public DateTimeOffset? EventDateTo { get; set; }

        [JsonPropertyName("EventIsPrivate")]
        public bool? EventIsPrivate { get; set; }

        [JsonPropertyName("EventLocationTitle")]
        public string? EventLocationTitle { get; set; }

        [JsonPropertyName("EventLocationCity")]
        public string? EventLocationCity { get; set; }

        [JsonPropertyName("EventLocationAddress1")]
        public string? EventLocationAddress1 { get; set; }

        [JsonPropertyName("EventLocationAddress2")]
        public string? EventLocationAddress2 { get; set; }

        [JsonPropertyName("EventLocationState")]
        public string? EventLocationState { get; set; }

        [JsonPropertyName("EventLocationCountry")]
        public string? EventLocationCountry { get; set; }

        [JsonPropertyName("EventLocationZip")]
        public string? EventLocationZip { get; set; }

        // ---------- Person ----------

        [JsonPropertyName("PersonFirstName")]
        public string? PersonFirstName { get; set; }

        [JsonPropertyName("PersonLastName")]
        public string? PersonLastName { get; set; }

        [JsonPropertyName("PersonJobTitle")]
        public string? PersonJobTitle { get; set; }

        [JsonPropertyName("PersonEmail")]
        public string? PersonEmail { get; set; }

        [JsonPropertyName("PersonPhone")]
        public string? PersonPhone { get; set; }

        [JsonPropertyName("PersonMobilePhone")]
        public string? PersonMobilePhone { get; set; }

        [JsonPropertyName("PersonDirectDialNumber")]
        public string? PersonDirectDialNumber { get; set; }

        [JsonPropertyName("PersonSurnamePrefixes")]
        public string? PersonSurnamePrefixes { get; set; }

        // ---------- Deal / Client ----------

        [JsonPropertyName("ClientName")]
        public string? ClientName { get; set; }

        [JsonPropertyName("DealContent")]
        public string? DealContent { get; set; }

        [JsonPropertyName("CpeDescription")]
        public string? CpeDescription { get; set; }

        [JsonPropertyName("RemoteDealID")]
        public int? RemoteDealID { get; set; }

        [JsonPropertyName("SectionDealsID")]
        public int? SectionDealsID { get; set; }

        // ---------- Taxonomy ----------

        [JsonPropertyName("RegionName")]
        public string? RegionName { get; set; }

        [JsonPropertyName("LocationCityID")]
        public int? LocationCityID { get; set; }

        // ---------- CMS ----------

        [JsonPropertyName("NodeID")]
        public int? NodeID { get; set; }

        // ---------- Social ----------

        [JsonPropertyName("Facebook")]
        public string? Facebook { get; set; }

        [JsonPropertyName("Twitter")]
        public string? Twitter { get; set; }

        [JsonPropertyName("LinkedIn")]
        public string? LinkedIn { get; set; }

        // ---------- Website channel (XbyK) ----------

        [JsonPropertyName("WebsiteChannelName")]
        public string? WebsiteChannelName { get; set; }

        [JsonPropertyName("WebPageItemTreePath")]
        public string? WebPageItemTreePath { get; set; }

        [JsonPropertyName("WebPageItemGuid")]
        public string? WebPageItemGuid { get; set; }
    }
}
