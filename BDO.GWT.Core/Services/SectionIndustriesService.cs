using BDO.GWT.Core;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
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

    public SectionIndustriesService(
        IHeroService heroService,
        IContentService contentService)
    {
        _heroService = heroService;
        _contentService = contentService;
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

      
        var industryCategories = await _contentService.GetChildPagesByParentGuidAsync<IndustryCategory>(
            page.SystemFields.WebPageItemID,
            cancellationToken);

        var orderedCategories = industryCategories
            .OrderBy(ic => ic.SystemFields.WebPageItemOrder)
            .ToList();

        var categoryIds = orderedCategories
            .Select(ic => ic.SystemFields.WebPageItemID)
            .ToList();

        var allIndustries = new List<Industry>();

        if (categoryIds.Any())
        {
            var industriesResult = await _contentService.GetPagesAsync<Industry>(
                new RetrievePagesParameters { LinkedItemsMaxLevel = 1 },
                query => query.Where(w => w.WhereIn(
                    nameof(IWebPageContentQueryDataContainer.WebPageItemParentID),
                    categoryIds))
                    .OrderBy(nameof(IWebPageContentQueryDataContainer.WebPageItemOrder)),
                RetrievalCacheSettings.CacheDisabled,
                cancellationToken);

            allIndustries = industriesResult
                .OrderBy(i => i.SystemFields.WebPageItemOrder)
                .ToList();
        }

        var industryUrls = allIndustries
            .Select(industry => (
                ParentID: industry.SystemFields.WebPageItemParentID, 
                Name: industry.MetadataTitle,
                Url: industry.GetUrl().RelativePath
                
            ))
            .ToList();

        var cards = orderedCategories
            .Select(ic =>
            {
                var imageUrl = ic.MetadataTeaserImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url;

                var readMoreLinks = industryUrls
                    .Where(u => u.ParentID == ic.SystemFields.WebPageItemID)
                    .Select(u => new LinkItemDto
                    {
                        Title = u.Name,
                        URL = u.Url
                    })
                    .ToArray();

                return new MultiLinkCardDto
                {
                    Title = ic.MetadataTitle,
                    Description = ic.MetadataDescription,
                    ImageURL = imageUrl,
                    ImageAltText = ic.MetadataTeaserImageAltText,
                    Url = ic.GetUrl().RelativePath,
                    ReadMoreLinks = readMoreLinks,  
                };
            })
            .ToArray();

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
            MultiLinksCardsGrid = new MultiLinkCardsGridDto
            {
                Title = page.GridTitle,
                Description = page.GridSummary,
                Cards = cards,
                IsMultiLinks = page.ShowMultiLinks,
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