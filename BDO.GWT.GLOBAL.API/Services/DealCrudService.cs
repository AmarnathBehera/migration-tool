using BDO.GWT.Common.Core.Exceptions;
using BDO.GWT.Core;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Helpers;
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
    private readonly IInfoProvider<ChannelInfo> _channelInfoProvider;
    private readonly IWebPageManager _webPageManager;
    private static readonly LanguageTag defaultCulture = LanguageTag.GB;
    private const string DEAL_CONTENT_TYPE = Deal.CONTENT_TYPE_NAME;
    private const string WEB_PAGE_GUID = "WebPageItemGUID";
    private string CHANNEL_NAME = string.Empty;
    private readonly IMappingHelper _mappingHelper;

    public DealCrudService(IContentService contentRetriever, IValidationState validationState, IWebsiteChannelContext websiteChannelContext, ICultureService cultureService, IInfoProvider<ChannelInfo> channelInfoProvider, IWebPageManagerFactory webPageManagerFactory, IUserInfoProvider userInfoProvider, IMappingHelper mappingHelper)
    {
        UserInfo user = userInfoProvider.Get("administrator");
        _contentRetriever = contentRetriever;
        _validationState = validationState;
        _websiteChannelContext = websiteChannelContext;
        _cultureService = cultureService;
        _channelInfoProvider = channelInfoProvider;
        _mappingHelper = mappingHelper;
        _webPageManager = webPageManagerFactory.Create(_websiteChannelContext.WebsiteChannelID == 0 ? 1 : _websiteChannelContext.WebsiteChannelID, user.UserID);
    }

    public async Task<ActionResult<IEnumerable<PageOutput<DealPageContent>>>> GetDealPagesAsync(int channelId, string? culture, bool? onlyPublished, CancellationToken cancellationToken)
    {
        var channel = await _channelInfoProvider.GetAsync(channelId);
        CHANNEL_NAME = channel.ChannelName;

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
        var channel = await _channelInfoProvider.GetAsync(channelId);
        CHANNEL_NAME = channel.ChannelName;

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
                query.Where(w => w.WhereEquals(WEB_PAGE_GUID, pageGUID));
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

        var currentCulture = await ValidateAndNormalizeSiteCultureAsync(channelId, culture ?? defaultCulture.Language, cancellationToken) ?? defaultCulture;

        var channel = await _channelInfoProvider.GetAsync(channelId);
        CHANNEL_NAME = channel.ChannelName;
        var _retrieveCurrentPage = new RetrievePagesParameters
        {
            LinkedItemsMaxLevel = 1,
            ChannelName = channel.ChannelName,
            LanguageName = currentCulture.Language,
            IsForPreview = false
        };

        var parentPage = await _contentRetriever.GetPagesAsync<SectionDeals>(_retrieveCurrentPage, query => query
            .Where(w => w.WhereEquals("WebPageItemID", nodeParentID)), RetrievalCacheSettings.CacheDisabled, cancellationToken);

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

        //_pageContentValidator.ValidateAndNormalizePageContent(request);

        //_validationState.ThrowIfThereAreErrors();

        // The page display name shown in the admin UI
        string pageDisplayName = request.MetadataTitle ?? "New Deal Created by API";

        var itemData = await CreateCMSTreeItem(request, currentCulture.Language, cancellationToken);

        // Creates a page metadata object
        var contentItemParameters = new ContentItemParameters(DEAL_CONTENT_TYPE, itemData);
        var createPageParameters = new CreateWebPageParameters(pageDisplayName, currentCulture.Language, contentItemParameters)
        {
            ParentWebPageItemID = nodeParentID
        };

        // Creates the page in the database
        int newPageId = await _webPageManager.Create(createPageParameters, cancellationToken);

        // Update the draft with new content
        if (!await _webPageManager.TryUpdateDraft(newPageId, currentCulture.Language, new UpdateDraftData(itemData)))
        {
            throw new Exception("Failed to update page draft");
        }

        // Publish the updated page
        if (!await _webPageManager.TryPublish(newPageId, currentCulture.Language))
        {
            throw new Exception("Failed to publish updated page");
        }

        // Retrieve the created page and return it
        var _retrieveNewPage = new RetrievePagesParameters
        {
            LinkedItemsMaxLevel = 1,
            ChannelName = channel.ChannelName,
            LanguageName = currentCulture.Language,
            IsForPreview = false
        };

        var createdResult = await _contentRetriever.GetPagesAsync<Deal>(_retrieveNewPage,
            query => query.Where(w => w.WhereEquals("WebPageItemID", newPageId)),
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
        CHANNEL_NAME = channel.ChannelName;
        var _retrieveCurrentPage = new RetrievePagesParameters
        {
            LinkedItemsMaxLevel = 1,
            ChannelName = channel.ChannelName,
            LanguageName = fromCulture,
            IsForPreview = false
        };

        // Get existing page
        var result = await _contentRetriever.GetPagesAsync<Deal>(_retrieveCurrentPage,
            query => query.Where(w => w.WhereEquals(WEB_PAGE_GUID, pageGUID)),
            RetrievalCacheSettings.CacheDisabled,
            cancellationToken);

        var existingDeal = result?.FirstOrDefault();
        if (existingDeal is null)
        {
            return new NotFoundResult();
        }

        // Create new culture version data
        var itemData = await CreateCMSTreeItem(request, toCulture, cancellationToken);

        // Create language variant for the existing page
        var createVariantParameters = new CMS.Websites.CreateLanguageVariantParameters(
            existingDeal.SystemFields.WebPageItemID,
            toCulture ?? defaultCulture.Language,
            existingDeal.SystemFields?.ContentItemName ?? "Deal culture variant",
            itemData);

        // Creates the language variant in the database
        if (!await _webPageManager.TryCreateLanguageVariant(createVariantParameters))
        {
            throw new Exception("Failed to create language variant");
        }

        // Update the draft with new content
        if (!await _webPageManager.TryUpdateDraft(existingDeal.SystemFields.WebPageItemID, toCulture, new UpdateDraftData(itemData)))
        {
            throw new Exception("Failed to update page draft");
        }

        // Publish the updated page
        if (!await _webPageManager.TryPublish(existingDeal.SystemFields.WebPageItemID, toCulture))
        {
            throw new Exception("Failed to publish updated page");
        }

        // Retrieve the created variant and return it
        var _retrieveNewVariant = new RetrievePagesParameters
        {
            LinkedItemsMaxLevel = 1,
            ChannelName = channel.ChannelName,
            LanguageName = toCulture,
            IsForPreview = false
        };

        var variantResult = await _contentRetriever.GetPagesAsync<Deal>(_retrieveNewVariant,
            query => query.Where(w => w.WhereEquals(WEB_PAGE_GUID, pageGUID)),
            RetrievalCacheSettings.CacheDisabled,
            cancellationToken);

        var variantDeal = variantResult?.FirstOrDefault();
        return variantDeal != null ? MapDealToPageOutput(variantDeal) : new BadRequestResult();
    }

    public async Task<ActionResult<PageOutput<DealPageContent>>> UpdatePageAsync(int channelId, Guid pageGUID, string? culture, DealPageContent request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return new BadRequestResult();
        }
        var channel = await _channelInfoProvider.GetAsync(channelId);
        CHANNEL_NAME = channel.ChannelName;
        var _retrieveCurrentPage = new RetrievePagesParameters
        {
            LinkedItemsMaxLevel = 1,
            ChannelName = channel.ChannelName,
            LanguageName = culture,
            IsForPreview = false
        };

        // Get existing page
        var result = await _contentRetriever.GetPagesAsync<Deal>(_retrieveCurrentPage,
            query => query.Where(w => w.WhereEquals(WEB_PAGE_GUID, pageGUID)),
            RetrievalCacheSettings.CacheDisabled,
            cancellationToken);

        var existingDeal = result?.FirstOrDefault();
        if (existingDeal is null)
        {
            return new NotFoundResult();
        }

        // Build ContentItemData for the update
        var itemData = await CreateCMSTreeItem(request, culture, cancellationToken);

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
            query => query.Where(w => w.WhereEquals(WEB_PAGE_GUID, pageGUID)),
            RetrievalCacheSettings.CacheDisabled,
            cancellationToken);

        var updatedPageDeal = updatedResult?.FirstOrDefault();
        return updatedPageDeal != null ? MapDealToPageOutput(updatedPageDeal) : new BadRequestResult();
    }

    public async Task DeletePageAsync(int channelId, Guid pageGUID, string? culture, bool deleteAllCultures, CancellationToken cancellationToken)
    {
        var channel = await _channelInfoProvider.GetAsync(channelId);
        CHANNEL_NAME = channel.ChannelName;
        if (deleteAllCultures)
        {
            // Get all available cultures for the channel
            var allCultures = await _cultureService.GetSiteCulturesLanguageCodeAsync(channelId, cancellationToken);

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
                    query => query.Where(w => w.WhereEquals(WEB_PAGE_GUID, pageGUID)),
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
                LanguageName = culture ?? defaultCulture.Language,
                IsForPreview = false
            };

            // Get existing page
            var result = await _contentRetriever.GetPagesAsync<Deal>(_retrieveCurrentPage,
                query => query.Where(w => w.WhereEquals(WEB_PAGE_GUID, pageGUID)),

                RetrievalCacheSettings.CacheDisabled,
                cancellationToken);

            var existingDeal = result?.FirstOrDefault();
            if (existingDeal is null)
            {
                throw new NotFoundException();
            }

            await _webPageManager.Delete(
                new DeleteWebPageParameters(existingDeal.SystemFields.WebPageItemID, culture ?? defaultCulture.Language)
                {
                    Permanently = true
                });
        }
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
                MetadataTeaserImage = deal.MetadataTeaserImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url,
                MetadataTeaserImageAltText = deal.MetadataTeaserImageAltText,
                Industry = deal.Industry.Select(x => x.Name).ToList(),
                SubIndustry = deal.SubIndustry.Select(x => x.Name).ToList(),
                DealType = deal.DealType.Select(x => x.Name).ToList(),
                DateField = deal.DateField,
                Size = deal.Size.Select(x => x.Name).ToList(),
                MetadataOfficeLocation = deal.MetadataOfficeLocation,
                ClientName = deal.ClientName,
                MetadataCanonical = deal.MetadataCanonical,
                RelatedDeals = string.Empty,
                MetadataRssCategory = deal.DealMetadataRssCategory,
                MetadataRssImage = deal.DealMetadataRssImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url,
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
                RelevantContactPerson = deal.RelevantContactPersonDeal?.FirstOrDefault()?.SystemFields?.WebPageUrlPath,
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

    private async Task<LanguageTag?> ValidateAndNormalizeSiteCultureAsync(int channelId, string languageTagString, CancellationToken cancellationToken)
    {
        var languageTag = ValidateAndNormalizeCultureAsync(languageTagString);

        if (languageTag == null)
        {
            return null;
        }

        var siteCultures = (await _cultureService.GetSiteCulturesLanguageCodeAsync(channelId, cancellationToken)).ToHashSet();

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

    private async Task<ContentItemData> CreateCMSTreeItem(DealPageContent deal, string? culture, CancellationToken cancellationToken)
    {
        var industryReferences = await BuildReferencesAsync<DealCardIndustry>(deal.Industry, i => i.Name, cancellationToken);
        var subIndustryReferences = await BuildReferencesAsync<DealCardSubIndustry>(deal.SubIndustry, s => s.Name, cancellationToken);
        var dealTypeReferences = await BuildReferencesAsync<DealCardDealType>(deal.DealType, d => d.Name, cancellationToken);
        var sizeReferences = await BuildReferencesAsync<DealCardRange>(deal.Size, r => r.Name, cancellationToken);

        var rssImageReferences = await _mappingHelper.BuildContentAttachmentImageReferences(deal.MetadataRssImage, cancellationToken);
        var downloadButtonReferences = await _mappingHelper.BuildContentAttachmentImageReferences(deal.DownloadButtonAttachment, cancellationToken);
        var ogImageReferences = await _mappingHelper.BuildContentAttachmentImageReferences(deal.MetadataOGImage, cancellationToken);
        var ogTwitterImageReferences = await _mappingHelper.BuildContentAttachmentImageReferences(deal.MetadataOGTwitterImage, cancellationToken);
        var teaserImageReferences = await _mappingHelper.BuildContentAttachmentImageReferences(deal.MetadataTeaserImage, cancellationToken);
        var heroBackgroundImageReferences = await _mappingHelper.BuildContentAttachmentImageReferences(deal.HeroBackgroundImage, cancellationToken);
        var heroBackgroundImageMobileReferences = await _mappingHelper.BuildContentAttachmentImageReferences(deal.HeroBackgroundImage_Mobile, cancellationToken);

        var relevantContactPersonReferences = await _mappingHelper.BuildContentAttachmentPageReferences(deal.RelevantContactPerson, CHANNEL_NAME, culture, cancellationToken);
        var heroCallToActionButtonUrlReferences = await _mappingHelper.BuildContentAttachmentPageReferences(deal.HeroCallToActionButtonUrl, CHANNEL_NAME, culture, cancellationToken);

        return new ContentItemData(new Dictionary<string, object>
        {
            { "Culture", culture ?? string.Empty },
            { "MetadataShowTitle", deal.ShowTitle },
            { "DealContent", deal.DealContent ?? string.Empty },
            { "Industry", industryReferences },
            { "SubIndustry", subIndustryReferences },
            { "DealType", dealTypeReferences },
            { "DateField", deal.DateField ?? DateTime.MinValue },
            { "Size", sizeReferences },
            { "MetadataOfficeLocation", deal.MetadataOfficeLocation ?? string.Empty },
            { "ClientName", deal.ClientName ?? string.Empty },
            { "RelatedDeals", deal.RelatedDeals },
            { "DealMetadataRssCategory", deal.MetadataRssCategory ?? string.Empty },
            { "DealMetadataRssImage", rssImageReferences },
            { "DealShowSocialMediaButtons", deal.ShowSocialMediaButtons },
            { "SharingOptIn", deal.SharingOptIn },
            { "DealCountry", deal.DealCountry ?? string.Empty },
            { "DisplayedContacts", deal.DisplayedContacts ?? string.Empty },
            { "RelevantContactPersonDeal", relevantContactPersonReferences },
            { "ShowCallToAction", deal.ShowCallToAction },
            { "CallToActionDescription", deal.CallToActionDescription ?? string.Empty },
            { "CallToActionButtonText", deal.CallToActionButtonText ?? string.Empty },
            { "CallToActionButtonUrl", deal.CallToActionButtonUrl ?? string.Empty },
            { "ShowSidebar", deal.ShowSidebar },
            { "ShowPrintButton", deal.ShowPrintButton },
            { "DownloadButtonAttachment", downloadButtonReferences },
            { "ShowSocialMediaButtons", deal.ShowSocialMediaButtons },
            { "MetadataOGTitle", deal.MetadataOGTitle ?? string.Empty },
            { "MetadataOGDescription", deal.MetadataOGDescription ?? string.Empty },
            { "MetadataOGImage", ogImageReferences },
            { "MetadataOGTwitterImageSizeCheckbox", deal.MetadataOGTwitterImageSizeCheckbox },
            { "MetadataOGTwitterImage", ogTwitterImageReferences },
            { "MetadataNofollow", deal.MetadataNofollow },
            { "MetadataNoindex", deal.MetadataNoindex },
            { "IncludeInSitemap", deal.IncludeInSitemap },
            { "MetadataTitle", deal.MetadataTitle ?? string.Empty },
            { "MetadataDescription", deal.MetadataDescription ?? string.Empty },
            { "MetadataTeaserImage", teaserImageReferences },
            { "MetadataTeaserImageAltText", deal.MetadataTeaserImageAltText ?? string.Empty },
            { "MetadataCanonical", deal.MetadataCanonical ?? string.Empty },
            { "HeroLayout", deal.HeroLayout ?? string.Empty },
            { "HeroTitle", deal.HeroTitle ?? string.Empty },
            { "HeroDescription", deal.HeroDescription ?? string.Empty },
            { "HeroBackgroundImage", heroBackgroundImageReferences },
            { "HeroBackgroundImageAltText", deal.HeroBackgroundImageAltText ?? string.Empty },
            { "HeroBackgroundImage_Mobile", heroBackgroundImageMobileReferences },
            { "HeroBackgroundImage_MobileAltText", deal.HeroBackgroundImage_MobileAltText ?? string.Empty },
            { "HeroCallToActionButtonType", deal.HeroCallToActionButtonType ?? string.Empty },
            { "HeroCallToActionButtonText", deal.HeroCallToActionButtonText ?? string.Empty },
            { "HeroCallToActionButtonUrl", heroCallToActionButtonUrlReferences },
            { "PageTitle", deal.MetadataTitle ?? string.Empty },
            { "PageDescription", deal.MetadataDescription ?? string.Empty },
            { "PageKeywords", string.Empty },
        });
    }

    /// <summary>
    /// Selects the content items of type <typeparamref name="T"/> whose name matches the supplied
    /// identifiers and returns them as a list of <see cref="ContentItemReference"/>. Names that do
    /// not match any existing item are ignored.
    /// </summary>
    /// <typeparam name="T">The content type to query (e.g. DealCardIndustry, DealCardRange).</typeparam>
    /// <param name="identifiers">The names to match.</param>
    /// <param name="nameSelector">Selects the name value used for matching from a content item.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    private async Task<List<ContentItemReference>> BuildReferencesAsync<T>(
        IEnumerable<string>? identifiers,
        Func<T, string> nameSelector,
        CancellationToken cancellationToken)
        where T : class, IWebPageFieldsSource
    {
        var names = identifiers?
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name.Trim())
            .ToList() ?? new List<string>();

        if (names.Count == 0)
        {
            return new List<ContentItemReference>();
        }

        var items = await _contentRetriever.GetItemsAsync<T>(
            builder => builder.Parameters(p => p.Where(w => w.WhereIn("Name", names))),
            cancellationToken);

        return items
            .Where(item => names.Contains(nameSelector(item), StringComparer.OrdinalIgnoreCase))
            .Select(item => new ContentItemReference { Identifier = item.SystemFields.ContentItemGUID })
            .ToList();
    }
}
