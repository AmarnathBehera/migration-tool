using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.services;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.Web.Core.Services;
using Kentico.Content.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace BDO.GWT.Core.Services;





public interface ISectionSpecialtiesService
{
    Task<SectionSpecialtiesPageDto> GetSectionSpecialtiesDtoAsync(CancellationToken cancellationToken);
}

public class SectionSpecialtiesService : ISectionSpecialtiesService
{
    private readonly IHeroService _heroService;
    private readonly IContentService _contentService;
    private readonly IPageUtility _pageUtility;

    public SectionSpecialtiesService(IHeroService heroService, IContentService contentService, IPageUtility pageUtility)
    {
        _heroService = heroService;
        _contentService = contentService;
        _pageUtility = pageUtility;
    }

    public async Task<SectionSpecialtiesPageDto> GetSectionSpecialtiesDtoAsync( CancellationToken cancellationToken)
    {

        var page = await _contentService.RetrieveCurrentPageAsync<SectionSpecialties>(
            new RetrieveCurrentPageParameters { LinkedItemsMaxLevel = 4 },
            cancellationToken);

        if (page == null)
        {
            return new SectionSpecialtiesPageDto();
        }

        var cards = await _pageUtility.GetMultiLinkCardsAsync<SpecialtiesCategory, SpecialtiesArea>(
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

        var dto = new SectionSpecialtiesPageDto
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

        return dto;
    }
}