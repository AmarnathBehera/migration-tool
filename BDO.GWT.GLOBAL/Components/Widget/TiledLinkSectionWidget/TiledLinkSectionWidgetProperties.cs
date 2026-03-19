//using BDO.GWT.Xperience.Core.LiveSite.Components.VisibilityConditions;
//using Kentico.Components.Web.Mvc.FormComponents;
//using Kentico.Forms.Web.Mvc;
//using Kentico.PageBuilder.Web.Mvc;
//using Microsoft.AspNetCore.Mvc.Formatters;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace BDO.GWT.Xperience.Core.LiveSite.Components.Widgets.TiledLinkSectionWidget;

//public class TiledLinkSectionWidgetProperties : IWidgetProperties
//{
//    [EditingComponent(TextInputComponent.IDENTIFIER, Label = "Title", ExplanationText = "Title that will be shown over tiled link section widget.", Order = 0)]
//    public string? Title { get; set; }

//    [EditingComponent(TextInputComponent.IDENTIFIER, Label = "Description", ExplanationText = "Description that will be shown over tiled link section widget but under title.", Order = 1)]
//    public string? Description { get; set; }

//    [EditingComponent(UrlSelector.IDENTIFIER, Label = "Background image", Order = 2)]
//    [EditingComponentProperty(nameof(UrlSelectorProperties.Tabs), ContentSelectorTabs.Media)]
//    [VisibilityCondition(nameof(MediaType), typeof(ContainsVisibilityCondition))]
//    [VisibilityConditionProperty(nameof(ContainsVisibilityCondition.Values), new[] { "image" })]
//    public string BackgroundImages { get; set; } = "";

   
//    [EditingComponent(TextInputComponent.IDENTIFIER, Label = "Background image alt text", Order = 3)]
//    [VisibilityCondition(nameof(BackgroundImages), ComparisonTypeEnum.IsNotEmpty)]
//    public string BackgroundImageAltText { get; set; } = "";

//    [EditingComponent(DropDownComponent.IDENTIFIER, Label = "Tile page type", ExplanationText = "Type of pages which will be shown as tiles.", Order = 4)]
//    [EditingComponentProperty(nameof(DropDownProperties.DataSource), "Industry\r\nService")]
//    public string TilePageType { get; set; } = null!;

//    [EditingComponent(PageSelector.IDENTIFIER, Label = "Tile pages", ExplanationText = "Pages that will be shown as tiles in the section.", Order = 5)]
//    [EditingComponentConfiguration(typeof(TilePagesConfiguration), nameof(TilePageType))]
//    public IEnumerable<PageSelectorItem> TilePages { get; set; } = Enumerable.Empty<PageSelectorItem>();

//    [EditingComponent(RadioButtonsComponent.IDENTIFIER, Label = "Light / Dark theme", Order = 6)]
//    [EditingComponentProperty(nameof(RadioButtonsProperties.DataSource), "light;Light theme\r\ndark;Dark theme")]
//    public string Theme { get; set; } = "light";

//    [EditingComponent(TextInputComponent.IDENTIFIER, Label = "See all link text", Order = 7)]
//    public string? SeeAllLinkText { get; set; }

//    [EditingComponent(UrlSelector.IDENTIFIER, Label = "See all link URL", Order = 8)]
//    public string? SeeAllLinkURL { get; set; }

//    [EditingComponent(DropDownComponent.IDENTIFIER, Label = "See all link URL target", Order = 9)]
//    [EditingComponentProperty(nameof(DropDownProperties.DataSource), "_self;Same tab\r\n_blank;New tab")]
//    public string SeeAllLinkURLTarget { get; set; } = "_self";

//    [EditingComponent(DropDownComponent.IDENTIFIER, Label = "Top margin", Order = 10)]
//    [EditingComponentProperty(nameof(DropDownProperties.DataSource), "0px;None\r\n20px\r\n40px\r\n60px\r\n80px")]
//    public string TopMargin { get; set; }

//    [EditingComponent(DropDownComponent.IDENTIFIER, Label = "Bottom margin", Order = 11)]
//    [EditingComponentProperty(nameof(DropDownProperties.DataSource), "0px;None\r\n20px\r\n40px\r\n60px\r\n80px")]
//    public string BottomMargin { get; set; }

//#pragma warning disable CA1812 // Avoid uninstantiated internal classes - this class is instantiated by Kentico
//    private class TilePagesConfiguration : FormComponentConfigurator<PageSelector>
//#pragma warning restore CA1812 // Avoid uninstantiated internal classes - this class is instantiated by Kentico
//    {
//        private readonly IPageRetriever<HomePage> _sectionIndustriesPageRetriever;

//        public TilePagesConfiguration(IPageRetriever<SectionIndustries> sectionIndustriesPageRetriever, IPageRetriever<SectionServices> sectionServicesPageRetriever)
//        {
//            _sectionIndustriesPageRetriever = sectionIndustriesPageRetriever;
//            _sectionServicesPageRetriever = sectionServicesPageRetriever;
//        }

//        public override void Configure(PageSelector formComponent, IFormFieldValueProvider formFieldValueProvider)
//        {
//            ConfigureAsync(formComponent, formFieldValueProvider).Wait();
//        }

//        private async Task ConfigureAsync(PageSelector formComponent, IFormFieldValueProvider formFieldValueProvider)
//        {
//            if (!formFieldValueProvider.TryGet<string>(nameof(TilePageType), out var tilePageType))
//            {
//                throw new Exception($"Property '{nameof(TilePageType)}' is not configured.");
//            }

//            formComponent.Properties.MaxPagesLimit = 0;

//            formComponent.Properties.RootPath = tilePageType switch
//            {
//                "Home" => await GetSectionPageNodeAliasPathAsync(_sectionIndustriesPageRetriever),
//                _ => null,
//            };
//        }

//        private static async Task<string?> GetSectionPageNodeAliasPathAsync<TPage>(IPageRetriever<TPage> pageRetriever) where TPage : Page
//        {
//            var pages = await pageRetriever.RetrievePagesAsync();

//            var page = pages.FirstOrDefault();

//            return page?.NodeAliasPath;
//        }
//    }
//}
