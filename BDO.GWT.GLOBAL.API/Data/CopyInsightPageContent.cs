using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BDO.GWT.GLOBAL.API.Data
{
    public class CopyInsightPageContent : IPageContent
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = null!;

        [JsonRequired]
        public bool ShowInsightTitle { get; set; }

        public string? Description { get; set; }

        [JsonRequired]
        public bool ShowInsightDescription { get; set; }

        public string? InsightContent { get; set; }

        [MaxLength(200)]
        public string? CallToActionPosition { get; set; }

        [MaxLength(200)]
        public string? Attachments { get; set; }

        [MaxLength(200)]
        public string? InsightAttachment { get; set; }

        [MaxLength(200)]
        public string? DownloadButtonTitle { get; set; }

        [MaxLength(200)]
        public string? DownloadButtonText { get; set; }

        [MaxLength(200)]
        public string? Contentsharing { get; set; }

        [JsonRequired]
        public bool SharingOptOut { get; set; }

        [JsonRequired]
        public bool SharingHeadline { get; set; }

        [MaxLength(200)]
        public string? OriginalSiteGuid { get; set; }

        [MaxLength(200)]
        public string? OriginalDocumentAttachmentType { get; set; }

        [MaxLength(200)]
        public List<string> OriginalIndustries { get; set; } = [];

        [MaxLength(200)]
        public List<string> OriginalServiceLines { get; set; } = [];

        [JsonRequired]
        public bool OriginalInvolvedCountriesAll { get; set; }

        [MaxLength(200)]
        public List<string> OriginalInvolvedCountries { get; set; } = [];

        [JsonRequired]
        public bool OriginalDocumentOwnerOptOut { get; set; }

        [MaxLength(200)]
        public string? OriginalDocumentOwnerName { get; set; }

        [MaxLength(200)]
        public string? OriginalDocumentOwnerEmail { get; set; }

        [MaxLength(200)]
        public string? SharingSourceFirm { get; set; }

        public string? SharingInstructions { get; set; }

        [JsonRequired]
        public bool ShowCallToAction { get; set; }

        public string? CallToActionDescription { get; set; }

        [MaxLength(200)]
        public string? CallToActionButtonText { get; set; }

        [MaxLength(2000)]
        public string? CallToActionButtonUrl { get; set; }

        [MaxLength(200)]
        public List<string> ImageFile { get; set; } = [];

        [Required]
        [MaxLength(200)]
        public string ImageFileAltText { get; set; } = null!;

        public DateTime? PublishFrom { get; set; }

        public DateTime? PublishTo { get; set; }

        public string? RedirectOnUnpublish { get; set; }
    }
}
