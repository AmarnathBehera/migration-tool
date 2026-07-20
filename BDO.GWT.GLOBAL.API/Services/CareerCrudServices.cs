using BDO.GWT.Common.Core.Exceptions;
using BDO.GWT.Core;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.Domain.Core.Entities;
using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Extensions;
using BDO.GWT.GLOBAL.API.Helpers;
using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Membership;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;

namespace BDO.GWT.GLOBAL.API.Services
{

    public interface ICareerCrudService
    {
        Task<ActionResult<IEnumerable<PageOutput<CareerPageContent>>>> GetCareerPagesAsync(int channelId, string? culture, bool? onlyPublished, CancellationToken cancellationToken);
        Task<ActionResult<PageOutput<CareerPageContent>>> GetCareerPageAsync(int channelId, Guid pageGUID, string? culture, CancellationToken cancellationToken);
        Task<ActionResult<PageOutput<CareerPageContent>>> CreateCareerPageAsync(int channelId, int nodeParentID, string? culture, CareerPageContent request, CancellationToken cancellationToken);
        Task<ActionResult<PageOutput<CareerPageContent>>> CreateCareerPageCultureVersionAsync(int channelId, Guid pageGUID, string? fromCulture, string? toCulture, CareerPageContent request, CancellationToken cancellationToken);
        Task<ActionResult<PageOutput<CareerPageContent>>> UpdateCareerPageAsync(int channelId, Guid pageGUID, string? culture, CareerPageContent request, CancellationToken cancellationToken);
        Task DeleteCareerPageAsync(int channelId, Guid pageGUID, string? culture, bool deleteAllCultures, CancellationToken cancellationToken);
    }
    public class CareerCrudService : ICareerCrudService
    {
        private readonly IContentService _contentRetriever;
        private readonly IValidationState _validationState;
        private readonly IWebsiteChannelContext _websiteChannelContext;
        private readonly ICultureService _cultureService;
        private readonly IInfoProvider<ChannelInfo> _channelInfoProvider;
        private readonly IWebPageManager _webPageManager;
        private readonly IMappingHelper _mappingHelper;
        private readonly IPageContentValidator _pageContentValidator;
        private readonly ICultureValidationService _cultureValidationService;
        private static readonly LanguageTag defaultCulture = LanguageTag.GB;
        private const string CAREER_CONTENT_TYPE = Career.CONTENT_TYPE_NAME;
        private const string WEB_PAGE_GUID = "WebPageItemGUID";
        private string CHANNEL_NAME = string.Empty;
        public CareerCrudService( IContentService contentRetriever, IValidationState validationState, 
            IWebsiteChannelContext websiteChannelContext, ICultureService cultureService, 
            IInfoProvider<ChannelInfo> channelInfoProvider,  
            IMappingHelper mappingHelper, IPageContentValidator pageContentValidator, ICultureValidationService cultureValidationService
            , IWebPageManagerFactory webPageManagerFactory, IUserInfoProvider userInfoProvider)
        {
            UserInfo user = userInfoProvider.Get("administrator");
            _contentRetriever = contentRetriever;
            _validationState = validationState;
            _websiteChannelContext = websiteChannelContext;
            _cultureService = cultureService;
            _channelInfoProvider = channelInfoProvider;
            _webPageManager = webPageManagerFactory.Create(_websiteChannelContext.WebsiteChannelID == 0 ? 1 : _websiteChannelContext.WebsiteChannelID, user.UserID);
            _mappingHelper = mappingHelper;
            _pageContentValidator = pageContentValidator;
            _cultureValidationService = cultureValidationService;

        }
        public Task<ActionResult<PageOutput<CareerPageContent>>> CreateCareerPageAsync(int channelId, int nodeParentID, string? culture, CareerPageContent request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<ActionResult<PageOutput<CareerPageContent>>> CreateCareerPageCultureVersionAsync(int channelId, Guid pageGUID, string? fromCulture, string? toCulture, CareerPageContent request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task DeleteCareerPageAsync(int channelId, Guid pageGUID, string? culture, bool deleteAllCultures, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }


        public async Task<ActionResult<PageOutput<CareerPageContent>>> GetCareerPageAsync(int channelId, Guid pageGUID, string? culture, CancellationToken cancellationToken)
        {
            var currentCulture = await _cultureValidationService.ValidateAndNormalizeSiteCultureAsync(channelId, 
                                                     culture ?? defaultCulture.Language, cancellationToken) ?? defaultCulture;

            if (string.IsNullOrEmpty(currentCulture.Locale))
            {
                throw new NotFoundException("Invalid culture specified");
            }

            var channel = await _channelInfoProvider.GetAsync(channelId, cancellationToken);
            if (string.IsNullOrEmpty(channel?.ChannelName))
            {
                throw new NotFoundException("Channel not found");
            }

            var _retrieveCurrentPage = new RetrievePagesParameters
            {
                LinkedItemsMaxLevel = 1,
                ChannelName = channel.ChannelName,
                LanguageName = currentCulture.Locale,
                IsForPreview = false
            };

            var result = await _contentRetriever.GetPagesAsync<Career>(_retrieveCurrentPage,
                query =>
                {
                    query.Where(w => w.WhereEquals(WEB_PAGE_GUID, pageGUID));
                },
                RetrievalCacheSettings.CacheDisabled,
                cancellationToken);

            var careerPage = result?.FirstOrDefault();

            if (careerPage is null)
            {
                throw new NotFoundException();
            }

            return new ActionResult<PageOutput<CareerPageContent>>(MapCareerToPageOutput(careerPage));
        }

        public Task<ActionResult<IEnumerable<PageOutput<CareerPageContent>>>> GetCareerPagesAsync(int channelId, string? culture, bool? onlyPublished, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<ActionResult<PageOutput<CareerPageContent>>> UpdateCareerPageAsync(int channelId, Guid pageGUID, string? culture, CareerPageContent request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private static PageOutput<CareerPageContent> MapCareerToPageOutput(Career career)
        {
            var language = ContentLanguageInfo.Provider.Get()
            .FirstOrDefault(l => l.ContentLanguageID == career.SystemFields.WebPageUrlPathContentLanguageID);

            return new PageOutput<CareerPageContent>
            {
                General = new PageGeneralOutput
                {
                    Name = career.SystemFields?.WebPageItemName ?? string.Empty,
                    Culture = language?.ContentLanguageName ?? "en-gb",
                    NodeParentID = career.SystemFields?.WebPageItemParentID ?? 0,
                    NodeID = career.SystemFields?.WebPageItemParentID ?? 0,
                    PageID = career.SystemFields?.WebPageItemID ?? 0,
                    NodeGUID = career.SystemFields?.ContentItemGUID ?? Guid.Empty,
                    PageGUID = career.SystemFields?.WebPageItemGUID ?? Guid.Empty,
                    NodeAliasPath = career.SystemFields?.WebPageUrlPath ?? string.Empty
                },
                Content = new CareerPageContent
                {
                    MetadataTitle = career.MetadataTitle ?? string.Empty,
                    MetadataDescription = career.MetadataDescription,
                    MetadataTeaserImage = career.MetadataTeaserImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url,
                    MetadataTeaserImageAltText = career.MetadataTeaserImageAltText,
                    MetadataCanonical = career.MetadataCanonical,
                    HeroLayout = career.HeroLayout ?? string.Empty,
                    HeroTitle = career.HeroTitle,
                    HeroDescription = career.HeroDescription,
                    HeroBackgroundImage = career.HeroBackgroundImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url,
                    HeroBackgroundImageAltText = career.HeroBackgroundImageAltText,
                    HeroBackgroundImage_Mobile = career.HeroBackgroundImage_Mobile?.FirstOrDefault()?.ContentAttachmentAsset?.Url,
                    HeroBackgroundImage_MobileAltText = career.HeroBackgroundImage_MobileAltText,
                    HeroCallToActionButtonType = career.HeroCallToActionButtonType,
                    HeroCallToActionButtonText = career.HeroCallToActionButtonText,
                    HeroCallToActionButtonUrl = career.HeroCallToActionButtonUrl?.FirstOrDefault()?.SystemFields?.WebPageUrlPath,
                    ShowSidebar = career.ShowSidebar,
                    ShowPrintButton = career.ShowPrintButton,
                    DownloadButtonAttachment = career.DownloadButtonAttachment?.FirstOrDefault()?.ContentAttachmentAsset?.Url,
                    ShowCallToAction = career.ShowCallToAction,
                    CallToActionDescription = career.CallToActionDescription,
                    CallToActionButtonText = career.CallToActionButtonText,
                    CallToActionButtonUrl = career.CallToActionButtonUrl,
                    ShowSocialMediaButtons = career.ShowSocialMediaButtons,              
                    MetadataOGTitle = career.MetadataOGTitle,
                    MetadataOGDescription = career.MetadataOGDescription,
                    MetadataOGImage = career.MetadataOGImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url,
                    MetadataOGTwitterImageSizeCheckbox = career.MetadataOGTwitterImageSizeCheckbox,
                    MetadataOGTwitterImage = career.MetadataOGTwitterImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url,
                    MetadataNofollow = career.MetadataNofollow,
                    MetadataNoindex = career.MetadataNoindex,
                    IncludeInSitemap = career.IncludeInSitemap,
                },
                Metadata = new PageMetadataOutput
                {
                    PageTitle = new InheritableString { Value = career.MetadataTitle },
                    PageDescription = new InheritableString { Value = career.MetadataDescription },
                    PageKeywords = new InheritableString { Value = string.Empty },
                },
                Navigation = new PageNavigationOutput
                {
                    ShowInMenu = false,
                    ShowInSitemap = career.IncludeInSitemap,
                },
            };
        }
    }
}
