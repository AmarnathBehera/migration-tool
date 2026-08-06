using CMS.Core;
using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Websites;
using CMS.Websites.Routing;

namespace BDO.GWT.Core.Services
{
    public interface IRecaptchaValidationService
    {
        Task<bool> ValidateAsync(
            bool isV3,
            decimal threshold,
            string recaptchaResponse,
            CancellationToken cancellationToken);
    }

    public class RecaptchaValidationService : IRecaptchaValidationService
    {
        private const string RecaptchaVersionV2 = "RecaptchaV2";
        private const string RecaptchaVersionV3 = "RecaptchaV3";

        private readonly IWebsiteChannelContext _websiteChannelContext;
        private readonly IInfoProvider<WebsiteCaptchaSettingsInfo> _captchaSettingsProvider;

        public RecaptchaValidationService(
            IWebsiteChannelContext websiteChannelContext,
            IInfoProvider<WebsiteCaptchaSettingsInfo> captchaSettingsProvider)
        {
            _websiteChannelContext = websiteChannelContext;
            _captchaSettingsProvider = captchaSettingsProvider;
        }

        public async Task<bool> ValidateAsync(
            bool isV3,
            decimal threshold,
            string recaptchaResponse,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(recaptchaResponse))
                return false;

            string? privateKey = GetRecaptchaPrivateKey(isV3);

            if (string.IsNullOrWhiteSpace(privateKey))
                return false;

            var response = await new RecaptchaValidator
            {
                PrivateKey = privateKey,
                Response = recaptchaResponse,
            }.Validate();

            if (response is null)
                return false;

            return !isV3 || response.Score >= (double)threshold;
        }

        protected virtual string? GetRecaptchaPrivateKey(bool isV3)
        {
            int channelId = _websiteChannelContext.WebsiteChannelID;
            int expectedVersion = isV3 ? 1 : 0; // replace with actual enum values

            var captchaSettings = _captchaSettingsProvider
                .Get()
                .WhereEquals(
                    nameof(WebsiteCaptchaSettingsInfo.WebsiteCaptchaSettingsWebsiteChannelID),
                    channelId)
                .WhereEquals(
                    nameof(WebsiteCaptchaSettingsInfo.WebsiteCaptchaSettingsReCaptchaVersion),
                    expectedVersion)
                .TopN(1)
                .FirstOrDefault();

            if (captchaSettings is null)
                return null;

            return captchaSettings.WebsiteCaptchaSettingsReCaptchaSecretKey;
        }
    }
}