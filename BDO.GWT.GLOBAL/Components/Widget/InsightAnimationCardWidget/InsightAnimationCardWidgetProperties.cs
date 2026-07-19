
using BDO.GWT.Web.Widgets;
using CMS.ContentEngine;
using Kentico.Components.Web.Mvc.FormComponents;
using Kentico.Forms.Web.Mvc;
using Kentico.Xperience.Admin.Base.FormAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDO.GWT.Xperience.Core.LiveSite.Components.Widgets.InsightAnimationCardWidget;
public class InsightAnimationCardWidgetProperties : BaseWidgetProperties
{
    [TextInputComponent( Label = "Title", ExplanationText = "Title that will be shown over insight carousel widget.", Order = 0)]
    public string? Title { get; set; }

    [TextInputComponent( Label = "Description", ExplanationText = "Description that will be shown over insight carousel widget but under title.", Order = 1)]
    public string? Description { get; set; }

    [ContentItemSelectorComponent (contentTypeName:nameof(Insight),Label = "Insight Animation Cards", ExplanationText = "Insights that will be added as individual cards.", Order = 2,MaximumItems =0)]
    public IEnumerable<ContentItemReference> InsightCards { get; set; } = Enumerable.Empty<ContentItemReference>();

    [DropDownComponent( Label = "See all link Target", Order = 3,Options = "_self;Same tab\r\n_blank;New tab")]
    public string openInNewTab { get; set; } = "_self";

    [TextInputComponent( Label = "See all link text", Order = 6)]
    public string? SeeAllLinkText { get; set; }

    [EditingComponent(UrlSelector.IDENTIFIER, Label = "See all link URL", Order = 7)]
    public string? SeeAllLinkURL { get; set; }

    [DropDownComponent( Label = "Top margin", Order = 8,Options = "0px;None\r\n20px\r\n40px\r\n60px\r\n80px")]
    public string TopMargin { get; set; } = null!;

    [DropDownComponent( Label = "Bottom margin", Order = 9,Options = "0px;None\r\n20px\r\n40px\r\n60px\r\n80px")]
    public string BottomMargin { get; set; } = null!;


#pragma warning disable CA1812 // Avoid uninstantiated internal classes - this class is instantiated by Kentico
//    private class InsightAnimationsConfiguration : FormComponentConfigurator<PageSelector>
//#pragma warning restore CA1812 // Avoid uninstantiated internal classes - this class is instantiated by Kentico
//    {
//        private readonly IPageRetriever<InsightAnimationCard> _sectionInsightsPageRetriever;

//        public InsightAnimationsConfiguration(IPageRetriever<InsightAnimationCard> sectionInsightsPageRetriever)
//        {
//            _sectionInsightsPageRetriever = sectionInsightsPageRetriever;
//        }

//        public override void Configure(PageSelector formComponent, IFormFieldValueProvider formFieldValueProvider)
//        {
//            ConfigureAsync(formComponent).Wait();
//        }

//        private async Task ConfigureAsync(PageSelector formComponent)
//        {
//            var insightSections = await _sectionInsightsPageRetriever.RetrievePagesAsync(query => query.Select(x => new { x.NodeAliasPath }));
//            var insightsSection = insightSections.FirstOrDefault();

//            if (insightsSection != null)
//            {
//                formComponent.Properties.RootPath = insightsSection.NodeAliasPath;
//                formComponent.Properties.MaxPagesLimit = 0;
//            }
//        }
//    }
}

