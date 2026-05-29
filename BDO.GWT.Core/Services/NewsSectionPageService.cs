using BDO.GWT.Common.Exceptions;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.Helpers;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.Web.Core.Services;
using CMS.Websites;
using Kentico.Content.Web.Mvc;

namespace BDO.GWT.Core.Services;
public interface INewsSectionPageService
{
    Task<NewsSectionDto> GetNewsSectionDtoAsync(SectionNews currentPage, CancellationToken cancellationToken);
    Task<PaginationDto<NewsPageCardDto>> GetNewsArticlePaginationResultAsync(string language, CardsQuery cardsQuery, CancellationToken cancellationToken);
}

public class NewsSectionPageService : INewsSectionPageService
{
    private readonly IFilteringHelper _filteringHelper;
    private readonly IHeroService _heroService;
    private readonly INewsArticleCardsService _newsArticleCardsService;
    private readonly IContentService _contentService;
    private readonly ICultureContext _cultureContext;
    private readonly IContentRetriever _contentRetriever;

    public NewsSectionPageService(
        IFilteringHelper filteringHelper, IHeroService heroService,
       INewsArticleCardsService newsArticleCardsService,IContentService contentService,
       ICultureContext cultureContext, IContentRetriever contentRetriever)
    {
        _filteringHelper = filteringHelper;
        _heroService = heroService;
        _newsArticleCardsService = newsArticleCardsService;
        _contentService = contentService;
        _cultureContext = cultureContext;
        _contentRetriever = contentRetriever;
    }
    public async Task<NewsSectionDto> GetNewsSectionDtoAsync(SectionNews currentPage, CancellationToken cancellationToken)
    {
        var bgImageUrl = currentPage.HeroBackgroundImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url;
        var mobileImageUrl = currentPage.HeroBackgroundImage_Mobile?.FirstOrDefault()?.ContentAttachmentAsset?.Url;
        var ctaUrlPath = currentPage.HeroCallToActionButtonUrl?.FirstOrDefault()?.SystemFields?.WebPageUrlPath;
        var heroData = new HeroData
        {
            Layout = currentPage.HeroLayout,
            Title = currentPage.HeroTitle,
            Description = currentPage.HeroDescription,
            BackgroundImage = bgImageUrl != null ? Link.Parse(bgImageUrl) : null,
            BackgroundImageAltText = currentPage.HeroBackgroundImageAltText,
            MobileBackgroundImage = mobileImageUrl != null ? Link.Parse(mobileImageUrl) : null,
            MobileBackgroundImageAltText = currentPage.HeroBackgroundImage_MobileAltText,
            CallToActionButtonType = currentPage.HeroCallToActionButtonType,
            CallToActionButtonText = currentPage.HeroCallToActionButtonText,
            CallToActionButtonUrl = ctaUrlPath != null ? Link.Parse(ctaUrlPath) : null,
        };
        var dto = new NewsSectionDto
        {
            Title = currentPage.PageTitle,
            Hero = await _heroService.GetHeroDtoAsync(heroData, cancellationToken),
            CardsGrid = await GetNewsArticleCardsGridDtoAsync(currentPage, cancellationToken),
        };

        return dto;
    }
    public async Task<PaginationDto<NewsPageCardDto>> GetNewsArticlePaginationResultAsync(string language, CardsQuery cardsQuery, CancellationToken cancellationToken)
    {
        var featuredNewsArticlePageIds = await GetFeaturedNewsArticlePageIdsAsync(cancellationToken);

        var paginationDto = await _newsArticleCardsService.GetFilteredItemsAsync(
            language,
            cardsQuery,
            null,
            newsArticle => featuredNewsArticlePageIds.Contains(newsArticle.SystemFields.WebPageItemID),
            cancellationToken
            );

        return paginationDto;
    }

    private async Task<ISet<int>> GetFeaturedNewsArticlePageIdsAsync(CancellationToken cancellationToken)
    {
        var newsSection = (await _contentService.GetPagesAsync<SectionNews>(
            new RetrievePagesParameters
            {
                LinkedItemsMaxLevel=2,
                LanguageName = _cultureContext.CurrentLanguageTag,
                UseLanguageFallbacks = false
            },
           query => query
               .OrderBy(nameof(IWebPageContentQueryDataContainer.WebPageItemOrder)),
           RetrievalCacheSettings.CacheDisabled,
           cancellationToken
       )).FirstOrDefault();

        if (newsSection == null)
        {
            throw new MissingSectionPageException($"'No pages have been found of type '{nameof(SectionNews)}'.");
        }
        var featuredNewsArticlePageIds = newsSection.ItemListFeaturedItems.Select(x => x.SystemFields.WebPageItemID).ToHashSet();

        return featuredNewsArticlePageIds;
    }

    private async Task<CardsGridDto> GetNewsArticleCardsGridDtoAsync(SectionNews currentPage, CancellationToken cancellationToken)
    {
        var filters = string.IsNullOrWhiteSpace(currentPage.ItemListFilters)
    ? new HashSet<string>()
    : currentPage.ItemListFilters
        .Split(';', StringSplitOptions.RemoveEmptyEntries)
        .Select(x => x.Trim())
        .ToHashSet();

        var selectedpageid = currentPage.ItemListAdvancedSearchLinkUrl?.FirstOrDefault()?.WebPageGuid;
        var selectedPage = selectedpageid.HasValue && selectedpageid.Value != Guid.Empty
    ? await _contentRetriever.RetrieveAllPagesByGuids<IWebPageFieldsSource>(new[] { selectedpageid.Value }, cancellationToken: cancellationToken)
    : Enumerable.Empty<IWebPageFieldsSource>();

        var newsArticleCardsGridDto = new CardsGridDto
        {
            Title = currentPage.ItemListTitle,
            Description = currentPage.ItemListDescription,
            Filters = currentPage.ItemListFilteringEnabled ? await _newsArticleCardsService.CreateFiltersAsync(filters, null, cancellationToken) : null,
            FilteringTooltipEnabled = await _filteringHelper.IsFilteringTooltipEnabledAsync(currentPage.ItemListFilteringEnabled, cancellationToken),
            ApiEndpointName = "NewsSectionPage",
            PageSize = 20,
            AdvancedSearchPrelinkText = currentPage.ItemListAdvancedSearchDescription,
            AdvancedSearchLinkText = currentPage.ItemListAdvancedSearchLinkText,
            AdvancedSearchLink= selectedPage.FirstOrDefault()?.GetUrl()?.RelativePath ?? string.Empty,//?.ToHtmlCompatibleString(),
        };

        return newsArticleCardsGridDto;
    }
}
