using Kentico.Components.Web.Mvc.FormComponents;
using Kentico.Forms.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;

namespace BDO.GWT.Xperience.Core.LiveSite.Components.Widgets.QuoteCardWidget;

public class QuoteCardWidgetProperties : IWidgetProperties
{
    [EditingComponent(TextInputComponent.IDENTIFIER, Order = 0, Label = "Heading")]
    public string? Heading { get; set; }

    [EditingComponent(RichTextComponent.IDENTIFIER, Order = 1, Label = "Subheading")]
    public string? Subheading { get; set; }

    [EditingComponent(UrlSelector.IDENTIFIER, Label = "Image", Order = 2, ExplanationText = "Recommended size depends on usage but a 3:2 aspect ratio works well for most instances.")]
    [EditingComponentProperty(nameof(UrlSelectorProperties.Tabs), ContentSelectorTabs.Media)]
    public string? Image { get; set; }

    [EditingComponent(TextInputComponent.IDENTIFIER, Order = 3, Label = "Image alt text")]
    public string? ImageAltText { get; set; }

    [EditingComponent(RichTextComponent.IDENTIFIER, Order = 4, Label = "Quote text", ExplanationText = "Recommended quote text length is under 100 words.")]
    [EditingComponentProperty(nameof(RichTextComponentProperties.ConfigurationName), "quoteText")]
    public string QuoteText { get; set; } = null!;

    [EditingComponent(TextInputComponent.IDENTIFIER, Order = 5, Label = "Quote’s author")]
    public string QuotesAuthor { get; set; } = null!;

    [EditingComponent(TextInputComponent.IDENTIFIER, Order = 6, Label = "Author’s job title")]
    public string? AuthorsJobTitle { get; set; }

    [EditingComponent(TextInputComponent.IDENTIFIER, Order = 7, Label = "Button text")]
    public string? ButtonText { get; set; } = null!;

    [EditingComponent(UrlSelector.IDENTIFIER, Order = 8, Label = "Button url")]
    public string? ButtonUrl { get; set; } = null!;

    [EditingComponent(DropDownComponent.IDENTIFIER, Label = "Primary Button link target", Order = 9)]
    [EditingComponentProperty(
   nameof(DropDownProperties.DataSource),
   "_self;Same tab\r\n_blank;New tab"
)]
    public string PrimaryButtonLinkTarget { get; set; } = "_self";

    [EditingComponent(RadioButtonsComponent.IDENTIFIER, Order = 10, ExplanationText = "The quote layout will adjust based on the selected text font. Please note that on mobile, the font size is reduced by 2px.", Label = "Quote text")]
    [EditingComponentProperty(nameof(RadioButtonsProperties.DataSource), "small;Small\r\nstandard;Standard\r\nlarge;Large")]
    public string Size { get; set; } = "standard";

    [EditingComponent(DropDownComponent.IDENTIFIER, Label = "Top margin", Order = 11)]
    [EditingComponentProperty(nameof(DropDownProperties.DataSource), "0px;None\r\n20px\r\n40px\r\n60px\r\n80px")]
    public string TopMargin { get; set; } = null!;

    [EditingComponent(DropDownComponent.IDENTIFIER, Label = "Bottom margin", Order = 12)]
    [EditingComponentProperty(nameof(DropDownProperties.DataSource), "0px;None\r\n20px\r\n40px\r\n60px\r\n80px")]
    public string BottomMargin { get; set; } = null!;
}

