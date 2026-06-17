using CMS.Core;
using CMS.DataEngine;
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
        private const string VerifyUrl = "https://www.google.com/recaptcha/api/siteverify";

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

        public async Task<bool> ValidateAsync(
            bool isV3,
            decimal threshold,
            string recaptchaResponse,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(recaptchaResponse))
            {
                return false;
            }

            var privateKey = GetRecaptchaPrivateKey(isV3);

            if (string.IsNullOrWhiteSpace(privateKey))
            {
                return false;
            }

            var result = await CallGoogleVerifyApiAsync(privateKey, recaptchaResponse, cancellationToken);

            if (result is null || !result.Success)
            {
                return false;
            }

            return !isV3 || result.Score >= (double)threshold;
        }

        private string? GetRecaptchaPrivateKey(bool isV3)
        {
            int channelId = _websiteChannelContext.WebsiteChannelID;
            string expectedVersion = isV3 ? RecaptchaVersionV3 : RecaptchaVersionV2;

            // Filter by BOTH channel ID and the correct reCAPTCHA version
            var captchaSettings = _captchaSettingsProvider
                .Get()
                .WhereEquals(
                    nameof(_websiteChannelContext.WebsiteChannelID),
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

        private async Task<RecaptchaVerifyResponse?> CallGoogleVerifyApiAsync(
            string secret,
            string token,
            CancellationToken cancellationToken)
        {
            var client = _httpClientFactory.CreateClient(HttpClientName);

            var formContent = new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("secret", secret),
                new KeyValuePair<string, string>("response", token)
            ]);

            using var httpResponse = await client.PostAsync(VerifyUrl, formContent, cancellationToken);

            if (!httpResponse.IsSuccessStatusCode)
            {
                return null;
            }

            var json = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

            return JsonSerializer.Deserialize<RecaptchaVerifyResponse>(json);
        }

        private sealed class RecaptchaVerifyResponse
        {
            [JsonPropertyName("success")]
            public bool Success { get; set; }

            [JsonPropertyName("score")]
            public double Score { get; set; }

            [JsonPropertyName("action")]
            public string? Action { get; set; }

            [JsonPropertyName("error-codes")]
            public List<string>? ErrorCodes { get; set; }
        }
    }
}