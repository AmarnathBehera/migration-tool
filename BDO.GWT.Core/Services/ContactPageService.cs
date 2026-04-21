using BDO.GWT.Core;
using BDO.GWT.Core.DTOs.ReusableFieldsDTOs;
using BDO.GWT.Web.Core.DTOs.Pages;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;

namespace BDO.GWT.Web.Core.Services;

public interface IContactPageService
{
    Task<FormPageDto> GetContactPageDtoAsync(Contact page, CancellationToken cancellationToken);
}

public class ContactPageService : IContactPageService
{
    private readonly IHeroService _heroService;
    private readonly IContentService _contentRetriever;
    private readonly IWebsiteChannelContext _websiteChannelContext;

    public ContactPageService(
        IContentService pageRetriever,
        IHeroService heroService,
        IWebsiteChannelContext websiteChannelContext)
    {
        _contentRetriever = pageRetriever;
        _heroService = heroService;
        _websiteChannelContext = websiteChannelContext;
    }

    public async Task<FormPageDto> GetContactPageDtoAsync(Contact page, CancellationToken cancellationToken)
    {
        var results = await _contentRetriever.GetPagesAsync<Contact>(
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

        var ContactPage = results?.FirstOrDefault();

        if (ContactPage == null)
        {
            return new FormPageDto(); // or throw exception if required
        }

        var sys = ContactPage.SystemFields;

        return new FormPageDto
        {
            Title = ContactPage.DescriptionTitle,
            Description = ContactPage.DescriptionBody,
            FormType = ContactPage.FormType,
            Form = ContactPage.Form,
            EmbedCode = ContactPage.EmbedCode,

            MetaData = new MetaDataDto
            {
                MetaTitle = ContactPage.MetadataTitle,
                MetaDescription = ContactPage.MetadataDescription,
                MetadataTeaserImage = ContactPage.MetadataTeaserImage,
                MetaTeaserImageAltText = ContactPage.MetadataTeaserImageAltText,
                MetaCanonical = ContactPage.MetadataCanonical
            },

            SearchAndSEO = new SearchAndSEODto
            {
                IncludeInSitemap = ContactPage.IncludeInSitemap,
                MetadataNofollow = ContactPage.MetadataNofollow,
                MetadataNoindex = ContactPage.MetadataNoindex
            },

            OGMetaData = new OGMetaDataDto
            {
                MetadataOGTitle = ContactPage.MetadataOGTitle,
                MetadataOGDescription = ContactPage.MetadataOGDescription,
                MetadataOGImage = ContactPage.MetadataOGImage,
                MetadataOGTwitterImage = ContactPage.MetadataOGTwitterImage,
                MetadataOGTwitterImageSizeCheckbox = ContactPage.MetadataOGTwitterImageSizeCheckbox
            },

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

            Hero = await _heroService.GetHeroDtoAsync(ContactPage, cancellationToken),
        };
    }
}
