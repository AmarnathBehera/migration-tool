//using BDO.GWT.Core.DTOs.Cards;

//using BDO.GWT.Web.Core.Helpers;


//namespace BDO.GWT.Web.Core.Services;

//public interface IEditorsPickCardsService
//{
//    Task<EditorsPickCardDto[]> GetEditorsPickCardDtoAsync(ICollection<IEditorsPickPage> editorsPickPages, Func<IEditorsPickPage, bool> isFeatured, CancellationToken cancellationToken);
//}

//public class EditorsPickCardsService : IEditorsPickCardsService, IPageCardsService<IEditorsPickPage, EditorsPickCardDto>
//{
//    private static readonly Type[] AllowedPageTypes =
//    [
//        typeof(BlogPost),
//        typeof(Event),
//        //typeof(Insight),
//        typeof(MicrositePage),
//        typeof(NewsArticle),
//        typeof(Webinar),
//    ];

//    private readonly ICardsService _cardsService;
//    private readonly IChildPageHelper _childPageHelper;
//    private readonly ICultureContext _cultureContext;
//    private readonly IMultiTypePageRetriever _multiTypePageRetriever;
//    private readonly IPageUrlService _pageUrlService;
//    private readonly IRetriever<ContentType> _contentTypeRetriever;

//    public EditorsPickCardsService(ICardsService cardsService, IChildPageHelper childPageHelper, ICultureContext cultureContext, IMultiTypePageRetriever multiTypePageRetriever, IPageUrlService pageUrlService, IRetriever<ContentType> contentTypeRetriever)
//    {
//        _cardsService = cardsService;
//        _childPageHelper = childPageHelper;
//        _cultureContext = cultureContext;
//        _multiTypePageRetriever = multiTypePageRetriever;
//        _pageUrlService = pageUrlService;
//        _contentTypeRetriever = contentTypeRetriever;
//    }

//    public void AddFilterQueryDecorator<TPageFilter>(Action<IMultiTypePageQuery<Page>> decorateFilterQuery)
//        where TPageFilter : Page
//    {
//        _cardsService.AddFilterQueryDecorator<TPageFilter>(decorateFilterQuery);
//    }

//    public async Task<FilterItemDto<OptionDto>[]> CreateFiltersAsync(HashSet<string> filters, string? nodeAliasPath, CancellationToken cancellationToken)
//    {
//        return await _cardsService.CreateFiltersAsync(filters, nodeAliasPath, cancellationToken);
//    }

//    public async Task<PaginationDto<EditorsPickCardDto>> GetFilteredItemsAsync(CardsQuery cardsQuery, Action<IPageQuery<IEditorsPickPage>>? decorateQuery, Func<IEditorsPickPage, bool> isFeatured, CancellationToken cancellationToken)
//    {
//        var contentPages = await _multiTypePageRetriever.RetrievePagesAsync<IEditorsPickPage>(query =>
//        {
//            query.Types(AllowedPageTypes);
//            ApplyCommonFilter(query);
//            decorateQuery?.Invoke(query.PageQuery);
//            query.Select(x => new
//            {
//                x.ID,
//                x.ContentType,
//                x.CreationDate,
//                x.Image,
//                x.ImageAltText,
//                x.MetadataDescription,
//                x.MetadataTitle,
//                x.NodeGUID,
//                x.NodeID,// Required for URL
//                x.PublishFrom,
//            });
//        }, cancellationToken);

//        var orderedContentPages = OrderingUtility.OrderFeaturablePages(contentPages, isFeatured).ToArray();

//        orderedContentPages = await ApplyCurrentPageFilterAsync(orderedContentPages, cardsQuery.Filters);

//        cardsQuery.CurrentPage ??= 1;
//        cardsQuery.PageSize ??= 20;

//        var paginatedEditorsPickPages = PaginationHelper.PaginatePages(orderedContentPages, cardsQuery)
//            .ToArray();

//        var dtos = await GetEditorsPickCardDtoAsync(paginatedEditorsPickPages, isFeatured, cancellationToken);

//        var paginationDto = new PaginationDto<EditorsPickCardDto>
//        {
//            Data = dtos,
//            TotalCount = orderedContentPages.Length,
//        };

//        return paginationDto;
//    }

//    private async Task<IEditorsPickPage[]> ApplyCurrentPageFilterAsync(IEditorsPickPage[] orderedContentPages, IDictionary<string, string> filters)
//    {
//        var filterWhereExpressions = await GetFilterParameterWhereExpressionsAsync(filters);

//        bool combinedPredicate(IEditorsPickPage x) => x.ShowInWidget && (filterWhereExpressions == null || filterWhereExpressions.All(whereExpr => whereExpr(x)));

//        return orderedContentPages.Where(combinedPredicate).ToArray();
//    }

//    private static void ApplyCommonFilter(IMultiTypePageQuery<IEditorsPickPage> pageQuery)
//    {
//        pageQuery
//            // TODO: Change to page.IsSearchExcluded != true when we implement IS NOT NULL check for DB fields that can be null
//            .Where(x => x.IsSearchExcluded == false || x.IsSearchExcluded == null);
//    }

//    private async Task<List<Func<IEditorsPickPage, bool>>> GetFilterParameterWhereExpressionsAsync(IDictionary<string, string> filterParameterValueByName)
//    {
//        var whereExpressions = new List<Func<IEditorsPickPage, bool>>();

//        foreach (var (filterParameterName, filterParameterValue) in filterParameterValueByName)
//        {
//            var filterParameterData = FilterUtility.GetFilterParameterData(filterParameterName);

//            if (filterParameterData is null)
//            {
//                continue;
//            }

//            if (filterParameterData.FilterParameterType == FilterParameterType.Page)
//            {
//                var page = await _multiTypePageRetriever.RetrievePageAsync(query => query
//                    .Types(filterParameterData.EntityType)
//                    .Where(x => x.Name == filterParameterValue)
//                );

//                if (page is null)
//                {
//                    continue;
//                }

//                var whereExpression = filterParameterName switch
//                {
//                    FilterParameterName.BusinessLine => await BuildBusinessLineWhereExpression(page.NodeAliasPath),
//                    FilterParameterName.ServiceArea => await BuildServiceAreaWhereExpression(page.NodeAliasPath),
//                    FilterParameterName.IndustryCategory => await BuildIndustryCategoryWhereExpression(page.NodeAliasPath),
//                    _ => null,
//                };

//                if (whereExpression is not null)
//                {
//                    whereExpressions.Add(whereExpression);
//                }
//            }
//            else if(filterParameterData.FilterParameterType == FilterParameterType.Entity)
//            {
//                switch (filterParameterName)
//                {
//                    case FilterParameterName.ContentType:
//                    {
//                        var entity = await _contentTypeRetriever.RetrieveEntityAsync(query => query.Where(x => x.CodeName == filterParameterValue));

//                        if (entity is not null)
//                        {
//                            whereExpressions.Add(x => x.ContentType.Contains(entity.ID));
//                        }

//                        break;
//                    }
//                }
//            }
//        }

//        return whereExpressions;
//    }

//    public async Task<EditorsPickCardDto[]> GetEditorsPickCardDtoAsync(ICollection<IEditorsPickPage> editorsPickPages, Func<IEditorsPickPage, bool> isFeatured, CancellationToken cancellationToken)
//    {
//        var cards = new List<EditorsPickCardDto>();

//        var urlByPageNodeId = await _pageUrlService.GetPageRelativeUrlByNodeIdAsync(editorsPickPages.Select(x => x.NodeID), _cultureContext.CurrentLanguageTag, cancellationToken);

//        foreach (var contentPage in editorsPickPages)
//        {
//            var pageType = contentPage.GetType();
//            var card = new EditorsPickCardDto
//            {
//                IsFeatured = isFeatured(contentPage),
//                ImageURL = contentPage.Image?.ToHtmlCompatibleString(),
//                ImageAltText = contentPage.ImageAltText,
//                Type = await GetTypeNameAsync(contentPage, cancellationToken),
//                Title = contentPage.MetadataTitle,
//                DateTextOverride = contentPage.RelatedContentDateOverride,
//                Date = (pageType == typeof(Event) || pageType == typeof(Webinar))
//                    ? contentPage.RelatedContentDate
//                    : (contentPage.PublishFrom ?? contentPage.CreationDate),
//                DateTo = (pageType == typeof(Event) || pageType == typeof(Webinar))
//                    ? contentPage.RelatedContentDateTo
//                    : null,
//                Description = HtmlHelper.ExtractTextFromHtml(contentPage.MetadataDescription),
//                ReadMoreLink = new LinkItemDto
//                {
//                    Title = ResourceStringKey.LiveSite.ReadMore,
//                    URL = urlByPageNodeId[contentPage.NodeID].ToHtmlCompatibleString(),
//                },
//            };

//            cards.Add(card);
//        }

//        return [.. cards];
//    }

//    private async Task<string?> GetTypeNameAsync(IEditorsPickPage contentPage, CancellationToken cancellationToken)
//    {
//        var contentTypeId = contentPage.ContentType.FirstOrDefault();

//        if (contentTypeId == default)
//        {
//            return null;
//        }

//        var insightType = await _contentTypeRetriever.RetrieveEntityAsync(contentTypeId, cancellationToken);

//        return insightType?.Name;
//    }

//    private async Task<Func<IEditorsPickPage, bool>?> BuildBusinessLineWhereExpression(string nodeAliasPathPrefix)
//    {
//        var nodeIdsByPageType = await _childPageHelper.GetDescendantNodeIdsByPageType([
//            typeof(BusinessLine),
//            typeof(ServiceArea),
//            typeof(Service),
//        ], nodeAliasPathPrefix);

//        var businessLineNodeIds = PageTypeUtility.GetPageTypeNodeIds<BusinessLine>(nodeIdsByPageType);
//        var serviceAreaNodeIds = PageTypeUtility.GetPageTypeNodeIds<ServiceArea>(nodeIdsByPageType);
//        var serviceNodeIds = PageTypeUtility.GetPageTypeNodeIds<Service>(nodeIdsByPageType);

//        return x =>
//            x.MetadataBusinessLines.Intersect(businessLineNodeIds).Any() ||
//            x.MetadataServiceAreas.Intersect(serviceAreaNodeIds).Any() ||
//            x.MetadataServices.Intersect(serviceNodeIds).Any();
//    }

//    private async Task<Func<IEditorsPickPage, bool>> BuildServiceAreaWhereExpression(string nodeAliasPathPrefix)
//    {
//        var nodeIdsByPageType = await _childPageHelper.GetDescendantNodeIdsByPageType([
//            typeof(ServiceArea),
//            typeof(Service),
//        ], nodeAliasPathPrefix);

//        var serviceAreaNodeIds = PageTypeUtility.GetPageTypeNodeIds<ServiceArea>(nodeIdsByPageType);
//        var serviceNodeIds = PageTypeUtility.GetPageTypeNodeIds<Service>(nodeIdsByPageType);

//        return x =>
//            x.MetadataServiceAreas.Intersect(serviceAreaNodeIds).Any() ||
//            x.MetadataServices.Intersect(serviceNodeIds).Any();
//    }

//    private async Task<Func<IEditorsPickPage, bool>?> BuildIndustryCategoryWhereExpression(string nodeAliasPathPrefix)
//    {
//        var nodeIdsByPageType = await _childPageHelper.GetDescendantNodeIdsByPageType([
//            typeof(IndustryCategory),
//            typeof(Industry),
//        ], nodeAliasPathPrefix);

//        var industryCategoryNodeIds = PageTypeUtility.GetPageTypeNodeIds<IndustryCategory>(nodeIdsByPageType);
//        var industryNodeIds = PageTypeUtility.GetPageTypeNodeIds<Industry>(nodeIdsByPageType);

//        return x =>
//            x.MetadataIndustryCategories.Intersect(industryCategoryNodeIds).Any() ||
//            x.MetadataIndustries.Intersect(industryNodeIds).Any();
//    }
//}
