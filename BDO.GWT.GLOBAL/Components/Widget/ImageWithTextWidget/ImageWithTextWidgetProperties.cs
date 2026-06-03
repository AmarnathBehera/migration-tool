using CMS.ContentEngine;
using Kentico.PageBuilder.Web.Mvc;
using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Admin.Base.Forms;
using System.Collections.Generic;

namespace BDO.GWT.Web.Widgets.ImageWithTextWidget;

public class ImageWithTextWidgetProperties : IWidgetProperties
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
        contentTypeNames: [ContentAttachment.CONTENT_TYPE_NAME, SocialLinks.CONTENT_TYPE_NAME],
        Label = "Button url",
        Order = 5,                                           
        MaximumItems = 1)]

    [VisibleIfEqualTo(nameof(UrlSource), "content")]         
    public IEnumerable<ContentItemReference> ButtonURL { get; set; } 

    [DropDownComponent(Label = "Primary Button link target", Order = 6, Options = "_self;Same tab\r\n_blank;New tab")]  // ← was 4, duplicate fixed
    public string PrimaryButtonLinkTarget { get; set; } = "_self";

    [RadioGroupComponent(Label = "Button type", Order = 5, Options = "None;None\r\nprimary;Primary\r\nsecondary;Secondary\r\ntertiary;Tertiary")]
    public string? ButtonType { get; set; }

    [RadioGroupComponent(Label = "Image or embed code", Order = 8, Options = "image;Image\r\nembed;Embed code")]
    public string MediaType { get; set; } = "image";

    [TextAreaComponent(Label = "Embed code", Order = 9)]
    [VisibleIfEqualTo(nameof(MediaType), "embed")]
    public string EmbedCode { get; set; }                  

    [ContentItemSelectorComponent(ContentAttachment.CONTENT_TYPE_NAME, Label = "Image", Order = 10, MaximumItems = 1)]
    [VisibleIfEqualTo(nameof(MediaType), "image")]
    public IEnumerable<ContentItemReference> ImageURL { get; set; }   

    [TextInputComponent(Label = "Image alt text", Order = 11)]
    [VisibleIfEqualTo(nameof(MediaType), "image")]
    public string ImageAltText { get; set; } = "";

    [RadioGroupComponent(Label = "Image / video side", Order = 12, Options = "left;Left\r\nright;Right")]
    public string MediaPosition { get; set; } = "left";

    [ContentItemSelectorComponent(
        ContentAttachment.CONTENT_TYPE_NAME,
        Label = "Background image",
        ExplanationText = "Recommended size depends on usage.",
        Order = 13,
        MaximumItems = 1)]
    public IEnumerable<ContentItemReference> BackgroundImageURL { get; set; }  // ← nullable

    [TextInputComponent(Label = "Background image alt text", Order = 14)]
    [VisibleIfNotEmpty(nameof(BackgroundImageURL))]
    public string BackgroundImageAltText { get; set; } = "";

    [RadioGroupComponent(Label = "Light / dark theme", Order = 15, Options = "light;Light theme\r\ndark;Dark theme")]
    public string Theme { get; set; } = "light";

    [DropDownComponent(Label = "Top margin", Order = 16, Options = "0px;None\r\n20px\r\n40px\r\n60px\r\n80px")]
    public string TopMargin { get; set; } = "0px";

    [DropDownComponent(Label = "Bottom margin", Order = 17, Options = "0px;None\r\n20px\r\n40px\r\n60px\r\n80px")]
    public string BottomMargin { get; set; } = "0px";

    [TextInputComponent(Label = "Widget ID", Order = 18, ExplanationText = "ID should be unique from any other ID on the page. If the ID contains multiple words, words must be joined with a '-' (e.g. three-word-id). No other special characters and no spaces.'")]

    public string WidgetID { get; set; }
}

public enum UrlSourceType
{
    Manual,
    ContentItem
}