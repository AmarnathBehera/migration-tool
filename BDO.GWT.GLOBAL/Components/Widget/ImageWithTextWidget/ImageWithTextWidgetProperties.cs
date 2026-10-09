using BDO.GWT.Core.Constants;
using CMS.ContentEngine;
using Kentico.PageBuilder.Web.Mvc;
using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.Admin.Websites.FormAnnotations;
using System.Collections.Generic;

namespace BDO.GWT.Web.Widgets.ImageWithTextWidget;

public class ImageWithTextWidgetProperties : BaseWidgetProperties
{
    [TextInputComponent(Label = "Title", Order = 0)]
    [RequiredValidationRule]
    public string Title { get; set; } = null!;

    [RichTextEditorComponent(Label = "Description", Order = 1)]
    [RequiredValidationRule]
    public string Description { get; set; } = null!;

    [TextInputComponent(Label = "Button text", Order = 2)]
    public string ButtonText { get; set; }

    [DropDownComponent(Label = "URL Source", Order = 3, Options = "content;Content item\r\nmanual;Manual URL")]
    public string UrlSource { get; set; } = "content";

    [TextInputComponent(Label = "Manual URL", Order = 4)]
    [VisibleIfEqualTo(nameof(UrlSource), "manual")]         
    public string ManualUrl { get; set; }                   

    [ContentItemSelectorComponent(
        contentTypeNames: [HomePage.CONTENT_TYPE_NAME,
        SectionAbout.CONTENT_TYPE_NAME,
        AboutPage.CONTENT_TYPE_NAME,
        Contact.CONTENT_TYPE_NAME,
        Person.CONTENT_TYPE_NAME,
        LocationCity.CONTENT_TYPE_NAME,
        Locations.CONTENT_TYPE_NAME,
        SectionIndustries.CONTENT_TYPE_NAME,
        IndustryCategory.CONTENT_TYPE_NAME,
        IndustryService.CONTENT_TYPE_NAME,
        Industry.CONTENT_TYPE_NAME,
        SectionCareers.CONTENT_TYPE_NAME,
        Career.CONTENT_TYPE_NAME,
        Careers.CONTENT_TYPE_NAME,
        CareerContent.CONTENT_TYPE_NAME,
        CareerCategory.CONTENT_TYPE_NAME,
        SectionServices.CONTENT_TYPE_NAME,
        BusinessLine.CONTENT_TYPE_NAME,
        ServiceArea.CONTENT_TYPE_NAME,
        Service.CONTENT_TYPE_NAME,
        Sub_Service.CONTENT_TYPE_NAME,
        SectionNews.CONTENT_TYPE_NAME,
        NewsArticle.CONTENT_TYPE_NAME,
        NewsGroup.CONTENT_TYPE_NAME,
        Event.CONTENT_TYPE_NAME,
        EventRegistrationForm.CONTENT_TYPE_NAME,
        Webinar.CONTENT_TYPE_NAME,
        SectionEvents.CONTENT_TYPE_NAME,
        SectionSpecialties.CONTENT_TYPE_NAME,
        SpecialtiesArea.CONTENT_TYPE_NAME,
        SpecialtiesPage.CONTENT_TYPE_NAME,
        SpecialtiesCategory.CONTENT_TYPE_NAME,
        SectionEvents.CONTENT_TYPE_NAME,
        LegalPage.CONTENT_TYPE_NAME,
        NewsGroup.CONTENT_TYPE_NAME,
        MicrositeMasterPage.CONTENT_TYPE_NAME,
        MicrositePage.CONTENT_TYPE_NAME,
        SectionEvents.CONTENT_TYPE_NAME,
        Deal.CONTENT_TYPE_NAME,
        SectionDeals.CONTENT_TYPE_NAME,
        SectionInsightsPage.CONTENT_TYPE_NAME,
        Insight.CONTENT_TYPE_NAME,
        SectionBusinessStrategies.CONTENT_TYPE_NAME,
        BusinessStrategy.CONTENT_TYPE_NAME,
        InsightCategory.CONTENT_TYPE_NAME,
        InsightGroup.CONTENT_TYPE_NAME,
        SectionBlogs.CONTENT_TYPE_NAME,
        Blog.CONTENT_TYPE_NAME,
        BlogContributors.CONTENT_TYPE_NAME,
        BlogPost.CONTENT_TYPE_NAME,
        InsightSubscriptionConfirmationPage.CONTENT_TYPE_NAME,
        Insightunsubscriptionpage.CONTENT_TYPE_NAME,
        ThankYouConversionPage.CONTENT_TYPE_NAME,
        ContentAttachment.CONTENT_TYPE_NAME],
        Label = "Button url",
        Order = 4,                                           
        MaximumItems = 1)]
    [VisibleIfEqualTo(nameof(UrlSource), "content")]         
    public IEnumerable<ContentItemReference> ButtonURL { get; set; }

    [DropDownComponent(Label = "Primary Button link target", Order = 5, Options = "_self;Same tab\r\n_blank;New tab")]  // ← was 4, duplicate fixed
    public string PrimaryButtonLinkTarget { get; set; } = "_self";

    [RadioGroupComponent(Label = "Button type", Order = 6, Options = "None;None\r\nprimary;Primary\r\nsecondary;Secondary\r\ntertiary;Tertiary")]
    public string ButtonType { get; set; }

    [RadioGroupComponent(Label = "Image or embed code", Order = 7, Options = "image;Image\r\nembed;Embed code")]
    public string MediaType { get; set; } = "image";

    [TextAreaComponent(Label = "Embed code", Order = 8)]
    [VisibleIfEqualTo(nameof(MediaType), "embed")]
    public string EmbedCode { get; set; }                  

    [ContentItemSelectorComponent(ContentAttachment.CONTENT_TYPE_NAME, Label = "Image", Order = 9, MaximumItems = 1)]
    [VisibleIfEqualTo(nameof(MediaType), "image")]
    [RequiredValidationRule]
    public IEnumerable<ContentItemReference> ImageURL { get; set; }   

    [TextInputComponent(Label = "Image alt text", Order = 10)]
    [VisibleIfEqualTo(nameof(MediaType), "image")]
    [RequiredValidationRule]
    public string ImageAltText { get; set; } = "";

    [RadioGroupComponent(Label = "Image / video side", Order = 11, Options = "left;Left\r\nright;Right")]
    public string MediaPosition { get; set; } = "left";

    [ContentItemSelectorComponent(
        ContentAttachment.CONTENT_TYPE_NAME,
        Label = "Background image",
        ExplanationText = "Recommended size depends on usage.",
        Order = 12,
        MaximumItems = 1)]
    public IEnumerable<ContentItemReference> BackgroundImageURL { get; set; }  

    [TextInputComponent(Label = "Background image alt text", Order = 13)]
    [VisibleIfNotEmpty(nameof(BackgroundImageURL))]
    [RequiredValidationRule]
    public string BackgroundImageAltText { get; set; } = "";

    [RadioGroupComponent(Label = "Light / dark theme", Order = 14, Options = "light;Light theme\r\ndark;Dark theme")]
    public string Theme { get; set; } = "light";

    [DropDownComponent(Label = "Top margin", Order = 15, Options = "0px;None\r\n20px\r\n40px\r\n60px\r\n80px")]
    public string TopMargin { get; set; } = "0px";

    [DropDownComponent(Label = "Bottom margin", Order = 16, Options = "0px;None\r\n20px\r\n40px\r\n60px\r\n80px")]
    public string BottomMargin { get; set; } = "0px";

}


public enum UrlSourceType
{
    Manual,
    ContentItem
}

