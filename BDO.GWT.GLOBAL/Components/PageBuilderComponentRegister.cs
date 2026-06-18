using BDO;
using BDO.GWT.GLOBAL.Components.Widget.KeyTakeawaysWidget;
using BDO.GWT.GLOBAL.Components.Widget.LocalNavigationWidget;
using BDO.GWT.GLOBAL.Widgets.NewsCarouselWidget;
using BDO.GWT.Web.Components.Widget.TabAccordionWidget;
using BDO.GWT.Web.Widgets.AccordionWidget;
using BDO.GWT.Web.Widgets.FaqWidget;
using BDO.GWT.Web.Widgets.FlexibleBannerWidget;
using BDO.GWT.Web.Widgets.FlexibleContentWidget;
using BDO.GWT.Web.Widgets.ImageWithTextWidget;
using BDO.GWT.Web.Widgets.QuoteCardWidget;
using BDO.GWT.Web.Widgets.RichTextWidget;
using BDO.GWT.Web.Widgets.SocialMediaButtonsWidget;
using BDO.GWT.Web.Widgets.TabAccordionWidget;
using BDO.GWT.Xperience.Core.LiveSite.Components.FormComponents;
using BDO.GWT.Xperience.Core.LiveSite.Components.FormSections.FormSectionProperties;
using BDO.GWT.Xperience.Core.LiveSite.Components.Widgets.ButtonWidget;
using BDO.GWT.Xperience.Core.LiveSite.Components.Widgets.EventsWebinarsSliderWidget;
using BDO.GWT.Xperience.Core.LiveSite.Components.Widgets.ExternalNavigationWidget;
using BDO.GWT.Xperience.Core.LiveSite.Components.Widgets.TiledLinkSectionWidget;
using BDO.GWT.Xperience.Core.LiveSite.Components.Widgets.ViewGlobalLocations;
using BDO.GWT.Xperience.LiveSite.Components.Section;
using Kentico.Forms.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;

// Widgets
[assembly: RegisterWidget(ComponentIdentifiers.Widgets.Imagewithtext, typeof(ImageWithTextWidget), "Image With Text", typeof(ImageWithTextWidgetProperties), AllowCache = true, IconClass = "icon-outdent")]
[assembly: RegisterWidget(ComponentIdentifiers.Widgets.QuoteCard, typeof(QuoteCardWidget), "Quote Card Widget", typeof(QuoteCardWidgetProperties), AllowCache = true, IconClass = "icon-right-double-quotation-mark")]
[assembly: RegisterWidget(ComponentIdentifiers.Widgets.ExternalNavigation, typeof(ExternalNavigationWidget), "External Navigation Item", typeof(ExternalNavigationWidgetProperties), AllowCache = true, IconClass = "icon-chain")]
[assembly: RegisterWidget(ComponentIdentifiers.Widgets.TiledLink, typeof(TiledLinkSectionWidget), "Tiled link section", typeof(TiledLinkSectionWidgetProperties), AllowCache = true, IconClass = "icon-l-grid-2-2")]
[assembly: RegisterWidget(ComponentIdentifiers.Widgets.Globallocation, typeof(ViewGlobalLocationsWidget), "View Global Location", typeof(ViewGlobalLocationsWidgetProperties), AllowCache = true, IconClass = "icon-ribbon")]
[assembly: RegisterWidget(ComponentIdentifiers.Widgets.RichText, typeof(RichTextWidget), "Rich Text", typeof(RichTextWidgetProperties), AllowCache = true, IconClass = "icon-l-text")]
[assembly: RegisterWidget(ComponentIdentifiers.Widgets.AccordionWidget, typeof(AccordionWidget), "Accordion Widget", typeof(AccordionWidgetProperties), AllowCache = true, IconClass = "icon-accordion")]
[assembly: RegisterWidget(ComponentIdentifiers.Widgets.Button, typeof(ButtonWidget), "Button", typeof(ButtonWidgetProperties), AllowCache = true, IconClass = "icon-button")]
[assembly: RegisterWidget(ComponentIdentifiers.Widgets.LocalNavigation, typeof(LocalNavigationWidget), "Local Navigation Item", typeof(LocalNavigationWidgetProperties), AllowCache = true, IconClass = "icon-chain")]
[assembly: RegisterWidget(ComponentIdentifiers.Widgets.KeyTakeaways, typeof(KeyTakeawaysWidget), "Key Takeaways", typeof(KeyTakeawaysWidgetProperties), AllowCache = true, IconClass = "icon-list-bullets")]
[assembly: RegisterWidget(ComponentIdentifiers.Widgets.EventsWebinarsSlider, typeof(EventsWebinarsSliderWidget), "Events Webinar Slider Widget", typeof(EventsWebinarsSliderWidgetProperties), AllowCache = true, IconClass = "icon-carousel")]
[assembly: RegisterWidget(ComponentIdentifiers.Widgets.FaqWidget, typeof(FaqWidget), "FAQ Widget", typeof(FaqWidgetProperties), AllowCache = true, IconClass = "icon-bubbles")]
[assembly: RegisterWidget(ComponentIdentifiers.Widgets.FlexibleBanner, typeof(FlexibleBannerWidget), "Flexible Banner", typeof(FlexibleBannerWidgetProperties), AllowCache = true, IconClass = "icon-ribbon")]
[assembly: RegisterWidget(ComponentIdentifiers.Widgets.FlexibleContent, typeof(FlexibleContentWidget), "Flexible Content", typeof(FlexibleContentWidgetProperties), AllowCache = true, IconClass = "icon-newspaper")]
[assembly: RegisterWidget(ComponentIdentifiers.Widgets.TabAccordion, typeof(TabAccordionWidget), "Tab Accordion", typeof(TabAccordionWidgetProperties), AllowCache = true, IconClass = "icon-tab")]
[assembly: RegisterWidget(ComponentIdentifiers.Widgets.NewsCarousel,typeof(NewsCarouselWidget),"News carousel",typeof(NewsCarouselWidgetProperties),AllowCache = true,IconClass = "icon-carousel")]
[assembly: RegisterWidget(ComponentIdentifiers.Widgets.SocialMediaButtons,typeof(SocialMediaButtonsWidget),"Social media buttons",typeof(SocialMediaButtonsWidgetProperties),AllowCache = true,IconClass = "icon-paper-plane")]

// Sections
[assembly: RegisterSection(identifier: ComponentIdentifiers.Sections.Container,name: "Container",propertiesType: typeof(BaseSectionProperties),customViewName: "~/Components/Section/SectionView/_ContainerSection.cshtml")]
[assembly: RegisterSection(identifier: ComponentIdentifiers.Sections.TwoColumns,name: "Two Columns: Even", propertiesType: typeof(SectionWithGapProperties),customViewName: "~/Components/Section/SectionView/_TwoColumnSection.cshtml")]
[assembly: RegisterSection(identifier: ComponentIdentifiers.Sections.TwoThirdOneThird,name: "Two Columns: 2/3 - 1/3", propertiesType: typeof(SectionWithGapProperties),customViewName: "~/Components/Section/SectionView/_TwoThirdOneThirdSection.cshtml")]
[assembly: RegisterSection(identifier: ComponentIdentifiers.Sections.OneThirdTwoThird,name: "Two Columns: 1/3 - 2/3", propertiesType: typeof(SectionWithGapProperties),customViewName: "~/Components/Section/SectionView/_OneThirdTwoThirdSection.cshtml")]
[assembly: RegisterSection(identifier: ComponentIdentifiers.Sections.ThreeColumns,name: "Three Columns: Even", propertiesType: typeof(SectionWithGapProperties),customViewName: "~/Components/Section/SectionView/_ThreeColumnSection.cshtml")]
[assembly: RegisterSection(identifier: ComponentIdentifiers.Sections.FourColumns,name: "Four Columns: Even", propertiesType: typeof(SectionWithGapProperties),customViewName: "~/Components/Section/SectionView/_FourColumnSection.cshtml")]
[assembly: RegisterSection(identifier: ComponentIdentifiers.Sections.FiveColumns,name: "Five Columns: Even", propertiesType: typeof(SectionWithGapProperties),customViewName: "~/Components/Section/SectionView/_FiveColumnSection.cshtml")]
[assembly: RegisterSection(identifier: ComponentIdentifiers.Sections.LocalNavigation,name: "Local Navigation", propertiesType: typeof(BaseSectionProperties),customViewName: "~/Components/Section/SectionView/_LocalNavigationSection.cshtml")]


// Register BaseFormSection
[assembly: RegisterFormSection(ComponentIdentifiers.FormSections.TwoColumns, "Two Columns: Even", customViewName: "~/Components/FormSections/Views/_TwoColumnFormSection.cshtml", PropertiesType = typeof(FormSectionWithGapProperties), IconClass = "icon-l-cols-2")]


//custom form component
[assembly: RegisterFormComponent(
    TextBoxReload.IDENTIFIER,
    typeof(TextBoxReload),
    "Text Box Reload",
    Description = "Custom text box with reload support",
    IconClass = "icon-l-text",
    ViewName = "~/Components/FormComponents/TextBoxReload/TextBoxReload.cshtml"
)]

[assembly: RegisterFormComponent(
    ConsentAgreement.IDENTIFIER,
    typeof(ConsentAgreement),
    "BDO Consent Agreement",
    Description = "{$" + ComponentIdentifiers.FormComponents.ConsentAgreement + ".Description$}",
    IconClass = "icon-cb-check-preview",
    ViewName = "~/Components/FormComponents/ConsentAgreement/ConsentAgreement.cshtml"
)]
[assembly: RegisterFormComponent(
    SecurityCode.IDENTIFIER,
    typeof(SecurityCode),
    "Security Code",
    IsAvailableInFormBuilderEditor = true,
    ViewName = "~/Components/FormComponents/SecurityCode/_SecurityCode.cshtml",
    Description = "{$" + ComponentIdentifiers.FormComponents.SecurityCode + ".Description$}",
    IconClass = "icon-cb-check-preview")]

[assembly: RegisterFormComponent(
    MonthYear.IDENTIFIER,
    typeof(MonthYear),
    "Month Year",
    IsAvailableInFormBuilderEditor = true,
    ViewName = "~/Components/FormComponents/MonthYear/_MonthYear.cshtml",
    Description = "{$" + ComponentIdentifiers.FormComponents.MonthYear + ".Description$}",
    IconClass = "icon-cb-check-preview")]

[assembly: RegisterFormComponent(
    ReadOnlyTextComponent.IDENTIFIER,
    typeof(ReadOnlyTextComponent),
    "Read Only Text",
    IsAvailableInFormBuilderEditor = true,
    ViewName = "~/Components/FormComponents/ReadOnlyText/_ReadOnlyTextComponent.cshtml",
    IconClass = "icon-cb-check-preview")]
