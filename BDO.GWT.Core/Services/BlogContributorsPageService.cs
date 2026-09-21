using BDO.GWT.Common.Exceptions;
using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.Utilities;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.Web.Core.Services;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using Kentico.Content.Web.Mvc.Routing;

namespace BDO.GWT.Core.Services;

public interface IBlogContributorsPageService
{
    Task<BlogContributorsDto> GetSectionPeoplePageDtoAsync(BlogContributors page, CancellationToken cancellationToken);

    Task<PaginationDto<PersonCardDto>> GetBlogContributorsPaginationDtoAsync(Guid pageGuid, string language, CardsQuery cardsQuery, CancellationToken cancellationToken = default);
}

public class BlogContributorsPageService : IBlogContributorsPageService
{
    #region Filtering constants

    private const string OrderFilterName = "Order";
    private const string LastNameAtoZOptionValue = "asc";
    private const string LastNameZtoAOptionValue = "desc";
    private const string NodeOrderOptionValue = "nodeorder";

    #endregion

    private readonly IPeopleCardsService _peopleCardsService;
    private readonly IContentService _contentService;
    private readonly IHeroService _heroService;
    private readonly IWebsiteChannelContext _websiteChannelContext;




    public BlogContributorsPageService(
        IPeopleCardsService peopleCardsService,
        IContentService contentService,
        IHeroService heroService,
        ICultureContext cultureContext,
        IPreferredLanguageRetriever preferredLanguageRetriever,
        IWebsiteChannelContext websiteChannelContext)
    {
        _peopleCardsService = peopleCardsService;
        _contentService = contentService;
        _heroService = heroService;
        _websiteChannelContext = websiteChannelContext;

    }

    public async Task<BlogContributorsDto> GetSectionPeoplePageDtoAsync(BlogContributors page, CancellationToken cancellationToken)
    {
        var options = new[]
        {
            FilterDtoUtility.CreateOptionDto(ResourceStringKey.LiveSite.People.SortLastNameAtoZ, LastNameAtoZOptionValue),
            FilterDtoUtility.CreateOptionDto(ResourceStringKey.LiveSite.People.SortLastNameZtoA, LastNameZtoAOptionValue),
            FilterDtoUtility.CreateOptionDto(ResourceStringKey.LiveSite.People.SortNodeOrder, NodeOrderOptionValue),
        };

     

        var heroData = new HeroData
        {
            Layout = page.HeroLayout,
            Title = page.HeroTitle,
            Description = page.HeroDescription,
            BackgroundImage = page.HeroBackgroundImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url is string bgUrl
                    ? Link.Parse(bgUrl) : null,
            BackgroundImageAltText = page.HeroBackgroundImageAltText,
            MobileBackgroundImage = page.HeroBackgroundImage_Mobile?.FirstOrDefault()?.ContentAttachmentAsset?.Url is string mobileUrl
                    ? Link.Parse(mobileUrl) : null,
            MobileBackgroundImageAltText = page.HeroBackgroundImage_MobileAltText,
            CallToActionButtonType = page.HeroCallToActionButtonType,
            CallToActionButtonText = page.HeroCallToActionButtonText,
            CallToActionButtonUrl = page.HeroCallToActionButtonUrl?.FirstOrDefault()?.SystemFields?.WebPageUrlPath is string ctaPath
                    ? Link.Parse(ctaPath) : null,
        };

        var dto = new BlogContributorsDto
        {
            Hero = await _heroService.GetHeroDtoAsync(heroData, cancellationToken),
            SortBy = FilterDtoUtility.CreateFilterItemDto("select",
                OrderFilterName,
                OrderFilterName,
                page.PeopleListDefaultOrder,
                options),
            PageSize = 20,
            ApiEndpoint = $"BlogContributors/{page.SystemFields.ContentItemGUID}/Get",
        };

        return dto;
    }

    public async Task<PaginationDto<PersonCardDto>> GetBlogContributorsPaginationDtoAsync(Guid pageGuid, string language, CardsQuery cardsQuery, CancellationToken cancellationToken = default)
    {
        var isPreview = true;

        var blogContributorsPage = (await _contentService.GetPagesAsync<BlogContributors>(
           new RetrievePagesParameters
           {
               LanguageName = language,
               UseLanguageFallbacks = isPreview,
               IsForPreview = isPreview,
           },
            query => query
                .OrderBy(nameof(IWebPageContentQueryDataContainer.WebPageItemOrder))
                .Where(w => w
                    .WhereEquals("ContentItemGUID", pageGuid)
                    .Or()
                    .WhereEquals(nameof(IWebPageContentQueryDataContainer.WebPageItemGUID), pageGuid)),
            RetrievalCacheSettings.CacheDisabled,
            cancellationToken
        )).FirstOrDefault();

        if (blogContributorsPage is null)
        {
            throw new MissingSectionPageException($"'{nameof(BlogContributors)} page with GUID '{pageGuid}' not found.");
        }

        var paginationDto = await _peopleCardsService.GetFilteredItemsAsync(
            blogContributorsPage.SystemFields.WebPageItemID,
            language,
            cardsQuery,
            cancellationToken);

        return paginationDto;
    }
}
