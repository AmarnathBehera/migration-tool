using BDO;
using BDO.GWT.Core;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.ReusableFieldsDTOs;
using CMS.ContentEngine;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;

namespace BDO.GWT.Core;

public interface IHomePageService
{
    Task<HomeDto?> GetHomePageDtoAsync(HomePage? page, CancellationToken cancellationToken = default);
    Task<string?> GetHomePageUrl(CancellationToken cancellationToken);
    
    Task<IEnumerable<HomeHeroSlideDto?>?> RetrieveHomeHeroSlidesDtoAsync(HomePage? homePage, CancellationToken cancellationToken);
}


public class HomePageService : IHomePageService
{

    private readonly IContentService _contentRetriever;
    private readonly IContentQueryExecutor _executor;
    private readonly IWebPageDataContextRetriever _webPageDataContextRetriever;
    private readonly IWebsiteChannelContext _websiteChannelContext;
    public HomePageService(IContentService pageRetriever, IContentQueryExecutor executor,
        IWebPageDataContextRetriever webPageDataContextRetriever, IWebsiteChannelContext websiteChannelContext)
    {
        _contentRetriever = pageRetriever;
        _executor = executor;
        _webPageDataContextRetriever = webPageDataContextRetriever;
        _websiteChannelContext = websiteChannelContext;
    }

    public async Task<HomeDto?> GetHomePageDtoAsync(HomePage? page, CancellationToken cancellationToken = default)
    {

        var webPageContext = _webPageDataContextRetriever.Retrieve();
        var channelName = webPageContext.WebPage.WebsiteChannelName;

        var queryBuilder = new ContentItemQueryBuilder()
            .ForContentType(HomePage.CONTENT_TYPE_NAME, config => config
                .ForWebsite(channelName)          // ← filters by current channel
                .TopN(1));

        var options = new ContentQueryExecutionOptions
        {
            ForPreview = _websiteChannelContext.IsPreview

        };

        var results = await _executor.GetMappedWebPageResult<HomePage>(
            queryBuilder, options, cancellationToken);

        var homePage = results.FirstOrDefault();

        if (homePage is null)
        {
            return null;
        }

        var homeHeroSlides = await RetrieveHomeHeroSlidesDtoAsync(homePage, cancellationToken);
        //var trendingTopics = await RetrieveTrendingTopicsDtoAsync(homePage, cancellationToken);

        return new HomeDto
        {
            HomeHeroSlides = homeHeroSlides,
            //TrendingTopics = trendingTopics
            MetaData = new MetaDataDto
            {
                MetaTitle = homePage.MetadataTitle,
                MetaDescription = homePage.MetadataDescription,
                MetadataTeaserImage = homePage.MetadataTeaserImage,
                MetaTeaserImageAltText = homePage.MetadataTeaserImageAltText,
                MetaCanonical = homePage.MetadataCanonical
            },

            SearchAndSEO = new SearchAndSEODto
            {
                IncludeInSitemap = homePage.IncludeInSitemap,
                MetadataNofollow = homePage.MetadataNofollow,
                MetadataNoindex = homePage.MetadataNoindex
            },

            OGMetaData = new OGMetaDataDto
            {
                MetadataOGTitle = homePage.MetadataOGTitle,
                MetadataOGDescription = homePage.MetadataOGDescription,
                MetadataOGImage = homePage.MetadataOGImage,
                MetadataOGTwitterImage = homePage.MetadataOGTwitterImage,
                MetadataOGTwitterImageSizeCheckbox = homePage.MetadataOGTwitterImageSizeCheckbox
            },

            BaseContent = new BaseContent
            {
                ContentItemID = homePage.SystemFields.ContentItemID,
                ContentItemGUID = homePage.SystemFields.ContentItemGUID,
                ContentItemName = homePage.SystemFields.ContentItemName,
                ContentItemContentTypeID = homePage.SystemFields.ContentItemContentTypeID,
                ContentItemIsSecured = homePage.SystemFields.ContentItemIsSecured,
                //ContentItemCommonDataVersionStatus = homePage.SystemFields.ContentItemCommonDataVersionStatus,
                ContentItemCommonDataContentLanguageID = homePage.SystemFields.ContentItemCommonDataContentLanguageID,
                ContentItemCommonDataFirstPublishedWhen = homePage.SystemFields.ContentItemCommonDataFirstPublishedWhen,
                ContentItemCommonDataLastPublishedWhen = homePage.SystemFields.ContentItemCommonDataLastPublishedWhen
            },
            BaseWebPage = new BaseWebPage
            {
                WebPageItemGUID = homePage.SystemFields.WebPageItemGUID,
                WebPageItemID = homePage.SystemFields.WebPageItemID,
                WebPageItemName = homePage.SystemFields.WebPageItemName,
                WebPageItemWebsiteChannelID = homePage.SystemFields.WebPageItemWebsiteChannelId,
                WebPageUrlPath = homePage.SystemFields.WebPageUrlPath,
                WebPageItemOrder = homePage.SystemFields.WebPageItemOrder,
                WebPageItemTreePath = homePage.SystemFields.WebPageItemTreePath,
                WebPageUrlPathContentLanguageID = homePage.SystemFields.WebPageUrlPathContentLanguageID,
                WebPageItemParentID = homePage.SystemFields.WebPageItemParentID
            }

        };
    }

    public async Task<string?> GetHomePageUrl(CancellationToken cancellationToken)
    {
        var homePage = await _contentRetriever.RetrievePagesAsync<HomePage>(cancellationToken);

        if (homePage == null)
        {
            return null;
        }

        var homePageUrl = homePage.First().GetUrl().AbsoluteUrl;

        return homePageUrl;
    }


    public async Task<IEnumerable<HomeHeroSlideDto?>?> RetrieveHomeHeroSlidesDtoAsync(HomePage homePage, CancellationToken cancellationToken)
    {
        try
        {
            var homeHeroSlides = await _contentRetriever.RetrievePagesAsync<HomeHeroSlide>(new RetrievePagesParameters
            {
                LinkedItemsMaxLevel = 3
            }, cancellationToken);

            if (homeHeroSlides == null || !homeHeroSlides.Any())
            {
                return null;
            }
            var homeHeroSlidesdto = homeHeroSlides.Select(homeHeroSlide => new HomeHeroSlideDto
            {
                Title = homeHeroSlide.Title,
                Description = homeHeroSlide.Description,
                ImageURL = homeHeroSlide.Image_Mobile?.FirstOrDefault()?.ContentAttachmentAsset.Url,
                ImageAltText = homeHeroSlide.ImageAltText,
                MobileImageURL = homeHeroSlide.Image_Mobile?.FirstOrDefault()?.ContentAttachmentAsset.Url,
                MobileImageAltText = homeHeroSlide.Image_MobileAltText,
                ThumbnailImageURL = homeHeroSlide.Image_Thumbnail?.FirstOrDefault()?.ContentAttachmentAsset?.Url,
                ThumbnailImageAltText = homeHeroSlide.Image_ThumbnailAltText,
                Button = homeHeroSlide.CTAButtonText != null && homeHeroSlide.CTAButtonUrl != null ? new LinkItemDto
                {
                    Title = homeHeroSlide.CTAButtonText,
                    URL = homeHeroSlide.CTAButtonUrl,
                } : null,
            }).ToList();


            return homeHeroSlidesdto;
        }
        catch (Exception ex)
        {
            return null;
        }
    }
}

