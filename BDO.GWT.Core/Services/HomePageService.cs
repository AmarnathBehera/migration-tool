using BDO;
using BDO.GWT.Core;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.ReusableFieldsDTOs;
using BDO.GWT.Core.Utilities;
using CMS.ContentEngine;
using CMS.Helpers;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;

namespace BDO.GWT.Core;

public interface IHomePageService
{
    Task<HomeDto?> GetHomePageDtoAsync(HomePage? page, CancellationToken cancellationToken = default);
    Task<string?> GetHomePageUrl(CancellationToken cancellationToken);

    Task<IEnumerable<HomeHeroSlideDto?>?> RetrieveHomeHeroSlidesDtoAsync(HomePage? homePage, CancellationToken cancellationToken);

    Task<TrendingTopicsDto?> RetrieveTrendingTopicsDtoAsync(CancellationToken cancellationToken = default);


}

public class HomePageService : IHomePageService
{

    private readonly IContentService _contentRetriever;
    //private readonly IContentQueryExecutor _executor;
    //private readonly IWebPageDataContextRetriever _webPageDataContextRetriever;
    private readonly IWebsiteChannelContext _websiteChannelContext;
    public HomePageService(IContentService pageRetriever, 
        //IContentQueryExecutor executor,
        //IWebPageDataContextRetriever webPageDataContextRetriever, 
        IWebsiteChannelContext websiteChannelContext)
    {
        _contentRetriever = pageRetriever;
        //_executor = executor;
        //_webPageDataContextRetriever = webPageDataContextRetriever;
        _websiteChannelContext = websiteChannelContext;
    }

    public async Task<HomeDto?> GetHomePageDtoAsync(HomePage? page, CancellationToken cancellationToken = default)
    {
       

        var results = await _contentRetriever.GetPagesAsync<HomePage>(
     new RetrievePagesParameters
     {
         LinkedItemsMaxLevel = 1
     },
     query =>
     {
         query.Where(w =>
             w.WhereEquals("ContentItemChannelID", _websiteChannelContext.WebsiteChannelID));
     },
     RetrievalCacheSettings.CacheDisabled,
     cancellationToken);

        var homePage = results.FirstOrDefault();

        if (homePage is null)
        {
            return null;
        }

        var homeHeroSlides = await RetrieveHomeHeroSlidesDtoAsync(homePage, cancellationToken);
        var trendingTopics = await RetrieveTrendingTopicsDtoAsync(cancellationToken);

        return new HomeDto
        {
            HomeHeroSlides = homeHeroSlides,
            TrendingTopics = trendingTopics,
           
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

    public async Task<TrendingTopicsDto?> RetrieveTrendingTopicsDtoAsync(CancellationToken cancellationToken = default)
    {
        var trendingTopicsPage = await _contentRetriever.RetrievePagesAsync<TrendingTopics>(cancellationToken);

        if (trendingTopicsPage is null)
        {
            return null;
        }

        var trendingTopicPagesResult = await _contentRetriever.RetrievePagesAsync<TrendingTopic>(new RetrievePagesParameters
        {
            LinkedItemsMaxLevel = 3
        },
         cancellationToken);

        if (trendingTopicPagesResult is null)
        {
            return null;
        }

        var trendingTopicPages = trendingTopicPagesResult.ToArray();

        var trendingTopicTabDtos = await Task.WhenAll(trendingTopicPages.Select(async x => new TrendingTopicTabDto
          {
                Title = x?.Title,
                ReadMoreLink = x.CTANodeGuid?.FirstOrDefault()?.SystemFields?.WebPageUrlPath,
                ImageURL = x.ImageUrl.FirstOrDefault()?.ContentAttachmentAsset.Url,
                ImageAltText = x.ImageAltText,
                Items = x.FeaturedItems.Select(item => item switch
                    {
                        IndustryService industryService => new TrendingTopicItemDto
                    {
                        Title = industryService.MetadataTitle,
                        Description = HtmlHelper.ExtractTextFromHtml(industryService.MetadataDescription),
                            ReadMoreLink = industryService.SystemFields?.WebPageUrlPath,


                    },

                    Industry industry => new TrendingTopicItemDto
                    {
                        Title = industry.MetadataTitle,
                        Description = HtmlHelper.ExtractTextFromHtml(industry.MetadataDescription),
                        ReadMoreLink = industry.SystemFields?.WebPageUrlPath
                    },
                        Service service => new TrendingTopicItemDto
                        {
                            Title = service.MetadataTitle,
                            Description = HtmlHelper.ExtractTextFromHtml(service.MetadataDescription),
                            ReadMoreLink = service.SystemFields?.WebPageUrlPath
                        },
                        ServiceArea serviceArea => new TrendingTopicItemDto
                        {
                            Title = serviceArea.MetadataTitle,
                            Description = HtmlHelper.ExtractTextFromHtml(serviceArea.MetadataDescription),
                            ReadMoreLink = serviceArea.SystemFields?.WebPageUrlPath
                        },
                        Sub_Service sub_Service => new TrendingTopicItemDto
                        {
                            Title = sub_Service.MetadataTitle,
                            Description = HtmlHelper.ExtractTextFromHtml(sub_Service.MetadataDescription),
                            ReadMoreLink = sub_Service.SystemFields?.WebPageUrlPath
                        },
                        _ => new TrendingTopicItemDto
                    {
                       Title = item.SystemFields?.WebPageItemName,
                       ReadMoreLink = item.SystemFields?.WebPageUrlPath
                    }



                 }).ToList(),


              }));

          var dto = new TrendingTopicsDto
          {
             Heading = trendingTopicsPage.FirstOrDefault()?.Heading,
             Tabs = trendingTopicTabDtos,
          };

          return dto;
    }
}

