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

        var contactPage = results?.FirstOrDefault();

        if (contactPage == null)
        {
            return new FormPageDto(); // or throw exception if required
        }

        return new FormPageDto
        {
            MetaData = new MetaDataDto
            {
                MetaTitle = contactPage?.MetadataTitle,
                MetaDescription = contactPage?.MetadataDescription,
                MetaTeaserImageAltText = contactPage?.MetadataTeaserImageAltText,
                MetaCanonical = contactPage?.MetadataCanonical
            },

            SearchAndSEO = new SearchAndSEODto
            {
                IncludeInSitemap = contactPage?.IncludeInSitemap ?? false,
                MetadataNofollow = contactPage?.MetadataNofollow ?? false,
                MetadataNoindex = contactPage?.MetadataNoindex ?? false
            },

            OGMetaData = new OGMetaDataDto
            {
                MetadataOGTitle = contactPage?.MetadataOGTitle,
                MetadataOGDescription = contactPage?.MetadataOGDescription,
                MetadataOGTwitterImageSizeCheckbox = contactPage?.MetadataOGTwitterImageSizeCheckbox ?? false
            },

            BaseContent = new BaseContent
            {
                ContentItemID = contactPage?.SystemFields?.ContentItemID ?? 0,
                ContentItemGUID = contactPage?.SystemFields?.ContentItemGUID ?? Guid.Empty,
                ContentItemName = contactPage?.SystemFields?.ContentItemName,
                ContentItemContentTypeID = contactPage?.SystemFields?.ContentItemContentTypeID ?? 0,
                ContentItemIsSecured = contactPage?.SystemFields?.ContentItemIsSecured ?? false,
                ContentItemCommonDataContentLanguageID = contactPage?.SystemFields?.ContentItemCommonDataContentLanguageID ?? 0,
                ContentItemCommonDataFirstPublishedWhen = contactPage?.SystemFields?.ContentItemCommonDataFirstPublishedWhen,
                ContentItemCommonDataLastPublishedWhen = contactPage?.SystemFields?.ContentItemCommonDataLastPublishedWhen
            },

            BaseWebPage = new BaseWebPage
            {
                WebPageItemGUID = contactPage?.SystemFields?.WebPageItemGUID ?? Guid.Empty,
                WebPageItemID = contactPage?.SystemFields?.WebPageItemID ?? 0,
                WebPageItemName = contactPage?.SystemFields?.WebPageItemName,
                WebPageItemWebsiteChannelID = contactPage?.SystemFields?.WebPageItemWebsiteChannelId ?? 0,
                WebPageUrlPath = contactPage?.SystemFields?.WebPageUrlPath,
                WebPageItemOrder = contactPage?.SystemFields?.WebPageItemOrder ?? 0,
                WebPageItemTreePath = contactPage?.SystemFields?.WebPageItemTreePath,
                WebPageUrlPathContentLanguageID = contactPage?.SystemFields?.WebPageUrlPathContentLanguageID ?? 0,
                WebPageItemParentID = contactPage?.SystemFields?.WebPageItemParentID ?? 0
            },


           Hero = await _heroService.GetHeroDtoAsync(contactPage, cancellationToken) 
    
    };
    }
}
