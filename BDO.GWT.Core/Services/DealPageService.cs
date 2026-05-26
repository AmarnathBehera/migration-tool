using BDO.GWT.Common.Exceptions;
using BDO.GWT.Core;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.Utilities;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.Domain.Core.Constants;
using BDO.GWT.Domain.Core.Entities.BDODealCards;
using BDO.GWT.Domain.Core.Entities.Pages;
using BDO.GWT.Domain.Core.Exceptions;
using BDO.GWT.Domain.Core.Retrievers;
using BDO.GWT.Domain.Core.Services;
using BDO.GWT.Web.Core.DTOs.Pages;
using BDO.GWT.Web.Core.Extensions;
using BDO.GWT.Web.Core.LiveSite.DTOs;
using BDO.GWT.Web.Core.LiveSite.Helpers;
using BDO.GWT.Web.Core.Services;
using BDO.GWT.Web.Core.Utilities;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using Kentico.Xperience.Admin.Base;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IndustryEntity = BDO.GWT.Domain.Core.Entities.BDODealCards.Industry;

namespace BDO.GWT.Web.Core.LiveSite.Services;

public interface IDealPageService
{
    Task<DealDto> GetDealPageDtoAsync(CancellationToken cancellationToken);
}

public class DealPageService : IDealPageService
{
    private const string GlobalLanguage = Language.English;


    private readonly ICultureContext _cultureContext;
    private readonly IHeroService _heroService;
    //private readonly IPageRetriever<SectionDeals> _sectionDealPageRetriever;
    //private readonly IPeopleHelper _peopleHelper;
    //private readonly IRetriever<DealType> _dealTypeRetriever;
    //private readonly IRetriever<GlobalDealType> _globalDealTypeRetriever;
    //private readonly IRetriever<GlobalIndustry> _globalIndustryRetriever;
    //private readonly IRetriever<GlobalSubIndustry> _globalSubIndustryRetriever;
    //private readonly IRetriever<IndustryEntity> _industryRetriever;
    //private readonly IRetriever<SubIndustry> _subIndustryRetriever;
    private readonly ISidebarService _sidebarService;
    private readonly IContentService _contentRetriever;
    private readonly IWebsiteChannelContext _websiteChannelContext;

    public DealPageService(ICultureContext cultureContext, IHeroService heroService, ISidebarService sidebarService, IContentService contentRetriever, IWebsiteChannelContext websiteChannelContext /*IPageRetriever<SectionDeals> dealSectionPageRetriever, IPeopleHelper peopleHelper, IRetriever<DealType> dealTypeRetriever, IRetriever<GlobalDealType> globalDealTypeRetriever, IRetriever<GlobalIndustry> globalIndustryRetriever, IRetriever<GlobalSubIndustry> globalSubIndustryRetriever, IRetriever<IndustryEntity> industryRetriever, IRetriever<SubIndustry> subIndustryRetriever*/)
    {
        _cultureContext = cultureContext;
        _heroService = heroService;
        //_sectionDealPageRetriever = dealSectionPageRetriever;
        //_peopleHelper = peopleHelper;
        //_dealTypeRetriever = dealTypeRetriever;
        //_globalDealTypeRetriever = globalDealTypeRetriever;
        //_globalIndustryRetriever = globalIndustryRetriever;
        //_globalSubIndustryRetriever = globalSubIndustryRetriever;
        //_industryRetriever = industryRetriever;
        //_subIndustryRetriever = subIndustryRetriever;
        _sidebarService = sidebarService;
        _contentRetriever = contentRetriever;
        _websiteChannelContext = websiteChannelContext;
    }

    public async Task<DealDto> GetDealPageDtoAsync(CancellationToken cancellationToken)
    {
        //deal page is expected to be only one per channel, so we take the first one. If there are more than one, it will be a content issue that needs to be fixed.
        var results = await _contentRetriever.GetPagesAsync<Deal>(
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

        var dealPage = results?.FirstOrDefault();

        if (dealPage == null)
        {
            return new DealDto(); // or throw exception if required
        }

        var sys = dealPage.SystemFields;

        // Map hero data
        var heroData = new HeroData
        {
            Layout = dealPage.HeroLayout,
            Title = dealPage.HeroTitle,
            Description = dealPage.HeroDescription,
            BackgroundImage = dealPage.HeroBackgroundImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url is string bgUrl ? Link.Parse(bgUrl) : null,
            BackgroundImageAltText = dealPage.HeroBackgroundImageAltText,
            MobileBackgroundImage = dealPage.HeroBackgroundImage_Mobile?.FirstOrDefault()?.ContentAttachmentAsset?.Url is string mobileUrl ? Link.Parse(mobileUrl) : null,
            MobileBackgroundImageAltText = dealPage.HeroBackgroundImage_MobileAltText,
            CallToActionButtonType = dealPage.HeroCallToActionButtonType,
            CallToActionButtonText = dealPage.HeroCallToActionButtonText,
            CallToActionButtonUrl = dealPage.HeroCallToActionButtonUrl?.FirstOrDefault()?.SystemFields?.WebPageUrlPath is string ctaPath ? Link.Parse(ctaPath) : null,
        };

        var dto = new DealDto
        {
            Hero = await _heroService.GetHeroDtoAsync(heroData, cancellationToken),
            Title = dealPage.ShowTitle ? dealPage.MetadataTitle : null,
            DealTypeNames = await GetDealTypeNames(dealPage.DealType, cancellationToken),
            IndustryNames = await GetIndustryNames(dealPage.Industry, cancellationToken),
            SubIndustryNames = await GetSubIndustryNames(dealPage.SubIndustry, cancellationToken),
            ClientName = dealPage.ClientName,
            Content = HtmlUtility.RemoveTildeFromUrlTypeHtmlAttributes(dealPage.DealContent),
            Sidebar = dealPage.ShowSidebar ? await GetSidebarDto(dealPage, cancellationToken) : null,
        };

        return dto;
    }

    private async Task<List<string>> GetDealTypeNames(ICollection<int> dealTypeIds, CancellationToken cancellationToken)
    {
        if (_cultureContext.CurrentLanguageTag.Language == GlobalLanguage)
        {
            var globalDealTypes = await _globalDealTypeRetriever.RetrieveEntitiesAsync(query => query.Where(x => dealTypeIds.Contains(x.ID)), cancellationToken);

            var globalDealTypeNames = globalDealTypes.Select(x => x.Name).ToList();

            return globalDealTypeNames;
        }

        var dealTypes = await _dealTypeRetriever.RetrieveEntitiesAsync(query => query
            .Where(x => x.LanguageTag == _cultureContext.CurrentLanguageTag && dealTypeIds.Contains(x.ID))
        , cancellationToken);

        var dealTypeNames = dealTypes.Select(x => x.Name).ToList();

        return dealTypeNames;
    }

    private async Task<List<string>> GetIndustryNames(ICollection<string> industryCodeNames, CancellationToken cancellationToken)
    {
        if (_cultureContext.CurrentLanguageTag.Language == GlobalLanguage)
        {
            var globalIndustries = await _globalIndustryRetriever.RetrieveEntitiesAsync(query => query.Where(x => industryCodeNames.Contains(x.CodeName)), cancellationToken);

            var globalIndustryNames = globalIndustries.Select(x => x.Name).ToList();

            return globalIndustryNames;
        }

        var industries = await _industryRetriever.RetrieveEntitiesAsync(query => query
            .Where(x => x.LanguageTag == _cultureContext.CurrentLanguageTag && industryCodeNames.Contains(x.CodeName))
        , cancellationToken);

        var industryNames = industries.Select(x => x.Name).ToList();

        return industryNames;
    }

    private async Task<List<string>> GetSubIndustryNames(ICollection<string> subIndustryCodeNames, CancellationToken cancellationToken)
    {
        if (_cultureContext.CurrentLanguageTag.Language == GlobalLanguage)
        {
            var globalSubIndustries = await _globalSubIndustryRetriever.RetrieveEntitiesAsync(query => query.Where(x => subIndustryCodeNames.Contains(x.CodeName)), cancellationToken);

            var globalSubIndustryNames = globalSubIndustries.Select(x => x.Name).ToList();

            return globalSubIndustryNames;
        }

        var subIndustries = await _subIndustryRetriever.RetrieveEntitiesAsync(query => query
            .Where(x => x.LanguageTag == _cultureContext.CurrentLanguageTag && subIndustryCodeNames.Contains(x.CodeName))
        , cancellationToken);

        var subIndustryNames = subIndustries.Select(x => x.Name).ToList();

        return subIndustryNames;
    }

    private async Task<DealSidebarDto> GetSidebarDto(Deal page, CancellationToken cancellationToken)
    {
        var sidebarDto = await _sidebarService.GetDealSidebarDtoAsync(page, cancellationToken);

        var copyLinkButtonSectionOption = await GetShowCopyLinkOptionFromSectionPageAsync(sidebarDto, cancellationToken);

        sidebarDto.ShowCopyLinkButton = copyLinkButtonSectionOption;

        sidebarDto.Date = page.DateField;

        sidebarDto.Contacts = (await _peopleHelper.GetKeyContactDtos(page.RelevantContactPerson, page.PeopleSort, page.LanguageTag, cancellationToken)).Take(page.DisplayedContacts).ToArray();

        return sidebarDto;
    }

    private async Task<bool> GetShowCopyLinkOptionFromSectionPageAsync(ContentSidebarDto dto, CancellationToken cancellationToken)
    {
        var dealSectionPage = (await _sectionDealPageRetriever.RetrievePagesAsync(query => query.Take(1), cancellationToken)).FirstOrDefault() ?? throw new MissingSectionPageException($"'No pages have been found of type '{nameof(SectionDeals)}'.");

        return dealSectionPage.ShowCopyUrlButtonInDealPage && dto.ShowCopyLinkButton;
    }
}
