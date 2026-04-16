using BDO;
using BDO.GWT.Xperience.Core.LiveSite.Components.Widgets.ExternalNavigationWidget;
using BDO.GWT.Xperience.Core.LiveSite.Components.Widgets.ImageWithTextWidget;
using BDO.GWT.Xperience.Core.LiveSite.Components.Widgets.QuoteCardWidget;
using BDO.GWT.Web.Widgets.RichTextWidget;
using BDO.GWT.Xperience.Core.LiveSite.Components.Widgets.TiledLinkSectionWidget;
using BDO.GWT.Xperience.Core.LiveSite.Components.Widgets.ViewGlobalLocations;
using BDO.GWT.Xperience.LiveSite.Components.Section;
using Kentico.PageBuilder.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc.PageTemplates;
using BDO.GWT.Web.Widgets.AccordionWidget;

// Widgets
[assembly: RegisterWidget(ComponentIdentifiers.Widgets.Imagewithtext,typeof(ImageWithTextWidget),"ImageWithText",typeof(ImageWithTextWidgetProperties),AllowCache = true,IconClass = "icon-xml-tag")]
[assembly: RegisterWidget(ComponentIdentifiers.Widgets.QuoteCard,typeof(QuoteCardWidget),"Quote Card Widget",typeof(QuoteCardWidgetProperties),AllowCache = true,IconClass = "icon-xml-tag")]
[assembly: RegisterWidget(ComponentIdentifiers.Widgets.ExternalNavigation,typeof(ExternalNavigationWidget),"ExternalNavigationWidget",typeof(ExternalNavigationWidgetProperties),AllowCache = true,IconClass = "icon-xml-tag")]
[assembly: RegisterWidget(ComponentIdentifiers.Widgets.TiledLink,typeof(TiledLinkSectionWidget),"Tiled Link Widget",typeof(TiledLinkSectionWidgetProperties),AllowCache = true,IconClass = "icon-xml-tag")]
[assembly: RegisterWidget(ComponentIdentifiers.Widgets.Globallocation,typeof(ViewGlobalLocationsWidget),"View Global Location",typeof(ViewGlobalLocationsWidgetProperties),AllowCache = true,IconClass = "icon-xml-tag")]
[assembly: RegisterWidget(ComponentIdentifiers.Widgets.RichText,typeof(RichTextWidget), "Rich Text", typeof(RichTextWidgetProperties),AllowCache = true,IconClass = "icon-xml-tag")]
[assembly: RegisterWidget(ComponentIdentifiers.Widgets.AccordionWidget,typeof(AccordionWidget), "Accordion Widget", typeof(AccordionWidgetProperties),AllowCache = true,IconClass = "icon-xml-tag")]

// Sections
[assembly: RegisterSection(identifier: ComponentIdentifiers.Sections.Container,name: "Container",propertiesType: typeof(BaseSectionProperties),customViewName: "~/Components/Section/SectionView/_ContainerSection.cshtml")]
[assembly: RegisterSection(identifier: ComponentIdentifiers.Sections.TwoColumns,name: "Two Columns: Even", propertiesType: typeof(SectionWithGapProperties),customViewName: "~/Components/Section/SectionView/_TwoColumnSection.cshtml")]
[assembly: RegisterSection(identifier: ComponentIdentifiers.Sections.TwoThirdOneThird,name: "Two Columns: 2/3 - 1/3", propertiesType: typeof(SectionWithGapProperties),customViewName: "~/Components/Section/SectionView/_TwoThirdOneThirdSection.cshtml")]
[assembly: RegisterSection(identifier: ComponentIdentifiers.Sections.OneThirdTwoThird,name: "Two Columns: 1/3 - 2/3", propertiesType: typeof(SectionWithGapProperties),customViewName: "~/Components/Section/SectionView/_OneThirdTwoThirdSection.cshtml")]
[assembly: RegisterSection(identifier: ComponentIdentifiers.Sections.ThreeColumns,name: "Three Columns: Even", propertiesType: typeof(SectionWithGapProperties),customViewName: "~/Components/Section/SectionView/_ThreeColumnSection.cshtml")]
[assembly: RegisterSection(identifier: ComponentIdentifiers.Sections.FourColumns,name: "Four Columns: Even", propertiesType: typeof(SectionWithGapProperties),customViewName: "~/Components/Section/SectionView/_FourColumnSection.cshtml")]
[assembly: RegisterSection(identifier: ComponentIdentifiers.Sections.FiveColumns,name: "Five Columns: Even", propertiesType: typeof(SectionWithGapProperties),customViewName: "~/Components/Section/SectionView/_FiveColumnSection.cshtml")]
[assembly: RegisterSection(identifier: ComponentIdentifiers.Sections.LocalNavigation,name: "Local Navigation", propertiesType: typeof(BaseSectionProperties),customViewName: "~/Components/Section/SectionView/_LocalNavigationSection.cshtml")]