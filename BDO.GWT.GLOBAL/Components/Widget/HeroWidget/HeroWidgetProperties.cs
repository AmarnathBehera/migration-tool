
using BDO;
using BDO.GWT.Core.Constants;
using BDO.GWT.Web.Widgets;
using CMS.ContentEngine;
using Kentico.Forms.Web.Mvc;

//using Kentico.Forms.Web.Mvc;
using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.Admin.Websites.FormAnnotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BDO.GWT.Web.Components.Widget.HeroWidget;

public class HeroWidgetProperties : BaseWidgetProperties
{
    [RadioGroupComponent( Label = "Layout",Options = $"{HeroLayout.BannerWithImage};Banner with Image\r\n{HeroLayout.Banner};Banner\r\n{HeroLayout.Hero};Hero", Order = 0)]
    public string Layout { get; set; } = null!;

    [TextInputComponent( Label = "Title", Order = 1)]
    public string Title { get; set; }

    [TextAreaComponent( Label = "Description", Order = 2)]
    public string Description { get; set; }

    [ContentItemSelectorComponent(contentTypeName: ContentAttachment.CONTENT_TYPE_NAME, Label = "Background image", ExplanationText = "Recommended size: 1440x400", Order = 3)]
    [VisibleIfNotEqualTo(nameof(Layout), HeroLayout.Banner)]
    public IEnumerable<ContentItemReference> BackgroundImage { get; set; } = Enumerable.Empty<ContentItemReference>();

    [TextInputComponent( Label = "Background image alt text", Order = 4)]
    [VisibleIfNotEqualTo(nameof(Layout),HeroLayout.Banner)]
    public string BackgroundImageAltText { get; set; } = "";

    [ContentItemSelectorComponent(contentTypeName: ContentAttachment.CONTENT_TYPE_NAME, Label = "Mobile background image", ExplanationText = "Recommended size: 720x1280", Order = 5)]
    [VisibleIfNotEqualTo(nameof(Layout), HeroLayout.Banner)]
    public IEnumerable<ContentItemReference> MobileBackgroundImage { get; set; } = Enumerable.Empty<ContentItemReference>();

    [TextInputComponent( Label = "Background image alt text", Order = 6)]
    [VisibleIfNotEqualTo(nameof(Layout), HeroLayout.Banner)]
    public string MobileBackgroundImageAltText { get; set; } = "";

    [RadioGroupComponent( Label = "CTA button type", Order = 7,Options = $"{CallToActionButtonType.None};None\r\n{CallToActionButtonType.Primary};Primary\r\n{CallToActionButtonType.Secondary};Secondary\r\n{CallToActionButtonType.Tertiary};Tertiary")]
    public string CtaButtonType { get; set; } = CallToActionButtonType.None;

    [TextInputComponent( Label = "CTA button text", Order = 8)]
    [VisibleIfNotEqualTo(nameof(CtaButtonType),  CallToActionButtonType.None)]
    public string CtaButtonText { get; set; } = "";

    [UrlSelectorComponent( Label = "CTA button URL", Order = 9)]

    [VisibleIfNotEqualTo(nameof(CtaButtonType),  CallToActionButtonType.None)]
    public string CtaButtonUrl { get; set; } = "";

    [DropDownComponent( Label = "CTA button URLTarget", Order = 10,Options = "_self;Same tab\r\n_blank;New tab")]
    public string CtaButtonUrlTarget { get; set; } = "_self";
}

