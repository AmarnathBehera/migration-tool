using BDO.GWT.Core;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.services;
using BDO.GWT.Core.ValueObjects;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Core.Services;

public interface ISectionBlogService
{
    Task<SectionBlogsPageDto> GetSectionBlogsDtoAsync(CancellationToken cancellationToken);
}

public class SectionBlogService : ISectionBlogService
{
    private readonly IHeroService _heroService;
    private readonly IContentService _contentService;
    private readonly IPageUtility _pageUtility;

    public SectionBlogService(
        IHeroService heroService,
        IContentService contentService,
        IPageUtility pageUtility)
    {
        _heroService = heroService;
        _contentService = contentService;
        _pageUtility = pageUtility;
    }

    public async Task<SectionBlogsPageDto> GetSectionBlogsDtoAsync(CancellationToken cancellationToken)
    {
        var page = await _contentService.RetrieveCurrentPageAsync<SectionBlogs>(
            new RetrieveCurrentPageParameters { LinkedItemsMaxLevel = 4 },
            cancellationToken);

        if (page == null)
        {
            return new SectionBlogsPageDto();
        }

        var cards = await _pageUtility.GetMultiLinkCardsAsync2<Blog,BlogContributors,Contact>(
           parentWebPageItemId: page.SystemFields.WebPageItemID,
           cardTitleSelector: ic => ic.MetadataTitle,
           cardDescriptionSelector: ic => ic.MetadataDescription,
           cardImageUrlSelector: ic => ic.MetadataTeaserImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url,
           cardImageAltTextSelector: ic => ic.MetadataTeaserImageAltText,
           childTitleSelector: industry => industry.MetadataTitle,
           child2TitleSelector: contact => contact.MetadataTitle,
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

        return new SectionBlogsPageDto
        {
            Hero = await _heroService.GetHeroDtoAsync(heroData, cancellationToken),
            MultiLinksCardsGrid = new MultiLinkCardsGridDto
            {
                Title = page.MetadataTitle,
                //Description = page.GridSummary,
                Cards = cards,
                //IsMultiLinks = page.ShowMultiLinks,
            },
        };
    }
}