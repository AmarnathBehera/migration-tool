
using BDO.GWT.Common.Exceptions;
using BDO.GWT.Core;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.DTOs.Pages;
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
    private readonly ICultureContext _cultureContext;
    private readonly ICardsService _cardsService;
    private readonly IWebPageUrlRetriever _webPageUrlRetriever;
    private readonly ISettingService _settingsService;
    private static readonly char[] FilterSeparators = [',', ';', '\n', '\r'];
    public CareersPageService(
        IHeroService heroService,
        IContentService contentRetriever,
        ICultureContext cultureContext,
        ICardsService cardsService,
        IWebPageUrlRetriever webPageUrlRetriever,
        ISettingService settingsService)
    {
        _heroService = heroService;
        _contentRetriever = contentRetriever;
        _cultureContext = cultureContext;
        _cardsService = cardsService;
        _webPageUrlRetriever = webPageUrlRetriever;
        _settingsService = settingsService;
    }

    public async Task<CareersPageDto> GetCareersPageDtoAsync(CancellationToken cancellationToken)
    {
        var page = await _contentRetriever.RetrieveCurrentPageAsync<Careers>(
            new RetrieveCurrentPageParameters { }, cancellationToken);

        if (page == null)
        {
            return new CareersPageDto();
        }

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

        var dto = new CareersPageDto
        {
            Hero = await _heroService.GetHeroDtoAsync(heroData, cancellationToken),
            CardsGrid = await GetCardsGridDtoAsync(page, cancellationToken),
        };

        return dto;
    }

    public async Task<PaginationDto<CareerCardDto>> GetCareerPaginationResultAsync(
        Guid pageGuid,
        CardsQuery query,
        CancellationToken cancellationToken)
    {

        var careersPages = await _contentRetriever.GetPagesAsync<Careers>(
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

        var careerPages = await _contentRetriever.GetPagesAsync<Career>(
            new RetrievePagesParameters
            {
                LinkedItemsMaxLevel = 1,
                LanguageName = _cultureContext.CurrentLanguageTag,
                UseLanguageFallbacks = false
            },
            q => q.Where(w =>
            {
                w.WhereEquals("ContentItemChannelID", _contentRetriever.GetCurrentChannelID());
                w.WhereEquals("WebPageItemParentID", careersPage.SystemFields.WebPageItemID);
            }),
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

        IEnumerable<Career> filtered = careerPages;

        if (query?.Filters != null && query.Filters.Any())
        {
            foreach (var filter in query.Filters)
            {
                var filterValue = filter.Value;
                filtered = filter.Key switch
                {
                    "careerEmploymentType" => filtered.Where(c =>
                        c.CareerEmploymentType.Any(n =>
                            n.Name.Equals(filterValue, StringComparison.OrdinalIgnoreCase))),
                    "careerLevel" => filtered.Where(c =>
                        c.CareerLevel.Any(n =>
                            n.Name.Equals(filterValue, StringComparison.OrdinalIgnoreCase))),
                    "careerJobTitle" => filtered.Where(c =>
                        c.CareerJobTitle.Any(n =>
                            n.Name.Equals(filterValue, StringComparison.OrdinalIgnoreCase))),
                    "officeLocation" => filtered.Where(c =>
                        c.MetadataOfficeLocation?.Any(loc =>
                            loc.MetadataTitle.Equals(filterValue, StringComparison.OrdinalIgnoreCase)) == true),
                    _ => filtered
                };
            }
        }

        var filteredArray = filtered
            .OrderByDescending(c => c.DocumentPublishFrom)
            .ToArray();

        var totalCount = filteredArray.Length;


        var pageSize = query?.PageSize > 0 ? query.PageSize.Value : 20;
        var currentPage = query?.CurrentPage > 0 ? query.CurrentPage.Value : 1;
        var careerspageurl = await GetLanguageSafeUrlAsync(careersPage, cancellationToken);

        var pagedCards = filteredArray
    .Skip((currentPage - 1) * pageSize)
    .Take(pageSize)
    .ToArray();

        var itemData = await Task.WhenAll(pagedCards.Select(async c =>
        {
            var absolutepageurl = await GetLanguageSafeUrlAsync(c, cancellationToken);

            return new CareerCardDto
            {
                ImageURL =  c.MetadataTeaserImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url,
                ImageAltText = c.MetadataTeaserImageAltText,
                Title = c.MetadataTitle,
                JobTitle = c.CareerJobTitle?.FirstOrDefault()?.Name,
                Reference = c.CareerReference,
                EmploymentType = c.CareerEmploymentType?.FirstOrDefault()?.Name,
                Level = c.CareerLevel?.FirstOrDefault()?.Name,
                Locations = c.MetadataOfficeLocation?
        .Select(loc => loc.MetadataTitle ?? string.Empty)
        .ToArray()
        ?? Array.Empty<string>(),
                PublishDate = c.DocumentPublishFrom,
                ApplyURL = absolutepageurl,
                JobTitleURL = careersPage.MetadataTitle is not null && c.CareerJobTitle is not null
                    ? $"{careerspageurl}?careerJobTitle={c.CareerJobTitle.FirstOrDefault()?.Name}"
                    : null,
            };
        }));


        return new PaginationDto<CareerCardDto>
        {
            Data = itemData,
            TotalCount = totalCount
        };
    }


    

    private async Task<CardsGridDto> GetCardsGridDtoAsync(Careers page, CancellationToken cancellationToken)
    {
        var settings = await _settingsService.GetFilteringInfo();
        var settingInfo = settings?.FirstOrDefault();


        FilterItemDto<OptionDto>[]? filters = null;

        if (page.ItemListFilteringEnabled && !string.IsNullOrWhiteSpace(page.CareerListFilters))
        {
            var filterHashSet = page.CareerListFilters
                .Split(FilterSeparators, StringSplitOptions.RemoveEmptyEntries)
                .Select(f => f.Trim())
                .Where(f => !string.IsNullOrEmpty(f))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (filterHashSet.Count != 0)
            {
                filters = await _cardsService.CreateFiltersAsync(filterHashSet, null, cancellationToken);
            }
        }

        var CareersCardsGridDto = new CardsGridDto
        {
            Title = page.ItemListTitle,
            Description = page.ItemListDescription,
            Filters = filters,
            FilteringTooltipEnabled = page.ItemListFilteringEnabled && settingInfo?.Showtooltip == true , 
            PageSize = 20,
            ApiEndpointName = $"Careers/{page.SystemFields.WebPageItemGUID}/Get",
            AdvancedSearchPrelinkText = page.ItemListAdvancedSearchDescription,
            AdvancedSearchLinkText = page.ItemListAdvancedSearchLinkText,
            AdvancedSearchLink = page.ItemListAdvancedSearchLinkUrl?.FirstOrDefault()?.ToString()
        };

        return CareersCardsGridDto;
    }

    private async Task<string> GetLanguageSafeUrlAsync(
    IWebPageFieldsSource page,
    CancellationToken cancellationToken)
    {
        var url = await _webPageUrlRetriever.Retrieve(
            page,
            _cultureContext.CurrentLanguageTag,
            cancellationToken);

        return url.AbsoluteUrl;
    }
}