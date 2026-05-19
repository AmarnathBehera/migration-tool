using BDO.GWT.Common.Core.Exceptions;
using BDO.GWT.Core;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.Web.Core.DTOs;
using CMS.ContentEngine;
using CMS.Websites;
using CMS.Websites.Routing;
using CommandLine;
using Kentico.Content.Web.Mvc;
using System;
using System.Linq;
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
    private readonly ITaxonomyRetriever _taxonomyRetriever;
    public CareersPageService(IHeroService heroService,IContentService contentRetriever, IWebsiteChannelContext websiteChannelContext, IContentRetriever retriever,ITaxonomyRetriever taxonomyRetriever)
    {
        _heroService = heroService;
        _contentRetriever = contentRetriever;
        _websiteChannelContext = websiteChannelContext;
        _retriever = retriever;
        _taxonomyRetriever = taxonomyRetriever;
    }
    public async Task<CareersPageDto> GetCareersPageDtoAsync(CancellationToken cancellationToken)
    {
        var page = await _contentRetriever.RetrieveCurrentPageAsync<Careers>(
            new RetrieveCurrentPageParameters { }, cancellationToken
            );

        

        if (page == null)
        {
            return new CareersPageDto();
        }
        var dto = new CareersPageDto
        {
            Hero = await _heroService.GetHeroDtoAsync(new HeroData
            {
                Layout = page.HeroLayout,
                Title = page.HeroTitle,
                Description = page.HeroDescription,
                BackgroundImage = page.HeroBackgroundImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url is string bgUrl ? Link.Parse(bgUrl) : null,
                BackgroundImageAltText = page.HeroBackgroundImageAltText,
                MobileBackgroundImage = page.HeroBackgroundImage_Mobile?.FirstOrDefault()?.ContentAttachmentAsset?.Url is string mobileUrl ? Link.Parse(mobileUrl) : null,
                MobileBackgroundImageAltText = page.HeroBackgroundImage_MobileAltText,
                CallToActionButtonType = page.HeroCallToActionButtonType,
                CallToActionButtonText = page.HeroCallToActionButtonText,
                CallToActionButtonUrl = page.HeroCallToActionButtonUrl?.FirstOrDefault()?.SystemFields?.WebPageUrlPath is string ctaPath ? Link.Parse(ctaPath) : null,
            }, cancellationToken),
            CardsGrid = await GetCardsGridDtoAsync(page, cancellationToken),
        };

        return dto;
    }

    public async Task<PaginationDto<CareerCardDto>> GetCareerPaginationResultAsync(Guid pageGuid,  CancellationToken cancellationToken)
    {
        var careersPage = await _retriever.RetrievePages<Careers>(
                new RetrievePagesParameters { LinkedItemsMaxLevel = 2 },
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



        var itemdata = await Task.WhenAll(
    careerPages.Select(async page =>
    {
        var employmentTypeIds = page.CareerEmploymentType?
            .Select(i => i.Identifier)
            .ToList();

        var careerLevelIds = page.CareerLevel?
            .Select(i => i.Identifier)
            .ToList();

        var careerJobTitleIds = page.CareerJobTitle?
            .Select(i => i.Identifier)
            .ToList();


        var employmentTags =
            employmentTypeIds?.Any() == true
                ? await _taxonomyRetriever.RetrieveTags(
                    employmentTypeIds,
                    "en-gb")
                : Enumerable.Empty<Tag>();

        var careerLevelTags =
            careerLevelIds?.Any() == true
                ? await _taxonomyRetriever.RetrieveTags(
                    careerLevelIds,
                    "en-gb")
                : Enumerable.Empty<Tag>();

        var careerJobTitleTags =
            careerJobTitleIds?.Any() == true
                ? await _taxonomyRetriever.RetrieveTags(
                    careerJobTitleIds,
                    "en-gb")
                : Enumerable.Empty<Tag>();

        var employmentTypeNames = employmentTags.Select(t => t.Name).ToList();
        var careerLevelNames = careerLevelTags.Select(t => t.Name).ToList();    
        var careerJobTitleNames = careerJobTitleTags.Select(t => t.Name).ToList();


        return new CareerCardDto
        {
            ImageURL = page.HeroBackgroundImage?
                .FirstOrDefault()?
                .ContentAttachmentAsset?
                .Url,

            ImageAltText = page.MetadataTeaserImageAltText,

            Title = page.MetadataTitle,

            JobTitle = page.MetadataTitle,

            Reference = page.CareerReference,

            EmploymentType = employmentTypeNames?.FirstOrDefault(),

            Level = careerLevelNames?.FirstOrDefault(),
                

            Locations = page.MetadataOfficeLocation?
                .Select(i => i.MetadataTitle)
                .ToArray(),

            PublishDate = page.DocumentPublishFrom,

            ApplyURL = page.GetUrl().AbsoluteUrl,

            JobTitleURL =
                careersPage.FirstOrDefault()?.MetadataTitle is not null
                && page.CareerJobTitle is not null
                    ? $"{careersPage.FirstOrDefault()?.GetUrl().AbsoluteUrl}?careerJobTitle={careerJobTitleNames?.FirstOrDefault()}"
                    : null,
        };
    }));

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
