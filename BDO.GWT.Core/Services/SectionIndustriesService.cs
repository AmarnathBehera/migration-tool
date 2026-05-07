using BDO.GWT.Core;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.ReusableFieldsDTOs;
using BDO.GWT.Core.services;
using BDO.GWT.Core.ValueObjects;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Core.Services;

public interface ISectionIndustriesService
{
    Task<SectionIndustriesPageDto> GetSectionIndustriesDtoAsync(CancellationToken cancellationToken);
}

public class SectionIndustriesService : ISectionIndustriesService
{
    private readonly IHeroService _heroService;
    private readonly IContentService _contentService;
    private readonly IPageUtility _pageUtility;

    public SectionIndustriesService(
        IHeroService heroService,
        IContentService contentService,
        IPageUtility pageUtility)
    {
        _heroService = heroService;
        _contentService = contentService;
        _pageUtility = pageUtility;
    }

    public async Task<SectionIndustriesPageDto> GetSectionIndustriesDtoAsync(CancellationToken cancellationToken)
    {
        
        var page = await _contentService.RetrieveCurrentPageAsync<SectionIndustries>(
            new RetrieveCurrentPageParameters { LinkedItemsMaxLevel = 4 },
            cancellationToken);

        if (page == null)
        {
            return new SectionIndustriesPageDto();
        }

      
        //var industryCategories = await _contentService.GetChildPagesByParentGuidAsync<IndustryCategory>(
        //    page.SystemFields.WebPageItemID,
        //    cancellationToken);

        //var orderedCategories = industryCategories
        //    .OrderBy(ic => ic.SystemFields.WebPageItemOrder)
        //    .ToList();

        //var categoryIds = orderedCategories
        //    .Select(ic => ic.SystemFields.WebPageItemID)
        //    .ToList();

        //var allIndustries = new List<Industry>();

        //if (categoryIds.Any())
        //{
        //    var industriesResult = await _contentService.GetPagesAsync<Industry>(
        //        new RetrievePagesParameters { LinkedItemsMaxLevel = 1 },
        //        query => query.Where(w => w.WhereIn(
        //            nameof(IWebPageContentQueryDataContainer.WebPageItemParentID),
        //            categoryIds))
        //            .OrderBy(nameof(IWebPageContentQueryDataContainer.WebPageItemOrder)),
        //        RetrievalCacheSettings.CacheDisabled,
        //        cancellationToken);

        //    allIndustries = industriesResult
        //        .OrderBy(i => i.SystemFields.WebPageItemOrder)
        //        .ToList();
        //}

        //var industryUrls = allIndustries
        //    .Select(industry => (
        //        ParentID: industry.SystemFields.WebPageItemParentID, 
        //        Name: industry.MetadataTitle,
        //        Url: industry.SystemFields.WebPageUrlPath

        //    ))
        //    .ToList();

        //var cards = orderedCategories
        //    .Select(ic =>
        //    {
        //        var imageUrl = ic.MetadataTeaserImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url;

        //        var readMoreLinks = industryUrls
        //            .Where(u => u.ParentID == ic.SystemFields.WebPageItemID)
        //            .Select(u => new LinkItemDto
        //            {
        //                Title = u.Name,
        //                URL = u.Url
        //            })
        //            .ToArray();

        //        return new MultiLinkCardDto
        //        {
        //            Title = ic.MetadataTitle,
        //            Description = ic.MetadataDescription,
        //            ImageURL = imageUrl,
        //            ImageAltText = ic.MetadataTeaserImageAltText,
        //            Url = ic.SystemFields?.WebPageUrlPath,
        //            ReadMoreLinks = readMoreLinks,  
        //        };
        //    })
        //    .ToArray();

        var cards = await _pageUtility.GetMultiLinkCardsAsync<IndustryCategory, Industry>(
            parentWebPageItemId: page.SystemFields.WebPageItemID,
            cardTitleSelector: ic => ic.MetadataTitle,
            cardDescriptionSelector: ic => ic.MetadataDescription,
            cardImageUrlSelector: ic => ic.MetadataTeaserImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url,
            cardImageAltTextSelector: ic => ic.MetadataTeaserImageAltText,
            childTitleSelector: industry => industry.MetadataTitle,
            cancellationToken: cancellationToken);



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

        return new SectionIndustriesPageDto
        {
            Hero = await _heroService.GetHeroDtoAsync(heroData, cancellationToken),
            MetaData = new MetaDataDto
            {
                MetaTitle = page.MetadataTitle,
                MetaDescription = page.MetadataDescription,
                MetadataTeaserImage = page.MetadataTeaserImage,
                MetaTeaserImageAltText = page.MetadataTeaserImageAltText,
                MetaCanonical = page.MetadataCanonical
            },
            SearchAndSEO = new SearchAndSEODto
            {
                IncludeInSitemap = page.IncludeInSitemap,
                MetadataNofollow = page.MetadataNofollow,
                MetadataNoindex = page.MetadataNoindex
            },
            OGMetaData = new OGMetaDataDto
            {
                MetadataOGTitle = page.MetadataOGTitle,
                MetadataOGDescription = page.MetadataOGDescription,
                MetadataOGImage = page.MetadataOGImage,
                MetadataOGTwitterImage = page.MetadataOGTwitterImage,
                MetadataOGTwitterImageSizeCheckbox = page.MetadataOGTwitterImageSizeCheckbox
            },
            MultiLinksCardsGrid = new MultiLinkCardsGridDto
            {
                Title = page.GridTitle,
                Description = page.GridSummary,
                Cards = cards,
                IsMultiLinks = page.ShowMultiLinks,
            },
            BaseContent = new BaseContent
            {
                ContentItemID = page?.SystemFields?.ContentItemID ?? 0,
                ContentItemGUID = page?.SystemFields?.ContentItemGUID ?? Guid.Empty,
                ContentItemName = page?.SystemFields?.ContentItemName,
                ContentItemContentTypeID = page?.SystemFields?.ContentItemContentTypeID ?? 0,
                ContentItemIsSecured = page?.SystemFields?.ContentItemIsSecured ?? false,
                ContentItemCommonDataContentLanguageID = page?.SystemFields?.ContentItemCommonDataContentLanguageID ?? 0,
                ContentItemCommonDataFirstPublishedWhen = page?.SystemFields?.ContentItemCommonDataFirstPublishedWhen ?? default,
                ContentItemCommonDataLastPublishedWhen = page?.SystemFields?.ContentItemCommonDataLastPublishedWhen ?? default,

            },
            BaseWebPage = new BaseWebPage
            {
                WebPageItemGUID = page?.SystemFields?.WebPageItemGUID ?? Guid.Empty,
                WebPageItemID = page?.SystemFields?.WebPageItemID ?? 0,
                WebPageItemName = page?.SystemFields?.WebPageItemName,
                WebPageItemWebsiteChannelID = page?.SystemFields?.WebPageItemWebsiteChannelId ?? 0,
                WebPageUrlPath = page?.SystemFields?.WebPageUrlPath,
                WebPageItemOrder = page?.SystemFields?.WebPageItemOrder ?? 0,
                WebPageItemTreePath = page?.SystemFields?.WebPageItemTreePath,
                WebPageUrlPathContentLanguageID = page?.SystemFields?.WebPageUrlPathContentLanguageID ?? 0,
                WebPageItemParentID = page?.SystemFields?.WebPageItemParentID ?? 0
            },
        };
    }
}











//using BDO.GWT.Core;
//using BDO.GWT.Core.Data;
//using BDO.GWT.Core.DTOs;
//using BDO.GWT.Core.ValueObjects;
//using CMS.Websites;
//using Kentico.Content.Web.Mvc;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;

//namespace BDO.GWT.Web.Core.Services;

//public interface ISectionIndustriesService
//{
//    Task<MultiLinkPageDto> GetSectionIndustriesDtoAsync(CancellationToken cancellationToken);
//}

//public class SectionIndustriesService : ISectionIndustriesService
//{
//    private readonly IHeroService _heroService;
//    private readonly IContentService _contentService;


//    public SectionIndustriesService(
//        IHeroService heroService,
//        IContentService contentService)
//    {
//        _heroService = heroService;
//        _contentService = contentService;
//    }

//    public async Task<MultiLinkPageDto> GetSectionIndustriesDtoAsync(CancellationToken cancellationToken)
//    {

//        var page = await _contentService.RetrieveCurrentPageAsync<SectionIndustries>(
//            new RetrieveCurrentPageParameters { LinkedItemsMaxLevel = 4 },
//            cancellationToken);

//        if (page == null)
//        {
//            return new MultiLinkPageDto();
//        }


//        var industryCategories = await _contentService.GetChildPagesByParentGuidAsync<IndustryCategory>(
//            page.SystemFields.WebPageItemID,
//            cancellationToken);

//        var orderedCategories = industryCategories
//            .OrderBy(ic => ic.SystemFields.WebPageItemOrder)
//            .ToList();


//        var industries = await _contentService.GetChildPagesByParentGuidAsync<Industry>(
//            page.SystemFields.WebPageItemID,
//            cancellationToken);

//        var orderedIndustries = industries
//            .OrderBy(i => i.SystemFields.WebPageItemOrder)
//            .ToList();

//        var industryUrls = orderedIndustries
//            .Select(industry => (
//                ParentID: industry.SystemFields.WebPageItemParentID,
//                Name: industry.SystemFields.WebPageItemName,
//                Url: industry.GetUrl().RelativePath
//            ))
//            .ToList();


//        var cards = orderedCategories
//            .Select(ic =>
//            {
//                var imageUrl = ic.MetadataTeaserImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url;

//                var readMoreLinks = industryUrls
//                    .Where(u => u.ParentID == ic.SystemFields.WebPageItemID)
//                    .Select(u => new LinkItemDto
//                    {
//                        Title = u.Name,
//                        URL = u.Url
//                    })
//                    .ToArray();

//                return new MultiLinkCardDto
//                {
//                    Title = ic.MetadataTitle,
//                    Description = ic.MetadataDescription,
//                    ImageURL = imageUrl,
//                    ImageAltText = ic.MetadataTeaserImageAltText,
//                    Url = ic.GetUrl().RelativePath,
//                    ReadMoreLinks = readMoreLinks,
//                };
//            })
//            .ToArray();

//        var bgImageUrl = page.HeroBackgroundImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url;
//        var mobileImageUrl = page.HeroBackgroundImage_Mobile?.FirstOrDefault()?.ContentAttachmentAsset?.Url;
//        var ctaUrlPath = page.HeroCallToActionButtonUrl?.FirstOrDefault()?.SystemFields?.WebPageUrlPath;

//        var heroData = new HeroData
//        {
//            Layout = page.HeroLayout,
//            Title = page.HeroTitle,
//            Description = page.HeroDescription,
//            BackgroundImage = bgImageUrl != null ? Link.Parse(bgImageUrl) : null,
//            BackgroundImageAltText = page.HeroBackgroundImageAltText,
//            MobileBackgroundImage = mobileImageUrl != null ? Link.Parse(mobileImageUrl) : null,
//            MobileBackgroundImageAltText = page.HeroBackgroundImage_MobileAltText,
//            CallToActionButtonType = page.HeroCallToActionButtonType,
//            CallToActionButtonText = page.HeroCallToActionButtonText,
//            CallToActionButtonUrl = ctaUrlPath != null ? Link.Parse(ctaUrlPath) : null,
//        };

//        return new MultiLinkPageDto
//        {
//            Hero = await _heroService.GetHeroDtoAsync(heroData, cancellationToken),
//            MultiLinksCardsGrid = new MultiLinkCardsGridDto
//            {
//                Title = page.GridTitle,
//                Description = page.GridSummary,
//                Cards = cards,
//                IsMultiLinks = page.ShowMultiLinks,
//            },
//        };
//    }
//}