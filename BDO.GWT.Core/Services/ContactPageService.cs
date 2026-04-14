

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

    public ContactPageService(IContentService pageRetriever, IHeroService heroService, IWebsiteChannelContext websiteChannelContext)
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
        var ContactPage = results.FirstOrDefault();

        if (ContactPage is null)
        {
            return null;
        }


        return new FormPageDto
        {
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
                ContentItemID = ContactPage.SystemFields.ContentItemID,
                ContentItemGUID = ContactPage.SystemFields.ContentItemGUID,
                ContentItemName = ContactPage.SystemFields.ContentItemName,
                ContentItemContentTypeID = ContactPage.SystemFields.ContentItemContentTypeID,
                ContentItemIsSecured = ContactPage.SystemFields.ContentItemIsSecured,
                //ContentItemCommonDataVersionStatus = homePage.SystemFields.ContentItemCommonDataVersionStatus,
                ContentItemCommonDataContentLanguageID = ContactPage.SystemFields.ContentItemCommonDataContentLanguageID,
                ContentItemCommonDataFirstPublishedWhen = ContactPage.SystemFields.ContentItemCommonDataFirstPublishedWhen,
                ContentItemCommonDataLastPublishedWhen = ContactPage.SystemFields.ContentItemCommonDataLastPublishedWhen
            },
            BaseWebPage = new BaseWebPage
            {
                WebPageItemGUID = ContactPage.SystemFields.WebPageItemGUID,
                WebPageItemID = ContactPage.SystemFields.WebPageItemID,
                WebPageItemName = ContactPage.SystemFields.WebPageItemName,
                WebPageItemWebsiteChannelID = ContactPage.SystemFields.WebPageItemWebsiteChannelId,
                WebPageUrlPath = ContactPage.SystemFields.WebPageUrlPath,
                WebPageItemOrder = ContactPage.SystemFields.WebPageItemOrder,
                WebPageItemTreePath = ContactPage.SystemFields.WebPageItemTreePath,
                WebPageUrlPathContentLanguageID = ContactPage.SystemFields.WebPageUrlPathContentLanguageID,
                WebPageItemParentID = ContactPage.SystemFields.WebPageItemParentID
            },
            Hero = await _heroService.GetHeroDtoAsync(ContactPage, cancellationToken),
            
        };
    }
}
