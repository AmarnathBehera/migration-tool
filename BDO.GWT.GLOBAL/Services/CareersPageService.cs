using BDO.GWT.Common.Core.Exceptions;
using BDO.GWT.Core;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.Web.Core.DTOs;
using CMS.Websites.Routing;
using CommandLine;
using Kentico.Content.Web.Mvc;
using System;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Core.Services;

public interface ICareersPageService
{
    public Task<CareersPageDto> GetCareersPageDtoAsync(CancellationToken cancellationToken);
    Task<PaginationDto<CareerCardDto>> GetCareerPaginationResultAsync(Guid pageGuid,  CancellationToken cancellationToken);
}
    public class CareersPageService:ICareersPageService
    {

    private readonly IHeroService _heroService;
    private readonly IContentService _contentRetriever;
    private readonly IWebsiteChannelContext _websiteChannelContext;
    private readonly IContentRetriever _retriever;

    public CareersPageService(IHeroService heroService,IContentService contentRetriever, IWebsiteChannelContext websiteChannelContext, IContentRetriever retriever)
    {
        _heroService = heroService;
        _contentRetriever = contentRetriever;
        _websiteChannelContext = websiteChannelContext;
        _retriever = retriever;
    }
    public async Task<CareersPageDto> GetCareersPageDtoAsync(CancellationToken cancellationToken)
    {
        var pages = await _contentRetriever.GetPagesAsync<Careers>(
            new RetrievePagesParameters
            {
                LinkedItemsMaxLevel = 5
            },
            query =>
            {
                query.Where(w =>
                    w.WhereEquals("ContentItemChannelID", _websiteChannelContext.WebsiteChannelID));
            },
            RetrievalCacheSettings.CacheDisabled,
            cancellationToken);

        var careerspage = pages?.FirstOrDefault();

        if (careerspage == null)
        {
            return new CareersPageDto();
        }
        var dto = new CareersPageDto
        {
            Hero = await _heroService.GetHeroDtoAsync(new HeroData
            {
                Layout = careerspage.HeroLayout,
                Title = careerspage.HeroTitle,
                Description = careerspage.HeroDescription,
                BackgroundImage = careerspage.HeroBackgroundImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url is string bgUrl ? Link.Parse(bgUrl) : null,
                BackgroundImageAltText = careerspage.HeroBackgroundImageAltText,
                MobileBackgroundImage = careerspage.HeroBackgroundImage_Mobile?.FirstOrDefault()?.ContentAttachmentAsset?.Url is string mobileUrl ? Link.Parse(mobileUrl) : null,
                MobileBackgroundImageAltText = careerspage.HeroBackgroundImage_MobileAltText,
                CallToActionButtonType = careerspage.HeroCallToActionButtonType,
                CallToActionButtonText = careerspage.HeroCallToActionButtonText,
                CallToActionButtonUrl = careerspage.HeroCallToActionButtonUrl?.FirstOrDefault()?.SystemFields?.WebPageUrlPath is string ctaPath ? Link.Parse(ctaPath) : null,
            }, cancellationToken),
            CardsGrid = await GetCardsGridDtoAsync(careerspage, cancellationToken),
        };

        return dto;
    }

    public async Task<PaginationDto<CareerCardDto>> GetCareerPaginationResultAsync(Guid pageGuid,  CancellationToken cancellationToken)
    {
        var careersPage = await _retriever.RetrievePages<Careers>(
                new RetrievePagesParameters { LinkedItemsMaxLevel = 1 },
            query =>
            {

                query.Where(w => w.WhereEquals("WebPageItemGUID", pageGuid));

            },
                cacheSettings: RetrievalCacheSettings.CacheDisabled,
                cancellationToken: cancellationToken
            );

        var careerPages = await _retriever.RetrievePages<Career>(
                new RetrievePagesParameters { LinkedItemsMaxLevel = 1 },
            query =>
            {

                query.Where(w => w.WhereEquals("WebPageItemParentID", careersPage.FirstOrDefault().SystemFields.WebPageItemID));
                     
            },
                cacheSettings: RetrievalCacheSettings.CacheDisabled, 
                cancellationToken: cancellationToken
            );

        if (careerPages is null)
        {
            throw new MissingSectionPageException($"'{nameof(Career)} page with GUID '{pageGuid}' not found.");
        }
        var itemdata = careerPages.Select(page => new CareerCardDto()
        {
            ImageURL = page.HeroBackgroundImage?.FirstOrDefault()?.ContentAttachmentAsset.Url,
            ImageAltText = page.MetadataTeaserImageAltText,
            Title = page.MetadataTitle,
            JobTitle = page?.MetadataTitle,
            Reference = page.CareerReference,
            //EmploymentType = page.CareerEmploymentType,
            //Level = careerLevelById.TryGetValue(career.CareerLevel, out var careerLevel) ? careerLevel.Name : null,
            //Locations = career.MetadataOfficeLocation.Select(id => locationOfficeNameById.GetValueOrDefault(id)).OfType<string>().ToArray(),
            PublishDate = page.DocumentPublishFrom,
            //ApplyURL = relativeUrlByNodeId[career.NodeID].ToHtmlCompatibleString(),
            //JobTitleURL = parentPageUrl is not null && careerJobTitle is not null ? $"{parentPageUrl}?careerJobTitle={careerJobTitle.CodeName}" : null,
        }).ToArray();

        var paginationDto = new PaginationDto<CareerCardDto>
        {
            Data = itemdata,
            TotalCount = careerPages.Count()
        };

        return paginationDto;
    }


    private async Task<CardsGridDto> GetCardsGridDtoAsync(Careers page, CancellationToken cancellationToken)
    {
        var hashset = page.CareerListFilters;

        //var createFilters = await _careerPageCardsService.CreateFiltersAsync(hashset, null, cancellationToken);

        var insightCardsGridDto = new CardsGridDto
        {
            Title = page.ItemListTitle,
            Description = page.ItemListDescription,
            //Filters = page.ItemListFilteringEnabled ? createFilters : null,
            FilteringTooltipEnabled = true, //for the time being we have hardcoded this.. we have to write a method to get the tooltip form cms
            PageSize = 20,
            ApiEndpointName = $"Careers/{page.SystemFields.WebPageItemGUID}/Get",
            AdvancedSearchPrelinkText = page.ItemListAdvancedSearchDescription,
            AdvancedSearchLinkText = page.ItemListAdvancedSearchLinkText,
            AdvancedSearchLink = page.ItemListAdvancedSearchLinkUrl?.FirstOrDefault()?.ToString()
        };

        return insightCardsGridDto;


    }

}
