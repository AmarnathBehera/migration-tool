using BDO.GWT.Core;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.ReusableFieldsDTOs;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.ValueObjects;
using CMS.ContentEngine;
using CMS.Websites;

namespace BDO.GWT.Web.Core.Services;

internal static class InsightListingDtoMapper
{
    public static async Task<InsightCardListingDto> MapToInsightCardListingDtoAsync<TPage>(
        TPage page,
        IHeroService heroService,
        Func<TPage, Task<CardsGridDto>> cardsGridFactory,
        CancellationToken cancellationToken)
        where TPage : IInsightListingPage
    {
        var sys = page.SystemFields;

        return new InsightCardListingDto
        {
            Title = page.MetadataTitle,
            Hero = await heroService.GetHeroDtoAsync(new HeroData
            {
                Layout = page.HeroLayout ?? string.Empty,
                Title = page.HeroTitle ?? string.Empty,
                Description = page.HeroDescription ?? string.Empty,
                BackgroundImage = page.HeroBackgroundImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url is string bgUrl ? Link.Parse(bgUrl) : null,
                BackgroundImageAltText = page.HeroBackgroundImageAltText ?? string.Empty,
                MobileBackgroundImage = page.HeroBackgroundImage_Mobile?.FirstOrDefault()?.ContentAttachmentAsset?.Url is string mobileUrl ? Link.Parse(mobileUrl) : null,
                MobileBackgroundImageAltText = page.HeroBackgroundImage_MobileAltText ?? string.Empty,
                CallToActionButtonType = page.HeroCallToActionButtonType ?? string.Empty,
                CallToActionButtonText = page.HeroCallToActionButtonText ?? string.Empty,
                CallToActionButtonUrl = page.HeroCallToActionButtonUrl?.FirstOrDefault()?.SystemFields?.WebPageUrlPath is string ctaPath ? Link.Parse(ctaPath) : null,
            }, cancellationToken),
            BaseContent = new BaseContent
            {
                ContentItemID = sys?.ContentItemID ?? 0,
                ContentItemGUID = sys?.ContentItemGUID ?? Guid.Empty,
                ContentItemName = sys?.ContentItemName,
                ContentItemContentTypeID = sys?.ContentItemContentTypeID ?? 0,
                ContentItemIsSecured = sys?.ContentItemIsSecured ?? false,
                ContentItemCommonDataContentLanguageID = sys?.ContentItemCommonDataContentLanguageID ?? 0,
                ContentItemCommonDataFirstPublishedWhen = sys?.ContentItemCommonDataFirstPublishedWhen ?? default,
                ContentItemCommonDataLastPublishedWhen = sys?.ContentItemCommonDataLastPublishedWhen ?? default
            },
            BaseWebPage = new BaseWebPage
            {
                WebPageItemGUID = sys?.WebPageItemGUID ?? Guid.Empty,
                WebPageItemID = sys?.WebPageItemID ?? 0,
                WebPageItemName = sys?.WebPageItemName,
                WebPageItemWebsiteChannelID = sys?.WebPageItemWebsiteChannelId ?? 0,
                WebPageUrlPath = sys?.WebPageUrlPath,
                WebPageItemOrder = sys?.WebPageItemOrder ?? 0,
                WebPageItemTreePath = sys?.WebPageItemTreePath,
                WebPageUrlPathContentLanguageID = sys?.WebPageUrlPathContentLanguageID ?? 0,
                WebPageItemParentID = sys?.WebPageItemParentID ?? 0
            },
            MetaData = new MetaDataDto
            {
                MetaTitle = page.MetadataTitle,
                MetaDescription = page.MetadataDescription,
                MetadataTeaserImage = page.MetadataTeaserImage.FirstOrDefault()?.ContentAttachmentAsset?.Url,
                MetaTeaserImageAltText = page.MetadataTeaserImageAltText,
                MetaCanonical = page.MetadataCanonical,
            },
            SearchAndSEO = new SearchAndSEODto
            {
                MetadataNofollow = page.MetadataNofollow,
                MetadataNoindex = page.MetadataNoindex,
                IncludeInSitemap = page.IncludeInSitemap,
            },
            OGMetaData = new OGMetaDataDto
            {
                MetadataOGTitle = page.MetadataOGTitle,
                MetadataOGDescription = page.MetadataOGDescription,
                MetadataOGImage = page.MetadataOGImage.FirstOrDefault()?.ContentAttachmentAsset?.Url,
                MetadataOGTwitterImage = page.MetadataOGTwitterImage.FirstOrDefault()?.ContentAttachmentAsset?.Url,
                MetadataOGTwitterImageSizeCheckbox = page.MetadataOGTwitterImageSizeCheckbox,
                Url = page.MetadataCanonical,
            },
            CardsGrid = await cardsGridFactory(page),
        };
    }
}

public interface IInsightListingPage
{
    WebPageFields? SystemFields { get; }
    string? MetadataTitle { get; }
    string? MetadataDescription { get; }
    IEnumerable<ContentAttachment> MetadataTeaserImage { get; }
    string? MetadataTeaserImageAltText { get; }
    string? MetadataCanonical { get; }
    bool MetadataNofollow { get; }
    bool MetadataNoindex { get; }
    bool IncludeInSitemap { get; }
    string? MetadataOGTitle { get; }
    string? MetadataOGDescription { get; }
    IEnumerable<ContentAttachment> MetadataOGImage { get; }
    IEnumerable<ContentAttachment> MetadataOGTwitterImage { get; }
    bool MetadataOGTwitterImageSizeCheckbox { get; }
    string? HeroLayout { get; }
    string? HeroTitle { get; }
    string? HeroDescription { get; }
    IEnumerable<ContentAttachment> HeroBackgroundImage { get; }
    string? HeroBackgroundImageAltText { get; }
    IEnumerable<ContentAttachment> HeroBackgroundImage_Mobile { get; }
    string? HeroBackgroundImage_MobileAltText { get; }
    string? HeroCallToActionButtonType { get; }
    string? HeroCallToActionButtonText { get; }
    IEnumerable<IWebPageFieldsSource> HeroCallToActionButtonUrl { get; }
}
