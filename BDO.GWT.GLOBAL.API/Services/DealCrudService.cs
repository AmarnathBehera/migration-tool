using BDO.GWT.Common.Core.Exceptions;
using BDO.GWT.Core;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.GLOBAL.API.Data;
using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Membership;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;

namespace BDO.GWT.GLOBAL.API.Services;

public interface IDealCrudService
{
    Task<ActionResult<IEnumerable<PageOutput<DealPageContent>>>> GetDealPagesAsync(int channelId, string? culture, bool? onlyPublished, CancellationToken cancellationToken);
    Task<ActionResult<PageOutput<DealPageContent>>> GetDealPageAsync(int channelId, Guid pageGUID, string? culture, CancellationToken cancellationToken);
    Task<ActionResult<PageOutput<DealPageContent>>> CreatePageAsync(int channelId, int nodeParentID, string? culture, DealPageContent request, CancellationToken cancellationToken);
    Task<ActionResult<PageOutput<DealPageContent>>> CreatePageCultureVersionAsync(int channelId, Guid pageGUID, string? fromCulture, string? toCulture, DealPageContent request, CancellationToken cancellationToken);
    Task<ActionResult<PageOutput<DealPageContent>>> UpdatePageAsync(int channelId, Guid pageGUID, string? culture, DealPageContent request, CancellationToken cancellationToken);
    Task DeletePageAsync(int channelId, Guid pageGUID, string? culture, bool deleteAllCultures, CancellationToken cancellationToken);
}

public class DealCrudService : IDealCrudService
{
    private readonly IContentService _contentRetriever;
    private readonly IValidationState _validationState;
    private readonly IWebsiteChannelContext _websiteChannelContext;
    private readonly ICultureService _cultureService;
    private readonly IPageContentValidator _pageContentValidator;
    private readonly IInfoProvider<ChannelInfo> _channelInfoProvider;
    private readonly IWebPageManager _webPageManager;
    private static readonly LanguageTag defaultCulture = LanguageTag.GB;

    public DealCrudService(IContentService contentRetriever, IValidationState validationState, IWebsiteChannelContext websiteChannelContext, IContentService contentService, ICultureService cultureService, IPageContentValidator pageContentValidator, IInfoProvider<ChannelInfo> channelInfoProvider, IWebPageManagerFactory webPageManagerFactory, IUserInfoProvider userInfoProvider)
    {
        UserInfo user = userInfoProvider.Get("administrator");
        _contentRetriever = contentRetriever;
        _validationState = validationState;
        _websiteChannelContext = websiteChannelContext;
        _cultureService = cultureService;
        _pageContentValidator = pageContentValidator;
        _channelInfoProvider = channelInfoProvider;
        //_webPageManager = webPageManager;
        _webPageManager = webPageManagerFactory.Create(_websiteChannelContext.WebsiteChannelID == 0 ? 1 : _websiteChannelContext.WebsiteChannelID, user.UserID);
    }

    public async Task<ActionResult<IEnumerable<PageOutput<DealPageContent>>>> GetDealPagesAsync(int channelId, string? culture, bool? onlyPublished, CancellationToken cancellationToken)
    {
        //var language = ContentLanguageInfo.Provider.Get()
        //.FirstOrDefault(l => l.ContentLanguageName == culture);

        var channel = await _channelInfoProvider.GetAsync(channelId);

        var retrieveParameters = new RetrievePagesParameters
        {
            LinkedItemsMaxLevel = 5,
            ChannelName = channel.ChannelName,
            LanguageName = culture ?? defaultCulture.Language,
            IsForPreview = false,
        };

        var results = await _contentRetriever.GetPagesAsync<Deal>(
        retrieveParameters,
        query =>
        {
            if (onlyPublished ?? true)
            {
                query.Where(w => w.WhereNotEquals("ContentItemCommonDataLastPublishedWhen", null));
            }
        },
        RetrievalCacheSettings.CacheDisabled,
        cancellationToken);

        var deals = results.ToList() ?? new List<Deal>();
        return deals.Select(MapDealToPageOutput).ToList();
    }
    
    public async Task<ActionResult<PageOutput<DealPageContent>>> GetDealPageAsync(int channelId, Guid pageGUID, string? culture, CancellationToken cancellationToken)
    {
        //var language = ContentLanguageInfo.Provider.Get()
        //.FirstOrDefault(l => l.ContentLanguageName == culture);

        var channel = await _channelInfoProvider.GetAsync(channelId);
        var _retrieveCurrentPage = new RetrievePagesParameters
        {
            LinkedItemsMaxLevel = 1,
            ChannelName = channel.ChannelName,
            LanguageName = culture ?? defaultCulture.Language,
            IsForPreview = false
        };

        var result = await _contentRetriever.GetPagesAsync<Deal>(_retrieveCurrentPage,
            query =>
            {
                query.Where(w => w.WhereEquals("WebPageItemGUID", pageGUID));
                //query.Where(w => w.WhereEquals("ContentItemCommonDataContentLanguageID", language?.ContentLanguageID));
            },
            RetrievalCacheSettings.CacheDisabled,
            cancellationToken);

        var deal = result?.FirstOrDefault();

        if (deal is null)
        {
            throw new NotFoundException();
        }

        return MapDealToPageOutput(deal);
    }

    public async Task<ActionResult<PageOutput<DealPageContent>>> CreatePageAsync(int channelId, int nodeParentID, string? culture, DealPageContent request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return new BadRequestResult();
        }

        var currentCulture = await ValidateAndNormalizeSiteCultureAsync(culture ?? defaultCulture.Language, cancellationToken) ?? defaultCulture;
        var channel = await _channelInfoProvider.GetAsync(channelId);
        var _retrieveCurrentPage = new RetrievePagesParameters
        {
            LinkedItemsMaxLevel = 1,
            ChannelName = channel.ChannelName,
            LanguageName = currentCulture.Language,
            IsForPreview = false
        };

        var parentPage = await _contentRetriever.GetPagesAsync<IWebPageFieldsSource>(_retrieveCurrentPage, query => query
            .Where(w => w.WhereEquals("NodeID", nodeParentID)), RetrievalCacheSettings.CacheDisabled, cancellationToken);

        if (parentPage == null)
        {
            _validationState.AddUnprocessableEntityError(nameof(nodeParentID), ErrorMessage.ParentWithNodeIdNotFound(nodeParentID));
            throw new UnprocessableEntityException();
        }

        //if (!_contentService.IsChildPageTypeAllowed<IPage>(parentPage))
        //{
        //    _validationState.AddUnprocessableEntityError(nameof(nodeParentID), ErrorMessage.ParentPageTypeDoesNotAllowChildPageType(parentPage.GetType(), typeof(IPage)));
        //    throw new UnprocessableEntityException();
        //}

        _pageContentValidator.ValidateAndNormalizePageContent(request);

        //_validationState.ThrowIfThereAreErrors();

        var deal = await MapDealPageContentToDeal(request, nodeParentID, currentCulture.Language, cancellationToken);

        // Create the page in Kentico CMS
        var itemData = new ContentItemData(new Dictionary<string, object>
        {
            { "MetadataTitle", deal.MetadataTitle ?? string.Empty },
            { "MetadataShowTitle", deal.MetadataShowTitle },
            { "MetadataDescription", deal.MetadataDescription ?? string.Empty },
            { "DealContent", deal.DealContent ?? string.Empty },
            { "MetadataTeaserImageAltText", deal.MetadataTeaserImageAltText ?? string.Empty },
            { "ClientName", deal.ClientName ?? string.Empty },
            { "HeroLayout", deal.HeroLayout ?? string.Empty },
            { "HeroTitle", deal.HeroTitle ?? string.Empty },
            { "HeroDescription", deal.HeroDescription ?? string.Empty },
            { "HeroBackgroundImageAltText", deal.HeroBackgroundImageAltText ?? string.Empty },
            { "HeroBackgroundImage_MobileAltText", deal.HeroBackgroundImage_MobileAltText ?? string.Empty },
            { "HeroCallToActionButtonType", deal.HeroCallToActionButtonType ?? string.Empty },
            { "HeroCallToActionButtonText", deal.HeroCallToActionButtonText ?? string.Empty },
            { "ShowSidebar", deal.ShowSidebar },
            { "ShowPrintButton", deal.ShowPrintButton },
            { "ShowCallToAction", deal.ShowCallToAction },
            { "CallToActionDescription", deal.CallToActionDescription ?? string.Empty },
            { "CallToActionButtonText", deal.CallToActionButtonText ?? string.Empty },
            { "CallToActionButtonUrl", deal.CallToActionButtonUrl ?? string.Empty },
            { "ShowSocialMediaButtons", deal.ShowSocialMediaButtons },
            { "SharingOptIn", deal.SharingOptIn },
            { "DealCountry", deal.DealCountry ?? string.Empty },
            { "MetadataOGTitle", deal.MetadataOGTitle ?? string.Empty },
            { "MetadataOGDescription", deal.MetadataOGDescription ?? string.Empty },
            { "MetadataNofollow", deal.MetadataNofollow },
            { "MetadataNoindex", deal.MetadataNoindex },
            { "IncludeInSitemap", deal.IncludeInSitemap },
        });

        var contentItemParameters = new ContentItemParameters(Deal.CONTENT_TYPE_NAME, itemData);
        var createParameters = new CreateWebPageParameters(
            displayName: request.MetadataTitle ?? "New Deal",
            languageName: currentCulture.ToString(),
            contentItemParameters: contentItemParameters)
        {
            ParentWebPageItemID = nodeParentID
        };

        var createdPageId = await _webPageManager.Create(createParameters);

        // Retrieve the created page and return it
        var _retrieveNewPage = new RetrievePagesParameters
        {
            LinkedItemsMaxLevel = 1,
            ChannelName = channel.ChannelName,
            LanguageName = currentCulture.ToString(),
            IsForPreview = false
        };

        var createdResult = await _contentRetriever.GetPagesAsync<Deal>(_retrieveNewPage,
            query => query.Where(w => w.WhereEquals("WebPageItemID", createdPageId)),
            RetrievalCacheSettings.CacheDisabled,
            cancellationToken);

        var createdDeal = createdResult?.FirstOrDefault();
        return createdDeal != null ? MapDealToPageOutput(createdDeal) : new BadRequestResult();
    }

    public async Task<ActionResult<PageOutput<DealPageContent>>> CreatePageCultureVersionAsync(int channelId, Guid pageGUID, string? fromCulture, string? toCulture, DealPageContent request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return new BadRequestResult();
        }
        var channel = await _channelInfoProvider.GetAsync(channelId);
        var _retrieveCurrentPage = new RetrievePagesParameters
        {
            LinkedItemsMaxLevel = 1,
            ChannelName = channel.ChannelName,
            LanguageName = fromCulture,
            IsForPreview = false
        };

        // Get existing page
        var result = await _contentRetriever.GetPagesAsync<Deal>(_retrieveCurrentPage,
            query => query.Where(w => w.WhereEquals("WebPageItemGUID", pageGUID)),
            RetrievalCacheSettings.CacheDisabled,
            cancellationToken);

        var existingDeal = result?.FirstOrDefault();
        if (existingDeal is null)
        {
            return new NotFoundResult();
        }

        // Create new culture version data
        var deal = await MapDealPageContentToDeal(request, 0, toCulture, cancellationToken);

        var itemData = new ContentItemData(new Dictionary<string, object>
        {
            { "MetadataTitle", deal.MetadataTitle ?? string.Empty },
            { "MetadataShowTitle", deal.MetadataShowTitle },
            { "MetadataDescription", deal.MetadataDescription ?? string.Empty },
            { "DealContent", deal.DealContent ?? string.Empty },
            { "MetadataTeaserImageAltText", deal.MetadataTeaserImageAltText ?? string.Empty },
            { "ClientName", deal.ClientName ?? string.Empty },
            { "HeroLayout", deal.HeroLayout ?? string.Empty },
            { "HeroTitle", deal.HeroTitle ?? string.Empty },
            { "HeroDescription", deal.HeroDescription ?? string.Empty },
            { "HeroBackgroundImageAltText", deal.HeroBackgroundImageAltText ?? string.Empty },
            { "HeroBackgroundImage_MobileAltText", deal.HeroBackgroundImage_MobileAltText ?? string.Empty },
            { "HeroCallToActionButtonType", deal.HeroCallToActionButtonType ?? string.Empty },
            { "HeroCallToActionButtonText", deal.HeroCallToActionButtonText ?? string.Empty },
            { "ShowSidebar", deal.ShowSidebar },
            { "ShowPrintButton", deal.ShowPrintButton },
            { "ShowCallToAction", deal.ShowCallToAction },
            { "CallToActionDescription", deal.CallToActionDescription ?? string.Empty },
            { "CallToActionButtonText", deal.CallToActionButtonText ?? string.Empty },
            { "CallToActionButtonUrl", deal.CallToActionButtonUrl ?? string.Empty },
            { "ShowSocialMediaButtons", deal.ShowSocialMediaButtons },
            { "SharingOptIn", deal.SharingOptIn },
            { "DealCountry", deal.DealCountry ?? string.Empty },
            { "MetadataOGTitle", deal.MetadataOGTitle ?? string.Empty },
            { "MetadataOGDescription", deal.MetadataOGDescription ?? string.Empty },
            { "MetadataNofollow", deal.MetadataNofollow },
            { "MetadataNoindex", deal.MetadataNoindex },
            { "IncludeInSitemap", deal.IncludeInSitemap },
        });

        // Create language variant for the existing page
        var createVariantParameters = new CMS.Websites.CreateLanguageVariantParameters(
            existingDeal.SystemFields.WebPageItemID,
            toCulture ?? defaultCulture.Language,
            existingDeal.SystemFields?.WebPageItemName + toCulture ?? "Deal Variant",
            itemData);

        await _webPageManager.TryCreateLanguageVariant(createVariantParameters);

        // Retrieve the created variant and return it
        var _retrieveNewVariant = new RetrievePagesParameters
        {
            LinkedItemsMaxLevel = 1,
            ChannelName = channel.ChannelName,
            LanguageName = toCulture,
            IsForPreview = false
        };

        var variantResult = await _contentRetriever.GetPagesAsync<Deal>(_retrieveNewVariant,
            query => query.Where(w => w.WhereEquals("WebPageItemID", existingDeal.SystemFields.WebPageItemID)),
            RetrievalCacheSettings.CacheDisabled,
            cancellationToken);

        var variantDeal = variantResult?.FirstOrDefault();
        return variantDeal != null ? MapDealToPageOutput(variantDeal) : new BadRequestResult();
    }

    public async Task<ActionResult<PageOutput<DealPageContent>>> UpdatePageAsync(int channelId, Guid pageGUID, string culture, DealPageContent request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return new BadRequestResult();
        }
        var channel = await _channelInfoProvider.GetAsync(channelId);
        var _retrieveCurrentPage = new RetrievePagesParameters
        {
            LinkedItemsMaxLevel = 1,
            ChannelName = channel.ChannelName,
            LanguageName = culture,
            IsForPreview = false
        };

        // Get existing page
        var result = await _contentRetriever.GetPagesAsync<Deal>(_retrieveCurrentPage,
            query => query.Where(w => w.WhereEquals("WebPageItemGUID", pageGUID)),
            RetrievalCacheSettings.CacheDisabled,
            cancellationToken);

        var existingDeal = result?.FirstOrDefault();
        if (existingDeal is null)
        {
            return new NotFoundResult();
        }

        // Update the existing deal with new content
        var updatedDeal = await UpdateDealPageContent(existingDeal, request);

        // Build ContentItemData for the update
        var itemData = new ContentItemData(new Dictionary<string, object>
        {
            { "MetadataTitle", updatedDeal.MetadataTitle ?? string.Empty },
            { "MetadataShowTitle", updatedDeal.MetadataShowTitle },
            { "MetadataDescription", updatedDeal.MetadataDescription ?? string.Empty },
            { "DealContent", updatedDeal.DealContent ?? string.Empty },
            { "MetadataTeaserImageAltText", updatedDeal.MetadataTeaserImageAltText ?? string.Empty },
            { "ClientName", updatedDeal.ClientName ?? string.Empty },
            { "HeroLayout", updatedDeal.HeroLayout ?? string.Empty },
            { "HeroTitle", updatedDeal.HeroTitle ?? string.Empty },
            { "HeroDescription", updatedDeal.HeroDescription ?? string.Empty },
            { "HeroBackgroundImageAltText", updatedDeal.HeroBackgroundImageAltText ?? string.Empty },
            { "HeroBackgroundImage_MobileAltText", updatedDeal.HeroBackgroundImage_MobileAltText ?? string.Empty },
            { "HeroCallToActionButtonType", updatedDeal.HeroCallToActionButtonType ?? string.Empty },
            { "HeroCallToActionButtonText", updatedDeal.HeroCallToActionButtonText ?? string.Empty },
            { "ShowSidebar", updatedDeal.ShowSidebar },
            { "ShowPrintButton", updatedDeal.ShowPrintButton },
            { "ShowCallToAction", updatedDeal.ShowCallToAction },
            { "CallToActionDescription", updatedDeal.CallToActionDescription ?? string.Empty },
            { "CallToActionButtonText", updatedDeal.CallToActionButtonText ?? string.Empty },
            { "CallToActionButtonUrl", updatedDeal.CallToActionButtonUrl ?? string.Empty },
            { "ShowSocialMediaButtons", updatedDeal.ShowSocialMediaButtons },
            { "SharingOptIn", updatedDeal.SharingOptIn },
            { "DealCountry", updatedDeal.DealCountry ?? string.Empty },
            { "MetadataOGTitle", updatedDeal.MetadataOGTitle ?? string.Empty },
            { "MetadataOGDescription", updatedDeal.MetadataOGDescription ?? string.Empty },
            { "MetadataNofollow", updatedDeal.MetadataNofollow },
            { "MetadataNoindex", updatedDeal.MetadataNoindex },
            { "IncludeInSitemap", updatedDeal.IncludeInSitemap },
        });

        // Create a draft, update it, and publish
        await _webPageManager.TryCreateDraft(existingDeal.SystemFields.WebPageItemID, culture);

        // Update the draft with new content
        if (!await _webPageManager.TryUpdateDraft(existingDeal.SystemFields.WebPageItemID, culture, new UpdateDraftData(itemData)))
        {
            throw new Exception("Failed to update page draft");
        }

        // Publish the updated page
        if (!await _webPageManager.TryPublish(existingDeal.SystemFields.WebPageItemID, culture))
        {
            throw new Exception("Failed to publish updated page");
        }

        // Retrieve the updated page and return it
        var updatedResult = await _contentRetriever.GetPagesAsync<Deal>(_retrieveCurrentPage,
            query => query.Where(w => w.WhereEquals("WebPageItemGUID", pageGUID)),
            RetrievalCacheSettings.CacheDisabled,
            cancellationToken);

        var updatedPageDeal = updatedResult?.FirstOrDefault();
        return updatedPageDeal != null ? MapDealToPageOutput(updatedPageDeal) : new BadRequestResult();
    }

    public async Task DeletePageAsync(int channelId, Guid pageGUID, string culture, bool deleteAllCultures, CancellationToken cancellationToken)
    {
        var channel = await _channelInfoProvider.GetAsync(channelId);

        if (deleteAllCultures)
        {
            // Get all available cultures for the channel
            var allCultures = await _cultureService.GetCurrentSiteCulturesAsync(cancellationToken);

            // Delete the page variant for each culture
            foreach (var cult in allCultures)
            {
                var _retrieveCurrentPage = new RetrievePagesParameters
                {
                    LinkedItemsMaxLevel = 1,
                    ChannelName = channel.ChannelName,
                    LanguageName = cult.ToString(),
                    IsForPreview = false
                };

                // Get existing page for this culture
                var result = await _contentRetriever.GetPagesAsync<Deal>(_retrieveCurrentPage,
                    query => query.Where(w => w.WhereEquals("WebPageItemGUID", pageGUID)),
                    RetrievalCacheSettings.CacheDisabled,
                    cancellationToken);

                var existingDeal = result?.FirstOrDefault();
                if (existingDeal is not null)
                {
                    await _webPageManager.Delete(
                        new DeleteWebPageParameters(existingDeal.SystemFields.WebPageItemID, cult.ToString())
                        {
                            Permanently = true  
                        });
                }
            }
        }
        else
        {
            // Delete only the en-gb variant
            var _retrieveCurrentPage = new RetrievePagesParameters
            {
                LinkedItemsMaxLevel = 1,
                ChannelName = channel.ChannelName,
                IsForPreview = false
            };

            // Get existing page
            var result = await _contentRetriever.GetPagesAsync<Deal>(_retrieveCurrentPage,
                query => query.Where(w => w.WhereEquals("WebPageItemGUID", pageGUID)),

                RetrievalCacheSettings.CacheDisabled,
                cancellationToken);

            var existingDeal = result?.FirstOrDefault();
            if (existingDeal is null)
            {
                throw new NotFoundException();
            }

            await _webPageManager.Delete(
                new DeleteWebPageParameters(existingDeal.SystemFields.WebPageItemID, defaultCulture.Language)
                {
                    Permanently = true
                });
        }
    }

    private static async Task<Deal> UpdateDealPageContent(Deal existingDeal, DealPageContent request)
    {
        // Update all deal properties from the request content
        existingDeal.MetadataTitle = request.MetadataTitle;
        existingDeal.MetadataShowTitle = request.ShowTitle;
        existingDeal.MetadataDescription = request.MetadataDescription;
        existingDeal.DealContent = request.DealContent;
        existingDeal.MetadataTeaserImageAltText = request.MetadataTeaserImageAltText;
        existingDeal.ClientName = request.ClientName;
        existingDeal.HeroLayout = request.HeroLayout;
        existingDeal.HeroTitle = request.HeroTitle;
        existingDeal.HeroDescription = request.HeroDescription;
        existingDeal.HeroBackgroundImageAltText = request.HeroBackgroundImageAltText;
        existingDeal.HeroBackgroundImage_MobileAltText = request.HeroBackgroundImage_MobileAltText;
        existingDeal.HeroCallToActionButtonType = request.HeroCallToActionButtonType;
        existingDeal.HeroCallToActionButtonText = request.HeroCallToActionButtonText;
        existingDeal.ShowSidebar = request.ShowSidebar;
        existingDeal.ShowPrintButton = request.ShowPrintButton;
        existingDeal.ShowCallToAction = request.ShowCallToAction;
        existingDeal.CallToActionDescription = request.CallToActionDescription;
        existingDeal.CallToActionButtonText = request.CallToActionButtonText;
        existingDeal.CallToActionButtonUrl = request.CallToActionButtonUrl;
        existingDeal.ShowSocialMediaButtons = request.ShowSocialMediaButtons;
        existingDeal.SharingOptIn = request.SharingOptIn;
        existingDeal.DealCountry = request.DealCountry;
        existingDeal.MetadataOGTitle = request.MetadataOGTitle;
        existingDeal.MetadataOGDescription = request.MetadataOGDescription;
        existingDeal.MetadataNofollow = request.MetadataNofollow;
        existingDeal.MetadataNoindex = request.MetadataNoindex;
        existingDeal.IncludeInSitemap = request.IncludeInSitemap;

        return await Task.FromResult(existingDeal);
    }

    private async Task<Deal> MapDealPageContentToDeal(DealPageContent content, int? nodeParentID, string? culture, CancellationToken cancellationToken)
    {
        return new Deal
        {
            MetadataTitle = content.MetadataTitle,
            MetadataShowTitle = content.ShowTitle,
            MetadataDescription = content.MetadataDescription,
            DealContent = content.DealContent,
            MetadataTeaserImageAltText = content.MetadataTeaserImageAltText,
            ClientName = content.ClientName,
            HeroLayout = content.HeroLayout,
            HeroTitle = content.HeroTitle,
            HeroDescription = content.HeroDescription,
            HeroBackgroundImageAltText = content.HeroBackgroundImageAltText,
            HeroBackgroundImage_MobileAltText = content.HeroBackgroundImage_MobileAltText,
            HeroCallToActionButtonType = content.HeroCallToActionButtonType,
            HeroCallToActionButtonText = content.HeroCallToActionButtonText,
            ShowSidebar = content.ShowSidebar,
            ShowPrintButton = content.ShowPrintButton,
            ShowCallToAction = content.ShowCallToAction,
            CallToActionDescription = content.CallToActionDescription,
            CallToActionButtonText = content.CallToActionButtonText,
            CallToActionButtonUrl = content.CallToActionButtonUrl,
            ShowSocialMediaButtons = content.ShowSocialMediaButtons,
            SharingOptIn = content.SharingOptIn,
            DealCountry = content.DealCountry,
            MetadataOGTitle = content.MetadataOGTitle,
            MetadataOGDescription = content.MetadataOGDescription,
            MetadataNofollow = content.MetadataNofollow,
            MetadataNoindex = content.MetadataNoindex,
            IncludeInSitemap = content.IncludeInSitemap,
        };
    }

    private static PageOutput<DealPageContent> MapDealToPageOutput(Deal deal)
    {
        var language = ContentLanguageInfo.Provider.Get()
        .FirstOrDefault(l => l.ContentLanguageID == deal.SystemFields.WebPageUrlPathContentLanguageID);

        return new PageOutput<DealPageContent>
        {
            General = new PageGeneralOutput
            {
                Name = deal.SystemFields?.WebPageItemName ?? string.Empty,
                Culture = language?.ContentLanguageName ?? "en-gb",
                NodeParentID = deal.SystemFields?.WebPageItemParentID ?? 0,
                NodeID = deal.SystemFields?.WebPageItemParentID ?? 0,
                PageID = deal.SystemFields?.WebPageItemID ?? 0,
                NodeGUID = deal.SystemFields?.ContentItemGUID ?? Guid.Empty,
                PageGUID = deal.SystemFields?.WebPageItemGUID ?? Guid.Empty,
                NodeAliasPath = deal.SystemFields?.WebPageUrlPath ?? string.Empty
            },
            Content = new DealPageContent
            {
                MetadataTitle = deal.MetadataTitle ?? string.Empty,
                ShowTitle = deal.MetadataShowTitle,
                MetadataDescription = deal.MetadataDescription,
                DealContent = deal.DealContent,
                MetadataTeaserImage = deal.MetadataTeaserImage?.ToString() ?? string.Empty,
                MetadataTeaserImageAltText = deal.MetadataTeaserImageAltText,
                Industry = deal.Industry.Select(x => x.Name).ToList(),
                SubIndustry = deal.SubIndustry.Select(x => x.Name).ToList(),
                DealType = deal.DealType.Select(x => x.Name).ToList(),
                DateField = deal.DateField,
                Size = deal.Size.Select(x => x.Name).ToList(),
                MetadataOfficeLocation = deal.MetadataOfficeLocation,
                ClientName = deal.ClientName,
                MetadataCanonical = deal.MetadataCanonical,
                RelatedDeals = [],
                MetadataRssCategory = deal.DealMetadataRssCategory,
                MetadataRssImage = deal.DealMetadataRssImage?.ToString() ?? string.Empty,
                HeroLayout = deal.HeroLayout ?? string.Empty,
                HeroTitle = deal.HeroTitle,
                HeroDescription = deal.HeroDescription,
                HeroBackgroundImage = deal.HeroBackgroundImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url,
                HeroBackgroundImageAltText = deal.HeroBackgroundImageAltText,
                HeroBackgroundImage_Mobile = deal.HeroBackgroundImage_Mobile?.FirstOrDefault()?.ContentAttachmentAsset?.Url,
                HeroBackgroundImage_MobileAltText = deal.HeroBackgroundImage_MobileAltText,
                HeroCallToActionButtonType = deal.HeroCallToActionButtonType,
                HeroCallToActionButtonText = deal.HeroCallToActionButtonText,
                HeroCallToActionButtonUrl = deal.HeroCallToActionButtonUrl?.FirstOrDefault()?.SystemFields?.WebPageUrlPath,
                ShowSidebar = deal.ShowSidebar,
                ShowPrintButton = deal.ShowPrintButton,
                DownloadButtonAttachment = deal.DownloadButtonAttachment?.FirstOrDefault()?.ContentAttachmentAsset?.Url,
                ShowCallToAction = deal.ShowCallToAction,
                CallToActionDescription = deal.CallToActionDescription,
                CallToActionButtonText = deal.CallToActionButtonText,
                CallToActionButtonUrl = deal.CallToActionButtonUrl,
                ShowSocialMediaButtons = deal.ShowSocialMediaButtons,
                SharingOptIn = deal.SharingOptIn,
                DealCountry = deal.DealCountry,
                DisplayedContacts = deal.DisplayedContacts,
                RelevantContactPerson = string.Empty,
                PeopleSort = [],
                MetadataOGTitle = deal.MetadataOGTitle,
                MetadataOGDescription = deal.MetadataOGDescription,
                MetadataOGImage = deal.MetadataOGImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url,
                MetadataOGTwitterImageSizeCheckbox = deal.MetadataOGTwitterImageSizeCheckbox,
                MetadataOGTwitterImage = deal.MetadataOGTwitterImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url,
                MetadataNofollow = deal.MetadataNofollow,
                MetadataNoindex = deal.MetadataNoindex,
                IncludeInSitemap = deal.IncludeInSitemap,
            },
            Metadata = new PageMetadataOutput
            {
                PageTitle = new InheritableString { Value = deal.MetadataTitle },
                PageDescription = new InheritableString { Value = deal.MetadataDescription },
                PageKeywords = new InheritableString { Value = string.Empty },
            },
            Navigation = new PageNavigationOutput
            {
                ShowInMenu = false,
                ShowInSitemap = deal.IncludeInSitemap,
            },
        };
    }

    private async Task<LanguageTag?> ValidateAndNormalizeSiteCultureAsync(string languageTagString, CancellationToken cancellationToken)
    {
        var languageTag = ValidateAndNormalizeCultureAsync(languageTagString);

        if (languageTag == null)
        {
            return null;
        }

        var siteCultures = (await _cultureService.GetCurrentSiteCulturesAsync(cancellationToken)).ToHashSet();

        if (!siteCultures.Contains(languageTag))
        {
            _validationState.AddError("culture", ErrorMessage.DocumentCultureNotAllowedOnSite(languageTag, _websiteChannelContext.WebsiteChannelName, siteCultures));
        }

        return languageTag;
    }

    private LanguageTag? ValidateAndNormalizeCultureAsync(string languageTagString)
    {
        if (LanguageTag.TryParse(languageTagString, out var languageTag))
        {
            return languageTag;
        }

        _validationState.AddError("culture", ErrorMessage.IncorrectLanguageTagFormat(languageTagString));

        return null;
    }
}
