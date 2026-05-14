using BDO;
using BDO.GWT.GLOBAL.Components.Widget.ImageWithTextWidget;
using BDO.GWT.GLOBAL.Components.Widget.LocalNavigationWidget;
using BDO.GWT.Web.Widgets.AccordionWidget;
using BDO.GWT.Web.Widgets.RichTextWidget;
using BDO.GWT.Xperience.Core.LiveSite.Components.FormComponents;
using BDO.GWT.Xperience.Core.LiveSite.Components.FormSections.FormSectionProperties;
using BDO.GWT.Xperience.Core.LiveSite.Components.Widgets.ButtonWidget;
using BDO.GWT.Xperience.Core.LiveSite.Components.Widgets.ExternalNavigationWidget;
using BDO.GWT.Xperience.Core.LiveSite.Components.Widgets.QuoteCardWidget;
using BDO.GWT.Xperience.Core.LiveSite.Components.Widgets.TiledLinkSectionWidget;
using BDO.GWT.Xperience.Core.LiveSite.Components.Widgets.ViewGlobalLocations;
using BDO.GWT.Xperience.LiveSite.Components.Section;
using Kentico.Forms.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;

// Widgets
[assembly: RegisterWidget(ComponentIdentifiers.Widgets.Imagewithtext,typeof(ImageWithTextWidget),"ImageWithText",typeof(ImageWithTextWidgetProperties),AllowCache = true,IconClass = "icon-xml-tag")]
[assembly: RegisterWidget(ComponentIdentifiers.Widgets.QuoteCard,typeof(QuoteCardWidget),"Quote Card Widget",typeof(QuoteCardWidgetProperties),AllowCache = true,IconClass = "icon-xml-tag")]
[assembly: RegisterWidget(ComponentIdentifiers.Widgets.ExternalNavigationWidget,typeof(ExternalNavigationWidget),"External Navigation Item",typeof(ExternalNavigationWidgetProperties),AllowCache = true,IconClass = "icon-xml-tag")]
[assembly: RegisterWidget(ComponentIdentifiers.Widgets.TiledLink,typeof(TiledLinkSectionWidget),"Tiled Link Widget",typeof(TiledLinkSectionWidgetProperties),AllowCache = true,IconClass = "icon-xml-tag")]
[assembly: RegisterWidget(ComponentIdentifiers.Widgets.Globallocation,typeof(ViewGlobalLocationsWidget),"View Global Location",typeof(ViewGlobalLocationsWidgetProperties),AllowCache = true,IconClass = "icon-xml-tag")]
[assembly: RegisterWidget(ComponentIdentifiers.Widgets.RichText,typeof(RichTextWidget), "Rich Text", typeof(RichTextWidgetProperties),AllowCache = true,IconClass = "icon-xml-tag")]
[assembly: RegisterWidget(ComponentIdentifiers.Widgets.AccordionWidget,typeof(AccordionWidget), "Accordion Widget", typeof(AccordionWidgetProperties),AllowCache = true,IconClass = "icon-xml-tag")]
[assembly: RegisterWidget(ComponentIdentifiers.Widgets.Button, typeof(ButtonWidget), "Button", typeof(ButtonWidgetProperties), AllowCache = true, IconClass = "icon-xml-tag")]
[assembly: RegisterWidget(ComponentIdentifiers.Widgets.LocalNavigation, typeof(LocalNavigationWidget), "Local Navigation Item", typeof(LocalNavigationWidgetProperties), AllowCache = true, IconClass = "icon-xml-tag")]

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
