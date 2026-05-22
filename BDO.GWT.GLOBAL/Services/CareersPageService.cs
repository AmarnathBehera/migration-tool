using BDO.GWT.Common.Core.Exceptions;
using BDO.GWT.Core;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.Web.Core.DTOs;
using CMS.ContentEngine;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Core.Services;

public interface ICareersPageService
{
    Task<CareersPageDto> GetCareersPageDtoAsync(CancellationToken cancellationToken);
    Task<PaginationDto<CareerCardDto>> GetCareerPaginationResultAsync(Guid pageGuid, CardsQuery query, CancellationToken cancellationToken);
}

public class CareersPageService : ICareersPageService
{
    private readonly IHeroService _heroService;
    private readonly IContentService _contentRetriever;
    private readonly IWebsiteChannelContext _websiteChannelContext;
    private readonly IContentRetriever _retriever;
    private readonly ICultureContext _cultureContext;
    private readonly ITaxonomyRetriever _taxonomyRetriever;
    private readonly ICardsService _cardsService;

    public CareersPageService(
        IHeroService heroService,
        IContentService contentRetriever,
        IWebsiteChannelContext websiteChannelContext,
        IContentRetriever retriever,
        ITaxonomyRetriever taxonomyRetriever,
        ICultureContext cultureContext,
        ICardsService cardsService)
    {
        _heroService = heroService;
        _contentRetriever = contentRetriever;
        _websiteChannelContext = websiteChannelContext;
        _retriever = retriever;
        _taxonomyRetriever = taxonomyRetriever;
        _cultureContext = cultureContext;
        _cardsService = cardsService;
    }

    public async Task<CareersPageDto> GetCareersPageDtoAsync(CancellationToken cancellationToken)
    {
        var page = await _contentRetriever.RetrieveCurrentPageAsync<Careers>(
            new RetrieveCurrentPageParameters { }, cancellationToken);

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
            }, cancellationToken),
            CardsGrid = await GetCardsGridDtoAsync(page, cancellationToken),
        };

        return dto;
    }

    public async Task<PaginationDto<CareerCardDto>> GetCareerPaginationResultAsync(
        Guid pageGuid,
        CardsQuery query,
        CancellationToken cancellationToken)
    {

        var careersPages = await _retriever.RetrievePages<Careers>(
            new RetrievePagesParameters { LinkedItemsMaxLevel = 2 },
            q => q.Where(w => w.WhereEquals("WebPageItemGUID", pageGuid)),
            cacheSettings: RetrievalCacheSettings.CacheDisabled,
            cancellationToken: cancellationToken);

        var careersPage = careersPages.FirstOrDefault();

        if (careersPage == null)
        {
            throw new MissingSectionPageException(
                $"Careers page with GUID '{pageGuid}' not found.");
        }

        var careerPages = await _retriever.RetrievePages<Career>(
            new RetrievePagesParameters { LinkedItemsMaxLevel = 1 },
            q => q.Where(w => w.WhereEquals("WebPageItemParentID", careersPage.SystemFields.WebPageItemID)),
            cacheSettings: RetrievalCacheSettings.CacheDisabled,
            cancellationToken: cancellationToken);

        if (careerPages == null || !careerPages.Any())
        {
            return new PaginationDto<CareerCardDto>
            {
                Data = Array.Empty<CareerCardDto>(),
                TotalCount = 0
            };
        }

        var languageTag = _cultureContext.CurrentLanguageTag;

        var allResolvedCards = await Task.WhenAll(careerPages.Select(async page =>
        {
            var employmentTags = await ResolveTagsAsync(page.CareerEmploymentType, languageTag, cancellationToken);
            var careerLevelTags = await ResolveTagsAsync(page.CareerLevel, languageTag, cancellationToken);
            var jobTitleTags = await ResolveTagsAsync(page.CareerJobTitle, languageTag, cancellationToken);

            return new
            {
                Page = page,
                EmploymentTypeNames = employmentTags.Select(t => t.Name).ToList(),
                CareerLevelNames = careerLevelTags.Select(t => t.Name).ToList(),
                JobTitleNames = jobTitleTags.Select(t => t.Name).ToList(),
            };
        }));

        var filtered = allResolvedCards.AsEnumerable();

        if (query?.Filters != null && query.Filters.Any())
        {
            foreach (var filter in query.Filters)
            {
                var filterValue = filter.Value;

                filtered = filter.Key switch
                {
                    "careerEmploymentType" => filtered.Where(c =>
                        c.EmploymentTypeNames.Any(n =>
                            n.Equals(filterValue, StringComparison.OrdinalIgnoreCase))),

                    "careerLevel" => filtered.Where(c =>
                        c.CareerLevelNames.Any(n =>
                            n.Equals(filterValue, StringComparison.OrdinalIgnoreCase))),

                    "careerJobTitle" => filtered.Where(c =>
                        c.JobTitleNames.Any(n =>
                            n.Equals(filterValue, StringComparison.OrdinalIgnoreCase))),

                    "officeLocation" => filtered.Where(c =>
                        c.Page.MetadataOfficeLocation?.Any(loc =>
                            loc.MetadataTitle.Equals(filterValue, StringComparison.OrdinalIgnoreCase)) == true),

                    _ => filtered
                };
            }
        }

        var filteredArray = filtered
            .OrderByDescending(c => c.Page.DocumentPublishFrom)
            .ToArray();

        var totalCount = filteredArray.Length;


        var pageSize = query?.PageSize > 0 ? query.PageSize.Value : 20;
        var currentPage = query?.CurrentPage > 0 ? query.CurrentPage.Value : 1;

        var pagedCards = filteredArray
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize);


        var itemData = pagedCards.Select(c => new CareerCardDto
        {
            ImageURL = c.Page.HeroBackgroundImage?
                .FirstOrDefault()?
                .ContentAttachmentAsset?
                .Url,

            ImageAltText = c.Page.MetadataTeaserImageAltText,

            Title = c.Page.MetadataTitle,

            JobTitle = c.JobTitleNames.FirstOrDefault() ?? c.Page.MetadataTitle,

            Reference = c.Page.CareerReference,

            EmploymentType = c.EmploymentTypeNames.FirstOrDefault(),

            Level = c.CareerLevelNames.FirstOrDefault(),

            Locations = c.Page.MetadataOfficeLocation?
                .Select(i => i.MetadataTitle)
                .ToArray(),

            PublishDate = c.Page.DocumentPublishFrom,

            ApplyURL = c.Page.GetUrl().AbsoluteUrl,

            JobTitleURL =
                careersPage.MetadataTitle is not null && c.Page.CareerJobTitle is not null
                    ? $"{careersPage.GetUrl().AbsoluteUrl}?careerJobTitle={c.JobTitleNames.FirstOrDefault()}"
                    : null,

        }).ToArray();

        return new PaginationDto<CareerCardDto>
        {
            Data = itemData,
            TotalCount = totalCount
        };
    }


    private async Task<IEnumerable<Tag>> ResolveTagsAsync(
        IEnumerable<TagReference>? tagReferences,
        string languageTag,
        CancellationToken cancellationToken)
    {
        // TagReference has an Identifier property of type Guid
        var identifiers = tagReferences?
            .Select(t => t.Identifier)
            .ToList();

        return identifiers?.Any() == true
            ? await _taxonomyRetriever.RetrieveTags(identifiers, languageTag, cancellationToken)
            : Enumerable.Empty<Tag>();
    }

    private async Task<CardsGridDto> GetCardsGridDtoAsync(Careers page, CancellationToken cancellationToken)
    {

        FilterItemDto<OptionDto>[]? filters = null;

        if (page.ItemListFilteringEnabled && !string.IsNullOrWhiteSpace(page.CareerListFilters))
        {
            var filterHashSet = page.CareerListFilters
                .Split(new[] { ',', ';', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(f => f.Trim())
                .Where(f => !string.IsNullOrEmpty(f))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (filterHashSet.Any())
            {
                filters = await _cardsService.CreateFiltersAsync(filterHashSet, null, cancellationToken);
            }
        }

        var insightCardsGridDto = new CardsGridDto
        {
            Title = page.ItemListTitle,
            Description = page.ItemListDescription,
            Filters = filters,
            FilteringTooltipEnabled = true,
            PageSize = 20,
            ApiEndpointName = $"Careers/{page.SystemFields.WebPageItemGUID}/Get",
            AdvancedSearchPrelinkText = page.ItemListAdvancedSearchDescription,
            AdvancedSearchLinkText = page.ItemListAdvancedSearchLinkText,
            AdvancedSearchLink = page.ItemListAdvancedSearchLinkUrl?.FirstOrDefault()?.ToString()
        };

        return insightCardsGridDto;
    }
}