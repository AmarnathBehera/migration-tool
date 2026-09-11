using CMS.ContentEngine;
using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Admin.Websites.FormAnnotations;
using Microsoft.AspNetCore.Mvc.Formatters;
using ReverseMarkdown.Converters;
using System.Collections.Generic;

namespace BDO.GWT.Web.Widgets.FlexibleBannerWidget;

public class FlexibleBannerWidgetProperties : BaseWidgetProperties
{
    [RadioGroupComponent(Label = "Layout", Order = 0, Options = "col-1;One column\r\ncol-1-shadowed;One column shadowed\r\ncol-2;Two columns\r\ncol-2-shadowed;Two columns shadowed")]
    public string Layout { get; set; } = "col-1";

    [RichTextEditorComponent(Order = 1, Label = "Description")]
    public string Description { get; set; }

    [ContentItemSelectorComponent(ContentAttachment.CONTENT_TYPE_NAME, Label = "Background image", Order = 2, MaximumItems = 1, ExplanationText = "Recommend image size: 1440x1000")]
    public IEnumerable<ContentItemReference> BackgroundImage { get; set; }

    [TextInputComponent(Label = "Background image alt text", Order = 3)]
    [VisibleIfNotEmpty(nameof(BackgroundImage))]
    [RequiredValidationRule]
    public string BackgroundImageAltText { get; set; }

    [RadioGroupComponent(Label = "Light / Dark theme", Order = 4, Options = "light;Light theme\r\ndark;Dark theme")]
    public string Theme { get; set; } = "light";

    [RadioGroupComponent(Label = "Button side", Order = 5, Options = "left;Left\r\nright;Right")]
    //[VisibleIfEqualTo(nameof(Layout), new[] {"col-2" , "col-2-shadowed"})]
    //[VisibleIfEqualTo(nameof(Layout), "col-2-shadowed")]
    [VisibleIfNotEqualTo(nameof(Layout), "col-1")]
    public string ButtonSide { get; set; } = "right";

    #region Primary button

    [RadioGroupComponent(Label = "Primary button type", Order = 6, Options = "primary;Primary\r\nsecondary;Secondary\r\ntertiary;Tertiary")]
    public string PrimaryButtonType { get; set; } = "primary";

    [TextInputComponent(Label = "Primary button text", Order = 7)]
    public string PrimaryButtonText { get; set; } = null!;

    [UrlSelectorComponent(Label = "Primary button URL", Order = 8)]
    public string PrimaryButtonUrl { get; set; } = null!;

    [DropDownComponent(Label = "Primary button link target", Order = 9, Options = "_self;Same tab\r\n_blank;New tab")]
    public string PrimaryButtonLinkTarget { get; set; } = "_self";

    #endregion

    #region Secondary button

    [RadioGroupComponent(Label = "Secondary button type", Order = 10, Options = "primary;Primary\r\nsecondary;Secondary\r\ntertiary;Tertiary")]
    public string SecondaryButtonType { get; set; } = "primary";

    [TextInputComponent(Label = "Secondary button text", Order = 11)]
    public string SecondaryButtonText { get; set; }

    [UrlSelectorComponent(Label = "Secondary button URL", Order = 12)]
    public string SecondaryButtonUrl { get; set; }

    [DropDownComponent(Label = "Secondary button link target", Order = 13, Options = "_self;Same tab\r\n_blank;New tab")]
    public string SecondaryButtonLinkTarget { get; set; } = "_self";

    #endregion

    [DropDownComponent(Label = "Top margin", Order = 14, Options = "0px;None\r\n20px\r\n40px\r\n60px\r\n80px")]
    public string TopMargin { get; set; } = null!;

    [DropDownComponent(Label = "Bottom margin", Order = 15, Options = "0px;None\r\n20px\r\n40px\r\n60px\r\n80px")]
    public string BottomMargin { get; set; } = null!;

}
