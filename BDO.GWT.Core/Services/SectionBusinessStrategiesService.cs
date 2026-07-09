using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.Utilities;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.Web.Core.Services;
using Kentico.Content.Web.Mvc;

namespace BDO.GWT.Core.Services;

public interface ISectionBusinessStrategiesService
{
    Task<SectionBusinessStrategiesDto> GetBusinessStrategiesDtoAsync(SectionBusinessStrategies page, CancellationToken cancellationToken);
}

public class SectionBusinessStrategiesService : ISectionBusinessStrategiesService
{

    private readonly IHeroService _heroService;
    private readonly IContentService _contentService;
    private readonly IWebPageUrlResolver _webPageUrlService;
    private readonly ICultureContext _cultureContext;

    public SectionBusinessStrategiesService(IHeroService heroService, IContentService contentService, IWebPageUrlResolver webPageUrlService, ICultureContext cultureContext)
    {
        _heroService = heroService;
        _contentService = contentService;
        _webPageUrlService = webPageUrlService;
        _cultureContext = cultureContext;
    }

    public async Task<SectionBusinessStrategiesDto> GetBusinessStrategiesDtoAsync(SectionBusinessStrategies page, CancellationToken cancellationToken)
    {
        var bgImageUrl = page.HeroBackgroundImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url;
        var mobileImageUrl = page.HeroBackgroundImage_Mobile?.FirstOrDefault()?.ContentAttachmentAsset?.Url;
        var ctaUrlPath = page.HeroCallToActionButtonUrl?.FirstOrDefault()?.SystemFields?.WebPageUrlPath;

        var heroData = new HeroData
        {
            Layout = page.HeroLayout,
            Title = page.HeroTitle,
            Description = page.HeroDescription,
            BackgroundImage = bgImageUrl != null ? Link.Parse(bgImageUrl) : null,
            BackgroundImageAltText = page.HeroBackgroundImageAltText,
            MobileBackgroundImage = mobileImageUrl != null ? Link.Parse(mobileImageUrl) : null,
            MobileBackgroundImageAltText = page.HeroBackgroundImage_MobileAltText,
            CallToActionButtonType = page.HeroCallToActionButtonType,
            CallToActionButtonText = page.HeroCallToActionButtonText,
            CallToActionButtonUrl = ctaUrlPath != null ? Link.Parse(ctaUrlPath) : null,
        };

        return new SectionBusinessStrategiesDto
        {
            Hero = await _heroService.GetHeroDtoAsync(heroData, cancellationToken),
            BackgroundImageURL = page.MenuBackgroundImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url,
            BackgroundImageAltText = page.MenuBackgroundImageAltText,
            BusinessStrategiesMenu = await GetBusinessStrategiesMenuAsync(page, cancellationToken),
        };
    }

    private async Task<List<BusinessStrategiesMenuDto>> GetBusinessStrategiesMenuAsync(SectionBusinessStrategies page, CancellationToken cancellationToken)
    {
        var businessStrategiesCategory = (await _contentService.GetPagesAsync<BusinessStrategiesCategory>(
            new RetrievePagesParameters
            {
                LanguageName = _cultureContext.CurrentLanguageTag,
                UseLanguageFallbacks = false
            },
           query => query
               .Where(w => w.WhereEquals("WebPageItemParentID", page.SystemFields.WebPageItemID)),
           RetrievalCacheSettings.CacheDisabled,
           cancellationToken
       ));
        List<int> businessStrategiesCategoryGuids = businessStrategiesCategory.Select(x => x.SystemFields.WebPageItemID).ToList();
        businessStrategiesCategoryGuids.Add(page.SystemFields.WebPageItemID);

        var businessStrategiesDetailsPages = (await _contentService.GetPagesAsync<BusinessStrategy>(
            new RetrievePagesParameters
            {
                LanguageName = _cultureContext.CurrentLanguageTag,
                UseLanguageFallbacks = false
            },
           query => query
               .Where(w => w.WhereEquals(nameof(BusinessStrategy.MenuShowInSection), true))
               .TopN(8)
               .OrderBy(nameof(BusinessStrategy.SystemFields.ContentItemCommonDataLastPublishedWhen))
               .Where(w => w.WhereIn("WebPageItemParentID", businessStrategiesCategoryGuids)),
           RetrievalCacheSettings.CacheDisabled,
           cancellationToken
       ));

        if (!businessStrategiesDetailsPages.Any())
        {
            return new List<BusinessStrategiesMenuDto>();
        }

        var businessStrategiesDetailsDtos = businessStrategiesDetailsPages.Select(businessStrategiesDetailsPage => new BusinessStrategiesMenuDto
        {
            ImageURL = businessStrategiesDetailsPage.MenuTileIcon?.FirstOrDefault()?.IconAttachmentAsset.Url,
            Title = businessStrategiesDetailsPage.MetadataTitle,
            Description = HtmlHelper.ExtractTextFromHtml(businessStrategiesDetailsPage.MetadataDescription),
            ReadMoreLink = new LinkItemDto
            {
                Title = businessStrategiesDetailsPage.MenuReadMoreText ?? "",
                URL= _webPageUrlService.GetRelativePath(businessStrategiesDetailsPage),
            },
        }).ToList();

        return businessStrategiesDetailsDtos;
    }
}