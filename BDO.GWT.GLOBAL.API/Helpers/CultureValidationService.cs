using BDO.GWT.Core;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.GLOBAL.API.Services;
using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Localization;
using CMS.Websites.Routing;
using System;

namespace BDO.GWT.GLOBAL.API.Helpers
{
    public interface ICultureValidationService
    {
        public List<string> GetConfiguredLanguageCodes();

        public bool IsValidCulture(string cultureCode);

        public Task<LanguageTag?> ValidateAndNormalizeSiteCultureAsync(int channelId, string languageTagString, CancellationToken cancellationToken);
    }
    
    public class CultureValidationService(IInfoProvider<ContentLanguageInfo> contentLanguageInfoProvider,
    ICultureService cultureService, IInfoProvider<ChannelInfo> channelInfoProvider,
      IValidationState validationState, IWebsiteChannelContext websiteChannelContext) : ICultureValidationService
    {
        private readonly IInfoProvider<ContentLanguageInfo> _contentLanguageInfoProvider = contentLanguageInfoProvider;
        private readonly ICultureService _cultureService = cultureService;
        private readonly IInfoProvider<ChannelInfo> _channelInfoProvider = channelInfoProvider;
       private readonly IValidationState _validationState = validationState;
      private readonly IWebsiteChannelContext _websiteChannelContext = websiteChannelContext;

        public List<string> GetConfiguredLanguageCodes()
        {
            return _contentLanguageInfoProvider 
                .Get()
                .Column(nameof(ContentLanguageInfo.ContentLanguageName))
                .GetListResult<string>()
                .ToList();
        }

        public bool IsValidCulture(string cultureCode)
        {
            if (string.IsNullOrWhiteSpace(cultureCode))
            {
                return false;
            }
            var configuredLanguageCodes = GetConfiguredLanguageCodes();
            return configuredLanguageCodes.Contains(cultureCode, StringComparer.OrdinalIgnoreCase);
        }

        public async Task<LanguageTag?> ValidateAndNormalizeSiteCultureAsync(int channelId, string languageTagString, CancellationToken cancellationToken)
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
    }
   
}
