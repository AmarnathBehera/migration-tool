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

public interface ISectionServicesService
{
    Task<SectionServicesPageDto> GetSectionServicesDtoAsync(CancellationToken cancellationToken);
}

public class SectionServicesService : ISectionServicesService
{
    private readonly IHeroService _heroService;
    private readonly IContentService _contentService;

    public SectionServicesService(
        IHeroService heroService,
        IContentService contentService)
    {
        _heroService = heroService;
        _contentService = contentService;
    }

    public async Task<SectionServicesPageDto> GetSectionServicesDtoAsync(CancellationToken cancellationToken)
    {
        
        var page = await _contentService.RetrieveCurrentPageAsync<SectionServices>(
            new RetrieveCurrentPageParameters { LinkedItemsMaxLevel = 4 },
            cancellationToken);

        if (page == null)
        {
            return new SectionServicesPageDto();
        }

      
        //var serviceCategories = await _contentService.GetChildPagesByParentGuidAsync<ServiceCategory>(
        //    page.SystemFields.WebPageItemID,
        //    cancellationToken);

        //var orderedCategories = serviceCategories
        //    .OrderBy(ic => ic.SystemFields.WebPageItemOrder)
        //    .ToList();

        //var categoryIds = orderedCategories
        //    .Select(ic => ic.SystemFields.WebPageItemID)
        //    .ToList();

        //var allServices = new List<Service>();

        //if (categoryIds.Any())
        //{
        //    var servicesResult = await _contentService.GetPagesAsync<Service>(
        //        new RetrievePagesParameters { LinkedItemsMaxLevel = 1 },
        //        query => query.Where(w => w.WhereIn(
        //            nameof(IWebPageContentQueryDataContainer.WebPageItemParentID),
        //            categoryIds))
        //            .OrderBy(nameof(IWebPageContentQueryDataContainer.WebPageItemOrder)),
        //        RetrievalCacheSettings.CacheDisabled,
        //        cancellationToken);

        //    allServices = servicesResult
        //        .OrderBy(i => i.SystemFields.WebPageItemOrder)
        //        .ToList();
        //}

        //var serviceUrls = allServices
        //    .Select(service => (
        //        ParentID: service.SystemFields.WebPageItemParentID, 
        //        Name: service.MetadataTitle,
        //        Url: service.GetUrl().RelativePath
                
        //    ))
        //    .ToList();

        //var cards = orderedCategories
        //    .Select(ic =>
        //    {
        //        var imageUrl = ic.MetadataTeaserImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url;

        //        var readMoreLinks = serviceUrls
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
        //            Url = ic.GetUrl().RelativePath,
        //            ReadMoreLinks = readMoreLinks,  
        //        };
        //    })
        //    .ToArray();

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

        return new SectionServicesPageDto
        {
            Hero = await _heroService.GetHeroDtoAsync(heroData, cancellationToken),
            MultiLinksCardsGrid = new MultiLinkCardsGridDto
            {
                Title = page.GridTitle,
                Description = page.GridSummary,
                //Cards = cards,
                IsMultiLinks = page.ShowMultiLinks,
            },
        };
    }
}