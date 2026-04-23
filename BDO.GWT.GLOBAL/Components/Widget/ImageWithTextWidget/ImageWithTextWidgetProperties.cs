using CMS.ContentEngine;
using Kentico.PageBuilder.Web.Mvc;
using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Admin.Base.Forms;
using System.Collections.Generic;


namespace BDO.GWT.GLOBAL.Components.Widget.ImageWithTextWidget;

public class ImageWithTextWidgetProperties : IWidgetProperties
{
    [TextInputComponent(Label = "Title", Order = 0)]
    [RequiredValidationRule]
    public string Title { get; set; } = null!;

    [RichTextEditorComponent(Label = "Description", Order = 1)]
    [RequiredValidationRule]
    public string Description { get; set; } = null!;

    [TextInputComponent(Label = "Button text", Order = 2)]
    public string? ButtonText { get; set; }

    [ContentItemSelectorComponent(contentTypeNames: [ContentAttachment.CONTENT_TYPE_NAME, SocialLinks.CONTENT_TYPE_NAME], Label = "Button url", Order = 3, MaximumItems = 1)]
    public IEnumerable<ContentItemReference> ButtonURL { get; set; }

    [DropDownComponent(Label = "Primary Button link target", Order = 4, Options = "_self;Same tab\r\n_blank;New tab")]
    public string PrimaryButtonLinkTarget { get; set; } = "_self";

    [RadioGroupComponent(Label = "Button type", Order = 5, Options = "primary;Primary\r\nsecondary;Secondary\r\ntertiary;Tertiary")]
    public string? ButtonType { get; set; }

    [RadioGroupComponent(Label = "Image or embed code", Order = 6, Options = "image;Image\r\nembed;Embed code")]
    public string MediaType { get; set; } = "image";

    [TextAreaComponent(Label = "Embed code", Order = 7)]
    [VisibleIfEqualTo(nameof(MediaType), "embed")]
    public string EmbedCode { get; set; }


    [ContentItemSelectorComponent(ContentAttachment.CONTENT_TYPE_NAME, Label = "Image", Order = 8, MaximumItems = 1)]
    [VisibleIfEqualTo(nameof(MediaType), "image")]
    [RequiredValidationRule]
    public IEnumerable<ContentItemReference> ImageURL { get; set; }

    [TextInputComponent(Label = "Image alt text", Order = 9)]
    [VisibleIfEqualTo(nameof(MediaType), "image")]
    [RequiredValidationRule]
    public string ImageAltText { get; set; } = "";

    [RadioGroupComponent(Label = "Image / video side", Order = 10, Options = "left;Left\r\nright;Right")]
    public string MediaPosition { get; set; } = "left";

    [ContentItemSelectorComponent(ContentAttachment.CONTENT_TYPE_NAME, Label = "Background image", ExplanationText = "Recommended size depends on usage.", Order = 11, MaximumItems = 1)]
    public IEnumerable<ContentItemReference> BackgroundImageURL { get; set; }

    [TextInputComponent(Label = "Background image alt text", Order = 12)]
    [VisibleIfNotEmpty(nameof(BackgroundImageURL))]
    public string BackgroundImageAltText { get; set; } = "";

    [RadioGroupComponent(Label = "Light / dark theme", Order = 13, Options = "light;Light theme\r\ndark;Dark theme")]
    public string Theme { get; set; } = "light";

    [DropDownComponent(Label = "Top margin", Order = 14, Options = "0px;None\r\n20px\r\n40px\r\n60px\r\n80px")]
    public string TopMargin { get; set; } = "0px";

    [DropDownComponent(Label = "Bottom margin", Order = 15, Options = "0px;None\r\n20px\r\n40px\r\n60px\r\n80px")]
    public string BottomMargin { get; set; } = "0px";
}
