using BDO;
using CMS.DataEngine;
using CMS.OnlineForms;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Core.LiveSite.Services;

public interface IFormSubmissionSubscriptionsService
{
    Task InvokeRegisteredEndpointsAsync(BizFormItem formItem, BizFormInfo formInfo, CancellationToken cancellationToken);
}

public class FormSubmissionSubscriptionsService : IFormSubmissionSubscriptionsService
{
    private readonly ILogger<FormSubmissionSubscriptionsService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IInfoProvider<FormSubmissionSubscriptionsInfo> _subscriptionProvider;

    public FormSubmissionSubscriptionsService(
        ILogger<FormSubmissionSubscriptionsService> logger,
        IHttpClientFactory httpClientFactory,
        IInfoProvider<FormSubmissionSubscriptionsInfo> subscriptionProvider)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _subscriptionProvider = subscriptionProvider;
    }

    public async Task InvokeRegisteredEndpointsAsync(BizFormItem formItem, BizFormInfo formInfo,CancellationToken cancellationToken)
    {
        if (formItem == null)
            return;

        string formCodeName = formItem.BizFormInfo?.FormName ?? string.Empty;

        var subscriptions = _subscriptionProvider
            .Get()
            .GetEnumerableTypedResult()
            .Where(s => s.Form != null && s.Form.Contains(formCodeName, StringComparer.OrdinalIgnoreCase))
            .ToList();

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

            try
            {
                var request = new HttpRequestMessage
                {
                    RequestUri = endpointUri,
                    Method = HttpMethod.Parse(subscription.Method),
                };

                var method = request.Method;
                bool methodSupportsBody = method != HttpMethod.Get
                    && method != HttpMethod.Head
                    && method != HttpMethod.Delete
                    && method != HttpMethod.Options
                    && method != HttpMethod.Trace;

                if (methodSupportsBody && !string.IsNullOrWhiteSpace(subscription.Requestbodyformat))
                {
                    var formattedBody = FormatRequestBody(formItem, subscription.Requestbodyformat);

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

                var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "Endpoint '{Endpoint}' returned {StatusCode} ({ReasonPhrase}) for subscription '{Codename}' during '{FormCodeName}' form submission. Method used: {Method}. Authorization required: {Useauthorization}.",
                        subscription.Endpoint, (int)response.StatusCode, response.ReasonPhrase,
                        subscription.Codename, formCodeName, subscription.Method, subscription.Useauthorization);
                }

                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex,
                    "Failed to send request to endpoint '{Endpoint}' during '{FormCodeName}' form submission (subscription: '{Codename}').",
                    subscription.Endpoint, formCodeName, subscription.Codename);
            }
        }
    }

    
    private static string FormatRequestBody(BizFormItem formItem, string bodyFormat)
    {
          var columnLookup = formItem.ColumnNames
            .ToDictionary(col => col, col => col, StringComparer.OrdinalIgnoreCase);

        return Regex.Replace(bodyFormat, @"\{(\w+)\}", m =>
        {
            var placeholder = m.Groups[1].Value;

            if (columnLookup.TryGetValue(placeholder, out var actualColumnName))
                return formItem.GetStringValue(actualColumnName, string.Empty);

            return string.Empty;
        });
    }
}
