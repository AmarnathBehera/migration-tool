using BDO.GWT.Common.Core.Constants;
using BDO.GWT.Common.Helpers;
using BDO.GWT.Domain.Core.Entities;
using CMS.DataEngine;
using CMS.OnlineForms;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace BDO.GWT.Web.Core.LiveSite.Services;

public interface IFormSubmissionSubscriptionsService
{
    Task InvokeRegisteredEndpointsAsync(BizFormItem formItem, FormEntry formEntry, CancellationToken cancellationToken);
}

public class FormSubmissionSubscriptionsService : IFormSubmissionSubscriptionsService
{
    private static readonly TimeSpan EndpointTimeout = TimeSpan.FromSeconds(30);

    private readonly ILogger<FormSubmissionSubscriptionsService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IInfoProvider<FormSubmissionSubscriptionsInfo> _subscriptionProvider;
    private readonly ISecretsEncryptionHelper _secretsEncryptionHelper;

    public FormSubmissionSubscriptionsService(
        ILogger<FormSubmissionSubscriptionsService> logger,
        IHttpClientFactory httpClientFactory,
        ISecretsEncryptionHelper secretsEncryptionHelper,
        IInfoProvider<FormSubmissionSubscriptionsInfo> subscriptionProvider)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _subscriptionProvider = subscriptionProvider;
        _secretsEncryptionHelper = secretsEncryptionHelper;
    }

    public async Task InvokeRegisteredEndpointsAsync(BizFormItem formItem, FormEntry formEntry, CancellationToken cancellationToken)
    {
        if (formItem == null)
            return;

        string formCodeName;
        try
        {
            formCodeName = formItem.BizFormInfo?.FormName ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not resolve BizFormInfo for the submitted form item; continuing with an empty form code name.");
            formCodeName = string.Empty;
        }

        var subscriptions = _subscriptionProvider
            .Get()
            .GetEnumerableTypedResult()
            .Where(s => TryGetFormNames(s).Contains(formCodeName, StringComparer.OrdinalIgnoreCase))
            .ToList();

        if (subscriptions.Count == 0)
        {
            // No endpoint registered for this form — nothing to send.
            return;
        }

        using var httpClient = _httpClientFactory.CreateClient();

        foreach (var subscription in subscriptions)
        {
            if (string.IsNullOrWhiteSpace(subscription.Endpoint) || string.IsNullOrWhiteSpace(subscription.Method))
                continue;

            if (!Uri.TryCreate(subscription.Endpoint, UriKind.Absolute, out var endpointUri))
            {
                _logger.LogWarning(
                    "Skipping subscription '{Codename}': endpoint '{Endpoint}' is not a valid absolute URI.",
                    subscription.Codename, subscription.Endpoint);
                continue;
            }

            HttpMethod method;
            try
            {
                method = HttpMethod.Parse(subscription.Method);
            }
            catch (FormatException)
            {
                _logger.LogWarning(
                    "Skipping subscription '{Codename}': '{Method}' is not a valid HTTP method.",
                    subscription.Codename, subscription.Method);
                continue;
            }

            try
            {
                var request = new HttpRequestMessage
                {
                    RequestUri = endpointUri,
                    Method = method,
                };

                bool methodSupportsBody = method != HttpMethod.Get
                    && method != HttpMethod.Head
                    && method != HttpMethod.Delete
                    && method != HttpMethod.Options
                    && method != HttpMethod.Trace;

                if (methodSupportsBody && !string.IsNullOrWhiteSpace(subscription.Requestbodyformat))
                {
                    var formattedBody = FormatRequestBody(formEntry, subscription.Requestbodyformat);

                    request.Content = string.IsNullOrEmpty(subscription.Requestcontenttype)
                        ? new StringContent(formattedBody, Encoding.UTF8)
                        : new StringContent(formattedBody, Encoding.UTF8, subscription.Requestcontenttype);
                }
                else if (!methodSupportsBody && !string.IsNullOrWhiteSpace(subscription.Requestbodyformat))
                {
                    _logger.LogWarning(
                        "Subscription '{Codename}' has a request body format configured but uses method '{Method}' which does not support a body. The body will be ignored.",
                        subscription.Codename, subscription.Method);
                }

                if (subscription.Useauthorization)
                {
                    var authorization = ConstructAuthorizationHeader(subscription);
                    if (authorization is null)
                    {
                        _logger.LogWarning(
                            "Subscription '{Codename}' has authorization enabled but no valid authorization could be constructed. The request will be sent without an Authorization header.",
                            subscription.Codename);
                    }
                    else
                    {
                        request.Headers.TryAddWithoutValidation("Authorization", authorization);
                    }
                }

                using var requestCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                requestCts.CancelAfter(EndpointTimeout);

                HttpResponseMessage response;
                try
                {
                    response = await httpClient.SendAsync(request, requestCts.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                {
                    _logger.LogError(
                        "Request to endpoint '{Endpoint}' timed out after {Timeout}s for subscription '{Codename}' during '{FormCodeName}' form submission.",
                        subscription.Endpoint, EndpointTimeout.TotalSeconds, subscription.Codename, formCodeName);
                    continue;
                }

                // Log once with full context and move on — do NOT also call
                // EnsureSuccessStatusCode(), which would throw and be logged
                // a second time (with less detail) by the catch block below.
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "Endpoint '{Endpoint}' returned {StatusCode} ({ReasonPhrase}) for subscription '{Codename}' during '{FormCodeName}' form submission. Method used: {Method}. Authorization required: {Useauthorization}.",
                        subscription.Endpoint, (int)response.StatusCode, response.ReasonPhrase,
                        subscription.Codename, formCodeName, subscription.Method, subscription.Useauthorization);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex,
                    "Failed to send request to endpoint '{Endpoint}' during '{FormCodeName}' form submission (subscription: '{Codename}').",
                    subscription.Endpoint, formCodeName, subscription.Codename);
            }
        }
    }

    // Safely resolves the configured form code names for a subscription.
    // Uses a virtual-property lookup so test doubles that shadow `Form`
    // (with `new`) are honored, and swallows any Kentico DataClass/IoC
    // initialization exceptions that can occur in unit-test scenarios.
    private static IEnumerable<string> TryGetFormNames(FormSubmissionSubscriptionsInfo subscription)
    {
        try
        {
            // Prefer the most-derived `Form` property (handles test subclasses
            // that hide the generated property with `new`).
            var prop = subscription.GetType().GetProperty(
                nameof(FormSubmissionSubscriptionsInfo.Form),
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);

            var value = prop?.GetValue(subscription) as IEnumerable<string>;
            return value ?? Enumerable.Empty<string>();
        }
        catch
        {
            return Enumerable.Empty<string>();
        }
    }

    private string? ConstructAuthorizationHeader(FormSubmissionSubscriptionsInfo formSubmissionSubscription)
    {
        if (!formSubmissionSubscription.Useauthorization)
        {
            return null;
        }

        return formSubmissionSubscription.Scheme switch
        {
            AuthenticationScheme.Basic => CreateBasicAuthorizationHeader(formSubmissionSubscription),
            _ => throw new NotImplementedException($"Authentication scheme '{formSubmissionSubscription.Scheme}' is not implemented."),
        };
    }

    // Returns the full "Basic <base64>" header value, or null if the secret
    // could not be decrypted (caller logs a warning and sends without auth).
    private string? CreateBasicAuthorizationHeader(FormSubmissionSubscriptionsInfo formSubmissionSubscription)
    {
        string? decryptedPassword = null;

        if (!string.IsNullOrEmpty(formSubmissionSubscription.EncryptedPassword))
        {
            try
            {
                decryptedPassword = _secretsEncryptionHelper.DecryptSecret(formSubmissionSubscription.EncryptedPassword);
            }
            catch (Exception ex)
            {
                // Don't let a bad/missing encryption key or malformed secret
                // crash the whole submission pipeline — skip auth for this
                // endpoint and let the caller log & continue without it.
                _logger.LogError(ex,
                    "Failed to decrypt password for subscription '{Codename}'. Sending without an Authorization header.",
                    formSubmissionSubscription.Codename);
                return null;
            }
        }

        var basicAuthorizationString = $"{formSubmissionSubscription.Username}:{decryptedPassword}";
        var basicAuthorizationBytes = Encoding.UTF8.GetBytes(basicAuthorizationString);
        var basicAuthorizationBase64String = Convert.ToBase64String(basicAuthorizationBytes);

        return $"Basic {basicAuthorizationBase64String}";
    }

    public static string FormatRequestBody(FormEntry formEntry, string requestBodyFormat)
    {
        var requestBody = requestBodyFormat;

        foreach (var (key, value) in formEntry)
        {
            if (value is null)
            {
                var newRequestBody = requestBody.Replace($"\"${key}$\"", "\"\"");

                requestBody = newRequestBody == requestBody
                    ? requestBody.Replace($"${key}$", "null")
                    : newRequestBody;
            }
            else
            {
                var rawValue = value.ToString() ?? string.Empty;

                // Quoted placeholder ("$key$") -> substitute a properly
                // JSON-escaped string, quotes included.
                var quotedPlaceholder = $"\"${key}$\"";
                if (requestBody.Contains(quotedPlaceholder))
                {
                    requestBody = requestBody.Replace(quotedPlaceholder, JsonSerializer.Serialize(rawValue));
                    continue;
                }

                requestBody = requestBody.Replace($"${key}$", JsonEncodedText.Encode(rawValue).ToString());
            }
        }

        return requestBody;
    }
}