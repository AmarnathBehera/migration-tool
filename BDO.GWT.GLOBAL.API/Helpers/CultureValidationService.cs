using CMS.Localization;
using System;
using CMS.ContentEngine;
using CMS.DataEngine;

namespace BDO.GWT.GLOBAL.API.Helpers
{
    public interface ICultureValidationService
    {
        public List<string> GetConfiguredLanguageCodes();

        public bool IsValidCulture(string cultureCode);
        
    }


    public class CultureValidationService : ICultureValidationService
    {
        private readonly IInfoProvider<ContentLanguageInfo> _contentLanguageInfoProvider;

        public CultureValidationService(IInfoProvider<ContentLanguageInfo> contentLanguageInfoProvider)
        {
            _contentLanguageInfoProvider = contentLanguageInfoProvider;
        }

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
    }
   
}
