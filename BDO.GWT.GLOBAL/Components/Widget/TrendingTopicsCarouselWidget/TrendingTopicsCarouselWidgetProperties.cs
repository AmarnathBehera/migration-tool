using BDO.GWT.Web.Widgets;
using CMS.ContentEngine;
using CMS.DataEngine;
using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Admin.Base.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Core.Components.Widget.TrendingTopicsCarouselWidget;

public class TrendingTopicsCarouselWidgetProperties : BaseWidgetProperties
{
    [TextInputComponent(Label = "Title", ExplanationText = "Title that will be shown over trending topics widget.", Order = 0)]
    public string Title { get; set; }

    [TextInputComponent(Label = "Description", ExplanationText = "Description that will be shown over trending topics widget but under title.", Order = 1)]
    public string Description { get; set; }

    [DropDownComponent(Label = "Topics page type", ExplanationText = "Type of pages that will be shown for this widget.", Order = 2,
        Options = "Blog\r\nIndustry\r\nInsight\r\nMicrosite\r\nService\r\nSpecialty")]
    public string TrendingTopicsPageType { get; set; } = null!;

    [ContentItemSelectorComponent(typeof(TrendingTopicsContentTypeFilter), Label = "Pages", Order = 3)]
    public IEnumerable<ContentItemReference> Pages { get; set; } = Enumerable.Empty<ContentItemReference>();

    [DropDownComponent(Label = "Redirect to Target Page", Order = 4,
        Options = "_self;Same tab\r\n_blank;New tab")]
    public string LearnMoreButtonURLTarget { get; set; } = "_self";

    [DropDownComponent(Label = "Top margin", Order = 5,
        Options = "0px;None\r\n20px\r\n40px\r\n60px\r\n80px")]
    public string TopMargin { get; set; } = "0px";

    [DropDownComponent(Label = "Bottom margin", Order = 6,
        Options = "0px;None\r\n20px\r\n40px\r\n60px\r\n80px")]
    public string BottomMargin { get; set; } = "0px";


#pragma warning disable CA1812
    public class TrendingTopicsContentTypeFilter : IContentTypesFilter
    {
        // Maps dropdown value → CONTENT_TYPE_NAME
        private static readonly Dictionary<string, string> _pageTypeMap = new()
        {
            { "Blog",      SectionBlogs.CONTENT_TYPE_NAME      },
            { "Industry",  SectionIndustries.CONTENT_TYPE_NAME  },
            { "Insight",   SectionInsightsPage.CONTENT_TYPE_NAME },
            { "Microsite", SectionMicrosites.CONTENT_TYPE_NAME  },
            { "Service",   SectionServices.CONTENT_TYPE_NAME    },
            { "Specialty", SectionSpecialties.CONTENT_TYPE_NAME },
        };

        private readonly IFormFieldValueProvider _formFieldValueProvider;

        public TrendingTopicsContentTypeFilter(IFormFieldValueProvider formFieldValueProvider)
        {
            _formFieldValueProvider = formFieldValueProvider;
        }

        public IEnumerable<Guid> AllowedContentTypeIdentifiers
        {
            get
            {
                // Read the sibling dropdown value at runtime
                if (_formFieldValueProvider.TryGet<string>(nameof(TrendingTopicsPageType), out var pageType)
                    && !string.IsNullOrEmpty(pageType)
                    && _pageTypeMap.TryGetValue(pageType, out var contentTypeName))
                {
                    var dataClass = DataClassInfoProviderBase<DataClassInfoProvider>
                        .GetDataClassInfo(contentTypeName);

                    if (dataClass is not null)
                    {
                        return new[] { dataClass.ClassGUID };
                    }
                }

                // Fallback: return all six if dropdown not yet set
                return _pageTypeMap.Values
                    .Select(name => DataClassInfoProviderBase<DataClassInfoProvider>.GetDataClassInfo(name))
                    .Where(info => info is not null)
                    .Select(info => info!.ClassGUID);
            }
        }
    }
#pragma warning restore CA1812
}