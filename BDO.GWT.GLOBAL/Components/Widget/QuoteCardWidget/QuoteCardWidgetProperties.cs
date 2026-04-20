using CMS.ContentEngine;
using Kentico.Components.Web.Mvc.FormComponents;
using Kentico.Forms.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using Kentico.Xperience.Admin.Base.FormAnnotations;
using System.Collections.Generic;

namespace BDO.GWT.Xperience.Core.LiveSite.Components.Widgets.QuoteCardWidget;

public class QuoteCardWidgetProperties : IWidgetProperties
{
    [TextInputComponent(Label = "Heading", Order = 0)]
    public string? Heading { get; set; }

    [RichTextEditorComponent(Order = 1, Label = "Subheading")]
    public string? Subheading { get; set; }

    [ContentItemSelectorComponent(ContentAttachment.CONTENT_TYPE_NAME,
    Label = "Image",
    Order = 2,
    ExplanationText = "Recommended size depends on usage but a 3:2 aspect ratio works well for most instances.")]
    public IEnumerable<ContentItemReference> Image { get; set; } = new List<ContentItemReference>();
  
    [TextInputComponent(Label = "Image alt text", Order = 3)]
    public string? ImageAltText { get; set; }

    [RichTextEditorComponent(Order = 4, Label = "Quote text", ExplanationText = "Recommended quote text length is under 100 words.")]
    public string QuoteText { get; set; } = null!;

    [TextInputComponent(Label = "Quote’s author", Order = 5)]
    public string QuotesAuthor { get; set; } = null!;

    [TextInputComponent(Label = "Author’s job title", Order = 6)]
    public string? AuthorsJobTitle { get; set; }

    [TextInputComponent(Label = "Button text", Order = 7)]
    public string? ButtonText { get; set; } = null!;

    [ContentItemSelectorComponent(new string[] { HomePage.CONTENT_TYPE_NAME, Navigation.CONTENT_TYPE_NAME }, Label = "Button URL", Order = 8)]
    public IEnumerable<ContentItemReference> ButtonUrl { get; set; } = new List<ContentItemReference>();

    [DropDownComponent(Label = "Primary Button link target", Order = 9, Options = "_self;Same tab\r\n_blank;New tab")]

    public string PrimaryButtonLinkTarget { get; set; } = "_self";

    [RadioGroupComponent(Label = "Quote text", Order = 10, Options = "small;Small\r\nmedium;Medium\r\nlarge;Large")]
    public string Size { get; set; } = "standard";

    [DropDownComponent(Label = "Top margin", Order = 11, Options = "0px;None\n20px\n40px\n60px\n80px")]
    public string? TopMargin { get; set; }

    [DropDownComponent(Label = "Bottom margin", Order = 12, Options = "0px;None\n20px\n40px\n60px\n80px")]
    public string? BottomMargin { get; set; }
}