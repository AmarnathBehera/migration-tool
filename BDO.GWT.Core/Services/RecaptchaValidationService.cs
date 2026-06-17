using CMS.Core;
using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Websites;
using CMS.Websites.Routing;
using System.Text.Json;
using System.Text.Json.Serialization;

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
        private const string HttpClientName = "Recaptcha";

        // These must match whatever values Kentico stores in WebsiteCaptchaSettingsReCaptchaVersion
        private const string RecaptchaVersionV2 = "RecaptchaV2";
        private const string RecaptchaVersionV3 = "RecaptchaV3";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IWebsiteChannelContext _websiteChannelContext;
        private readonly IInfoProvider<WebsiteCaptchaSettingsInfo> _captchaSettingsProvider;

        public RecaptchaValidationService(
            IHttpClientFactory httpClientFactory,
            IWebsiteChannelContext websiteChannelContext,
            IInfoProvider<WebsiteCaptchaSettingsInfo> captchaSettingsProvider)
        {
            _httpClientFactory = httpClientFactory;
            _websiteChannelContext = websiteChannelContext;
            _captchaSettingsProvider = captchaSettingsProvider;
        }

        public async Task<bool> ValidateAsync( bool isV3, decimal threshold, string recaptchaResponse, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(recaptchaResponse))
            {
                return false;
            }
            var response = await new RecaptchaValidator
            {
                PrivateKey = GetRecaptchaPrivateKey(isV3),
                Response = recaptchaResponse,
            }.Validate();

            return !isV3 || response.Score >= (double)threshold;

        }

        private string? GetRecaptchaPrivateKey(bool isV3)
        {
            int channelId = _websiteChannelContext.WebsiteChannelID;
            string expectedVersion = isV3 ? RecaptchaVersionV3 : RecaptchaVersionV2;

            // Filter by BOTH channel ID and the correct reCAPTCHA version
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
            {
                return null;
            }

            return captchaSettings.WebsiteCaptchaSettingsReCaptchaSecretKey;
        }

      
    }
}