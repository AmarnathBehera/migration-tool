using BDO.GWT.Xperience.Core.LiveSite.Components.VisibilityConditions;
using Kentico.Components.Web.Mvc.FormComponents;
using Kentico.Forms.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;

namespace BDO.GWT.Xperience.Core.LiveSite.Components.Widgets.ImageWithTextWidget;

public class ImageWithTextWidgetProperties : IWidgetProperties
{
    [EditingComponent(TextInputComponent.IDENTIFIER, Label = "Title", Order = 0)]
    public string Title { get; set; } = null!;

    [EditingComponent(RichTextComponent.IDENTIFIER, Label = "Description", Order = 1)]
    [EditingComponentProperty(nameof(RichTextComponentProperties.ConfigurationName), "description")]
    public string Description { get; set; } = null!;

    [EditingComponent(TextInputComponent.IDENTIFIER, Label = "Button text", Order = 2)]
    public string? ButtonText { get; set; }

    [EditingComponent(UrlSelector.IDENTIFIER, Label = "Button url", Order = 3)]
    public string? ButtonURL { get; set; }

    [EditingComponent(DropDownComponent.IDENTIFIER, Label = "Primary Button link target", Order = 4)]
    [EditingComponentProperty(
    nameof(DropDownProperties.DataSource),
    "_self;Same tab\r\n_blank;New tab"
)]    public string PrimaryButtonLinkTarget { get; set; } = "_self";

    [EditingComponent(RadioButtonsComponent.IDENTIFIER, Label = "Button type", Order = 5)]
    [EditingComponentProperty(nameof(RadioButtonsProperties.DataSource), "primary;Primary\r\nsecondary;Secondary\r\ntertiary;Tertiary")]
    public string? ButtonType { get; set; }

    [EditingComponent(RadioButtonsComponent.IDENTIFIER, Label = "Image or embed code", Order = 6)]
    [EditingComponentProperty(nameof(RadioButtonsProperties.DataSource), "image;Image\r\nembed;Embed code")]
    public string MediaType { get; set; } = "image";

    [EditingComponent(TextAreaComponent.IDENTIFIER, Label = "Embed code", Order = 7)]
    [VisibilityCondition(nameof(MediaType), typeof(ContainsVisibilityCondition))]
    [VisibilityConditionProperty(nameof(ContainsVisibilityCondition.Values), new[] { "embed" })]
    public string EmbedCode { get; set; } = "";

    [EditingComponent(UrlSelector.IDENTIFIER, Label = "Image", Order = 8)]
    [EditingComponentProperty(nameof(UrlSelectorProperties.Tabs), ContentSelectorTabs.Media)]
    [VisibilityCondition(nameof(MediaType), typeof(ContainsVisibilityCondition))]
    [VisibilityConditionProperty(nameof(ContainsVisibilityCondition.Values), new[] { "image" })]
    public string ImageURL { get; set; } = "";

    [EditingComponent(TextInputComponent.IDENTIFIER, Label = "Image alt text", Order = 9)]
    [VisibilityCondition(nameof(MediaType), typeof(ContainsVisibilityCondition))]
    [VisibilityConditionProperty(nameof(ContainsVisibilityCondition.Values), new[] { "image" })]
    public string ImageAltText { get; set; } = "";

    [EditingComponent(RadioButtonsComponent.IDENTIFIER, Label = "Image / video side", Order = 10)]
    [EditingComponentProperty(nameof(RadioButtonsProperties.DataSource), "left;Left\r\nright;Right")]
    public string MediaPosition { get; set; } = "left";

    [EditingComponent(UrlSelector.IDENTIFIER, Label = "Background image", ExplanationText = "Recommended size depends on usage.", Order = 11)]
    [EditingComponentProperty(nameof(UrlSelectorProperties.Tabs), ContentSelectorTabs.Media)]
    public string? BackgroundImageURL { get; set; }

    [EditingComponent(TextInputComponent.IDENTIFIER, Label = "Background image alt text", Order = 12)]
    [VisibilityCondition(nameof(BackgroundImageURL), ComparisonTypeEnum.IsNotNull)]
    public string BackgroundImageAltText { get; set; } = "";

    [EditingComponent(RadioButtonsComponent.IDENTIFIER, Label = "Light / dark theme", Order = 13)]
    [EditingComponentProperty(nameof(RadioButtonsProperties.DataSource), "light;Light theme\r\ndark;Dark theme")]
    public string Theme { get; set; } = "light";

    [EditingComponent(DropDownComponent.IDENTIFIER, Label = "Top margin", Order = 14)]
    [EditingComponentProperty(nameof(DropDownProperties.DataSource), "0px;None\r\n20px\r\n40px\r\n60px\r\n80px")]
    public string TopMargin { get; set; } = "0px";

    [EditingComponent(DropDownComponent.IDENTIFIER, Label = "Bottom margin", Order = 15)]
    [EditingComponentProperty(nameof(DropDownProperties.DataSource), "0px;None\r\n20px\r\n40px\r\n60px\r\n80px")]
    public string BottomMargin { get; set; } = "0px";
}
