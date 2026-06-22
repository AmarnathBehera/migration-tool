//using BDO.GWT.Core.Data;
//using BDO.GWT.Core.DTOs;
//using BDO.GWT.Core.DTOs.Cards;
//using BDO.GWT.Web.Core.Services;
//using CMS.Websites;


//namespace BDO.GWT.Web.Core.LiveSite.Services;

//public interface IEditorsPickService 
//{
//    Task<EditorsPickDto> GetEditorsPickDtoAsync(IWebPageFieldsSource currentPage, EditorsPickData data, CancellationToken cancellationToken);

//    Task<ICollection<EditorsPickCardDto>> GetFilteredItemsAsync(StatefulCardsQuery itemsQuery, CancellationToken cancellationToken);
//}

//public class EditorsPickService : CarouselServiceBase<EditorsPickCarouselState>, IEditorsPickService
//{
//    private readonly ICarouselStateHelper<EditorsPickCarouselState> _carouselStateHelper;
//    private readonly IFilteringHelper _filteringHelper;
//    private readonly IMediaFileService _mediaFileService;
//    private readonly IPageCardsService<IEditorsPickPage, EditorsPickCardDto> _editorsPickCardsService;

//    public EditorsPickService(ICarouselStateHelper<EditorsPickCarouselState> carouselStateHelper, IFilteringHelper filteringHelper, IMediaFileService mediaFileService, IPageCardsService<IEditorsPickPage, EditorsPickCardDto> editorsPickCardsService) : base(carouselStateHelper)
//    {
//        _carouselStateHelper = carouselStateHelper;
//        _filteringHelper = filteringHelper;
//        _mediaFileService = mediaFileService;
//        _editorsPickCardsService = editorsPickCardsService;
//    }

//    public async Task<EditorsPickDto> GetEditorsPickDtoAsync(IWebPageFieldsSource currentPage, EditorsPickData data, CancellationToken cancellationToken)
//    {
//        var backgroundImagePermanentLink = await _mediaFileService.GetMediaFilePermanentLinkAsync(data.BackgroundImageMediaFileGuid, cancellationToken);

//        var carouselState = new EditorsPickCarouselState
//        {
//            CurrentPageTypeName = currentPage.GetType().Name,
//            CurrentPageNodeId = currentPage.NodeID,
//            CurrentPageNodeAliasPath = currentPage.NodeAliasPath,
//            FeaturedNodeGuids = data.FeaturedPageNodeGuids.ToHashSet(),
//            PageNodeGuids = data.EditorsPickNodeGuids.ToHashSet(),
//        };

//        var stateGUID = _carouselStateHelper.SetCarouselState(carouselState);

//        var dto = new EditorsPickDto
//        {
//            Title = data.Title,
//            Description = data.Description,
//            Filters = data.FilteringEnabled ? await _editorsPickCardsService.CreateFiltersAsync(data.Filters, null, cancellationToken) : null,
//            FilteringTooltipEnabled = await _filteringHelper.IsFilteringTooltipEnabledAsync(data.FilteringEnabled, cancellationToken),
//            Items = await GetFilteredItemsAsync(new StatefulCardsQuery { State = stateGUID }, cancellationToken),
//            BackgroundImageURL = backgroundImagePermanentLink?.ToHtmlCompatibleString(),
//            BackgroundImageAltText = data.BackgroundImageAltText,
//            SeeAllLink = data is { SeeAllLinkText: not null, SeeAllLinkUrl: not null }
//                ? new LinkItemDto
//                {
//                    Title = data.SeeAllLinkText,
//                    URL = data.SeeAllLinkUrl,
//                    Target= data.SeeAllLinkURLTarget,
//                }
//                : null,
//            TopMargin = data.TopMargin,
//            BottomMargin = data.BottomMargin,
//            WidgetID = data.WidgetID,
//            StateGUID = stateGUID,
//        };

//        return dto;
//    }

//    public async Task<ICollection<EditorsPickCardDto>> GetFilteredItemsAsync(StatefulCardsQuery itemsQuery, CancellationToken cancellationToken)
//    {
//        var carouselState = _carouselStateHelper.GetCarouselState(itemsQuery.State);

//        var featuredPageNodeGuidArray = carouselState.FeaturedNodeGuids.ToArray();
//        var nodeGuids = featuredPageNodeGuidArray.Concat(carouselState.PageNodeGuids).Distinct();

//        var paginationDto = await _editorsPickCardsService.GetFilteredItemsAsync(
//            itemsQuery,
//            query => query.Where(x => nodeGuids.Contains(x.NodeGUID)),
//            page => carouselState.FeaturedNodeGuids.Contains(page.NodeGUID),
//            cancellationToken);

//        return paginationDto.Data;
//    }
//}
