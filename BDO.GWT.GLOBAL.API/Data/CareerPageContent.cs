using System.ComponentModel.DataAnnotations;

namespace BDO.GWT.GLOBAL.API.Data
{
    public class CareerPageContent : IMetadataContent
    {
        [Required]
        public string MetadataTitle { get; set; } = string.Empty;

        [Required]
        public string? MetadataDescription { get; set; }

        //[MaxLength(200)]
        //public string? MetadataMenuGroup { get; set; }

        [MaxLength(200)]
        public string? MetadataTeaserImage { get; set; }

        [Required]
        [MaxLength(200)]
        public string MetadataTeaserImageAltText { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? MetadataCanonical { get; set; }

        [MaxLength(60)]
        public string? MetadataOGTitle { get; set; }

        [MaxLength(500)]
        public string? MetadataOGDescription { get; set; }

        [MaxLength(500)]
        public string? MetadataOGImage { get; set; }

        public bool MetadataOGTwitterImageSizeCheckbox { get; set; }

        [MaxLength(500)]
        public string? MetadataOGTwitterImage { get; set; }

        public bool MetadataNofollow { get; set; }

        public bool MetadataNoindex { get; set; }

        public bool IncludeInSitemap { get; set; }

        public DateTime? PublishFrom { get; set; }

        public DateTime? PublishTo { get; set; }

        public string? RedirectOnUnpublish { get; set; }

        // Career page specific fields
        [Required]
        [MaxLength(200)]
        public string HeroLayout { get; set; } = null!;

        [MaxLength(200)]
        public string? HeroTitle { get; set; }

        [MaxLength(200)]
        public string? HeroDescription { get; set; }

        [Required]
        [MaxLength(200)]
        public string? HeroBackgroundImage { get; set; }

        [Required]
        [MaxLength(200)]
        public string? HeroBackgroundImageAltText { get; set; }

        [Required]
        [MaxLength(200)]
        public string? HeroBackgroundImage_Mobile { get; set; }

        [Required]
        [MaxLength(200)]
        public string? HeroBackgroundImage_MobileAltText { get; set; }

        [MaxLength(200)]
        public string? HeroCallToActionButtonType { get; set; }

        [Required]
        [MaxLength(200)]
        public string? HeroCallToActionButtonText { get; set; }

        [Required]
        [MaxLength(200)]
        public string? HeroCallToActionButtonUrl { get; set; }

        [Required]
        public bool ShowSidebar { get; set; }

        [Required]
        public bool ShowPrintButton { get; set; }

        [MaxLength(200)]
        public string? DownloadButtonAttachment { get; set; }

        [Required]
        public bool ShowCallToAction { get; set; }

        [MaxLength(200)]
        public string? CallToActionDescription { get; set; }

        [MaxLength(50)]
        public string? CallToActionButtonText { get; set; }

        [MaxLength(200)]
        public string? CallToActionButtonUrl { get; set; }

        [Required]
        public bool ShowSocialMediaButtons { get; set; }
        
        public int CareerContactPeopleID { get; set; }
        public int[] MetadataRelevantContactPerson { get; set; } = Array.Empty<int>();
        public int[] MetadataOfficeLocation { get; set; } = Array.Empty<int>();


        [MaxLength(50)]
        public string CareerReference { get; set; } = string.Empty;

        public string CareerJobTitle { get; set; } = string.Empty;

        public string CareerLevel { get; set; } = string.Empty;

        public string CareerEmploymentType { get; set; } = string.Empty;

        [MaxLength(200)]
        public string CareerApplyForm { get; set; } = string.Empty;

    }
}
