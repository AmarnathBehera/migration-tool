using BDO.GWT.Common.Core.Exceptions;
using BDO.GWT.Core;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.GLOBAL.API.Data;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;

namespace BDO.GWT.GLOBAL.API.Services;

public interface IDealCrudService
{
    Task<ActionResult<IEnumerable<PageOutput<DealPageContent>>>> GetDealPagesAsync(string? culture, bool? onlyPublished, CancellationToken cancellationToken);
    Task<ActionResult<PageOutput<DealPageContent>>> GetDealPageAsync(Guid pageGUID, CancellationToken cancellationToken);
    Task<ActionResult<PageOutput<DealPageContent>>> CreatePageAsync(int nodeParentID, string? culture, DealPageContent request, CancellationToken cancellationToken);
    Task<ActionResult<PageOutput<DealPageContent>>> CreatePageCultureVersionAsync(Guid pageGUID, string? culture, DealPageContent request, CancellationToken cancellationToken);
    Task<ActionResult<PageOutput<DealPageContent>>> UpdatePageAsync(Guid pageGUID, DealPageContent request, CancellationToken cancellationToken);
    Task DeletePageAsync(Guid pageGUID, bool deleteAllCultures, CancellationToken cancellationToken);
}

public class DealCrudService : IDealCrudService
{
    private readonly IContentService _contentRetriever;
    private readonly IValidationState _validationState;
    private readonly IWebsiteChannelContext _websiteChannelContext;
    private readonly ICultureService _cultureService;
    private readonly IPageContentValidator _pageContentValidator;

    private static readonly RetrievePagesParameters _retrieveParameters = new()
    {
        LinkedItemsMaxLevel = 5
    };
    private static readonly RetrievePagesParameters _retrieveCurrentPage = new()
    {
        LinkedItemsMaxLevel = 1
    };

    public DealCrudService(IContentService contentRetriever, IValidationState validationState, IWebsiteChannelContext websiteChannelContext, IContentService contentService, ICultureService cultureService, IPageContentValidator pageContentValidator)
    {
        _contentRetriever = contentRetriever;
        _validationState = validationState;
        _websiteChannelContext = websiteChannelContext;
        _cultureService = cultureService;
        _pageContentValidator = pageContentValidator;
    }

    public async Task<ActionResult<IEnumerable<PageOutput<DealPageContent>>>> GetDealPagesAsync(string? culture, bool? onlyPublished, CancellationToken cancellationToken)
    {
        var retrieveParameters = new RetrievePagesParameters
        {
            LinkedItemsMaxLevel = 5,
            ChannelName = _websiteChannelContext.WebsiteChannelName ?? string.Empty,
            LanguageName = string.IsNullOrEmpty(culture) ? "en-gb" : culture,
            IsForPreview = false
        };

        var results = await _contentRetriever.GetPagesAsync<Deal>(
        retrieveParameters,
        query =>
        {
            if (!string.IsNullOrEmpty(culture))
            {
                query.Where(w => w.WhereEquals("WebPageItemLanguage", culture));
            }
            if (onlyPublished ?? true)
            {
                query.Where(w => w.WhereNotEmpty("WebPageItemPublishedVersionHistoryID"));
            }
        },
        RetrievalCacheSettings.CacheDisabled,
        cancellationToken);

        var deals = results.ToList() ?? new List<Deal>();
        return deals.Select(MapDealToPageOutput).ToList();
    }
    
    public async Task<ActionResult<PageOutput<DealPageContent>>> GetDealPageAsync(Guid pageGUID, CancellationToken cancellationToken)
    {
        var result = await _contentRetriever.GetPagesAsync<Deal>(_retrieveCurrentPage,
            query => query.Where(w => w.WhereEquals("WebPageItemGUID", pageGUID)),
            RetrievalCacheSettings.CacheDisabled,
            cancellationToken);

        var deal = result?.FirstOrDefault();

        if (deal is null)
        {
            throw new NotFoundException();
        }

        return MapDealToPageOutput(deal);
    }

    public async Task<ActionResult<PageOutput<DealPageContent>>> CreatePageAsync(int nodeParentID, string? culture, DealPageContent request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return new BadRequestResult();
        }

        var currentCulture = await ValidateAndNormalizeSiteCultureAsync(culture ?? "en-gb", cancellationToken) ?? LanguageTag.GB;

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

        // For now, return the mapped output
        // TODO: Implement actual page creation in Kentico CMS
        return MapDealToPageOutput(deal);
    }

    public async Task<ActionResult<PageOutput<DealPageContent>>> CreatePageCultureVersionAsync(Guid pageGUID, string? culture, DealPageContent request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return new BadRequestResult();
        }

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

        // Create new culture version
        var deal = await MapDealPageContentToDeal(request, 0, culture, cancellationToken);

        // TODO: Implement actual page creation in Kentico CMS for new culture version
        return MapDealToPageOutput(deal);
    }

    public async Task<ActionResult<PageOutput<DealPageContent>>> UpdatePageAsync(Guid pageGUID, DealPageContent request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return new BadRequestResult();
        }

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
        var updatedDeal = await MapDealPageContentToDeal(request, 0, "", cancellationToken);

        // TODO: Implement actual page update in Kentico CMS
        return MapDealToPageOutput(updatedDeal);
    }

    public async Task DeletePageAsync(Guid pageGUID, bool deleteAllCultures, CancellationToken cancellationToken)
    {
        // TODO: Implement actual page deletion in Kentico CMS
        // This should delete the page with the given GUID
        // If deleteAllCultures is true, delete all culture versions of the page
        await Task.CompletedTask;
    }

    private async Task<Deal> MapDealPageContentToDeal(DealPageContent content, int nodeParentID, string culture, CancellationToken cancellationToken)
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
        return new PageOutput<DealPageContent>
        {
            General = new PageGeneralOutput
            {
                Name = deal.SystemFields?.WebPageItemName ?? string.Empty,
                Culture = string.Empty,
                NodeParentID = null,
                NodeID = deal.SystemFields?.WebPageItemParentID ?? 0,
                PageID = deal.SystemFields?.WebPageItemID ?? 0,
                NodeGUID = Guid.Empty,
                PageGUID = deal.SystemFields?.WebPageItemGUID ?? Guid.Empty,
                NodeAliasPath = string.Empty,
            },
            Content = new DealPageContent
            {
                MetadataTitle = deal.MetadataTitle ?? string.Empty,
                ShowTitle = deal.MetadataShowTitle,
                MetadataDescription = deal.MetadataDescription,
                DealContent = deal.DealContent,
                MetadataTeaserImage = null,
                MetadataTeaserImageAltText = deal.MetadataTeaserImageAltText,
                Industry = [],
                SubIndustry = [],
                DealType = [],
                DateField = null,
                Size = null,
                MetadataOfficeLocation = [],
                ClientName = deal.ClientName,
                MetadataCanonical = null,
                RelatedDeals = [],
                MetadataRssCategory = [],
                MetadataRssImage = null,
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
                DisplayedContacts = 0,
                RelevantContactPerson = [],
                PeopleSort = [],
                MetadataOGTitle = deal.MetadataOGTitle,
                MetadataOGDescription = deal.MetadataOGDescription,
                MetadataOGImage = deal.MetadataOGImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url,
                MetadataOGTwitterImageSizeCheckbox = false,
                MetadataOGTwitterImage = deal.MetadataOGTwitterImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url,
                MetadataNofollow = deal.MetadataNofollow,
                MetadataNoindex = deal.MetadataNoindex,
                IncludeInSitemap = deal.IncludeInSitemap,
                PublishFrom = null,
                PublishTo = null,
                RedirectOnUnpublish = null,
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
