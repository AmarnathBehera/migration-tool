//using BDO.GWT.Core.Data;
//using BDO.GWT.Core.Data.Queries;
//using BDO.GWT.Core.DTOs;
//using BDO.GWT.Core.Helper;
//using BDO.GWT.Core.Helpers;
//using BDO.GWT.Core.Services;
//using BDO.GWT.Web.Core.LiveSite.Helpers;
//using BDO.GWT.Web.Core.Services;
//using CMS.Websites;
//using Microsoft.AspNetCore.Mvc;


//namespace BDO.GWT.Web.Core.LiveSite.Services;

//public interface IEventsWebinarsSliderService
//{
//    Task<EventsWebinarsSliderDto> GetEventsWebinarsSliderAsync(
//        IWebPageFieldsSource currentPage,
//        string currentPageContentTypeName,
//        EventsWebinarsSliderData data,
//        CancellationToken cancellationToken);

//    Task<ICollection<EventsWebinarsCardDto>> GetFilteredItemsAsync(
//        StatefulCardsQuery itemsQuery,
//        CancellationToken cancellationToken);
//}

//public class EventsWebinarsSliderService : CarouselServiceBase<CarouselState>, IEventsWebinarsSliderService
//{
//    private readonly ICarouselHelper _carouselHelper;
//    private readonly ICarouselStateHelper<CarouselState> _carouselStateHelper;
//    private readonly IFilteringHelper _filteringHelper;
//    private readonly IPageCardsService<IEventOrWebinar, EventsWebinarsCardDto> _eventWebinarCardsService;

//    public EventsWebinarsSliderService(ICarouselHelper carouselHelper, ICarouselStateHelper<CarouselState> carouselStateHelper, IFilteringHelper filteringHelper, IPageCardsService<IEventOrWebinar, EventsWebinarsCardDto> eventWebinarCardsService) : base(carouselStateHelper)
//    {
//        _carouselHelper = carouselHelper;
//        _carouselStateHelper = carouselStateHelper;
//        _filteringHelper = filteringHelper;
//        _eventWebinarCardsService = eventWebinarCardsService;
//    }

//    public async Task<EventsWebinarsSliderDto> GetEventsWebinarsSliderAsync(
//            IWebPageFieldsSource currentPage,
//            string currentPageContentTypeName,
//            EventsWebinarsSliderData data,
//            CancellationToken cancellationToken)
//    {


//        var carouselState = new CarouselState
//        {
//            CurrentPageTypeName = currentPage.GetType().Name,
//            CurrentPageNodeId = currentPage.SystemFields.ContentItemID,
//            CurrentPageNodeAliasPath = currentPage.SystemFields.WebPageItemTreePath,
//            FeaturedNodeGuids = data.FeaturedEventAndWebinarNodeGuids.ToHashSet(),
//        };

//        var stateGUID = _carouselStateHelper.SetCarouselState(carouselState);

//        var dto = new EventsWebinarsSliderDto
//        {
//            Title = data.Title,
//            Description = data.Description,
//            Filters = data.FilteringEnabled ? await _eventWebinarCardsService.CreateFiltersAsync(data.Filters, null, cancellationToken) : null,
//            FilteringTooltipEnabled = await _filteringHelper.IsFilteringTooltipEnabledAsync(data.FilteringEnabled, cancellationToken),
//            Items = await GetFilteredItemsAsync(new StatefulCardsQuery { State = stateGUID }, cancellationToken),
//            //  BackgroundImageURL = backgroundImagePermanentLink?.ToHtmlCompatibleString(),
//            BackgroundImageAltText = data.BackgroundImageAltText,
//            SeeAllLink = data is { SeeAllLinkText: not null, SeeAllLinkUrl: not null } ? new LinkItemDto
//            {
//                Title = data.SeeAllLinkText,
//                URL = data.SeeAllLinkUrl,
//                Target = data.SeeAllLinkURLTarget
//            } : null,
//            TopMargin = data.TopMargin,
//            BottomMargin = data.BottomMargin,
//            WidgetID = data.WidgetID,
//            StateGUID = stateGUID,
//        };

//        return dto;
//    }

//    public async Task<ICollection<EventsWebinarsCardDto>> GetFilteredItemsAsync(StatefulCardsQuery itemsQuery, CancellationToken cancellationToken)
//    {
//        var carouselState = _carouselStateHelper.GetCarouselState(itemsQuery.State);

//        var nodeID = carouselState.CurrentPageNodeId;

//        var addRelatedContentFilter = carouselState.CurrentPageTypeName switch
//        {
//            nameof(BusinessLine) => query => query.Where(x => x.MetadataBusinessLines.Contains(nodeID)),
//            nameof(ServiceArea) => query => query.Where(x => x.MetadataServiceAreas.Contains(nodeID)),
//            nameof(Service) => query => query.Where(x => x.MetadataServices.Contains(nodeID)),
//            nameof(IndustryCategory) => query => query.Where(x => x.MetadataIndustryCategories.Contains(nodeID)),
//            nameof(Industry) => query => query.Where(x => x.MetadataIndustries.Contains(nodeID)),
//            nameof(SpecialtiesCategory) => query => query.Where(x => x.MetadataSpecialtiesCategories.Contains(nodeID)),
//            nameof(SpecialtiesArea) => query => query.Where(x => x.MetadataSpecialtiesAreas.Contains(nodeID)),
//            nameof(SpecialtiesPage) => query => query.Where(x => x.MetadataSpecialtiesPages.Contains(nodeID)),
//            // nameof(LocationCity) => await _carouselHelper.CreateLocationCityRelatedContentFilterAsync(carouselState.CurrentPageNodeAliasPath, cancellationToken),
//            nameof(LocationOffice) => query => query.Where(x => x.MetadataOfficeLocation.Contains(nodeID)),
//            //  nameof(MicrositeMasterPage) => query => query.Where(x => x.MicrositeMasterPages.Contains(nodeID)),
//            //nameof(MicrositePage) => query => query.Where(x => x.MicrositePages.Contains(nodeID)),
//            _ => null,
//        };

//        itemsQuery.CurrentPage ??= 1;
//        itemsQuery.PageSize ??= addRelatedContentFilter is null ? 10 : 20;

//        var paginationDto = await _eventWebinarCardsService.GetFilteredItemsAsync(
//            itemsQuery,
//            query => addRelatedContentFilter?.Invoke(query),
//            eventOrWebinar => carouselState.FeaturedNodeGuids.Contains(eventOrWebinar.NodeGUID),
//            cancellationToken);

//        return paginationDto.Data;
//    }
//}
