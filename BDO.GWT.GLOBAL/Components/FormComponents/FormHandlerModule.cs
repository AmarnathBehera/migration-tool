using BDO.GWT.Common;
using BDO.GWT.Core;
using BDO.GWT.GLOBAL.Services;
using BDO.GWT.Web.Core.LiveSite.Services;
using BDO.GWT.Xperience.Core.LiveSite.Components.FormComponents;
using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.EmailEngine;
using CMS.EmailLibrary;
using CMS.OnlineForms;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using Kentico.Xperience.Admin.Base;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

[assembly: RegisterModule(typeof(FormHandlerModule))]

namespace BDO.GWT.Xperience.Core.LiveSite.Components.FormComponents;

public partial class FormHandlerModule : Module
{
    private IHttpContextAccessor _httpContextAccessor;
    private ILogger<FormHandlerModule> _logger;
    private IServiceScopeFactory _scopeFactory;
    private const string EmailKey = "Email";
    public FormHandlerModule() : base("CustomFormHandlers")
    {
    }

    protected override void OnInit(ModuleInitParameters parameters)
    {
        base.OnInit(parameters);

        _httpContextAccessor = parameters.Services.GetRequiredService<IHttpContextAccessor>();
        _logger = parameters.Services.GetRequiredService<ILogger<FormHandlerModule>>();
        _scopeFactory = parameters.Services.GetRequiredService<IServiceScopeFactory>();

        BizFormItemEvents.Insert.After += FormItem_InsertAfterHandler;
    }

    private async void FormItem_InsertAfterHandler(object sender, BizFormItemEventArgs e)
    {
        try
        {
            BizFormItem formDataItem = e.Item;
            if (formDataItem == null)
                return;

            BizFormInfo formInfo = e.Item.BizFormInfo;
            if (formInfo == null)
                return;
            await CheckFormSubmissionAsync(formDataItem, formInfo).ConfigureAwait(false);

            switch (formInfo.FormName?.ToLowerInvariant())
            {
                case "bdo_contactuswithsaveurl":
                    {
                        formDataItem.SetValue("Url", GetReferrerUrl());
                        formDataItem.Update();
                        break;
                    }

                case "bdo_contactpeoplewithemail":
                    {
                        await SendEmailAsync(formDataItem).ConfigureAwait(false);
                        break;
                    }

                default:
                    return;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle form item insert event.");
        }
    }
    private async Task CheckFormSubmissionAsync(BizFormItem formDataItem, BizFormInfo formInfo)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var formSubmissionSubscriptionsService = scope.ServiceProvider
                .GetRequiredService<IFormSubmissionSubscriptionsService>();

            await formSubmissionSubscriptionsService
                .InvokeRegisteredEndpointsAsync(formDataItem,  formInfo, CancellationToken.None)
                .ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to invoke registered form submission endpoints for form {FormName}.", formInfo.FormName);
        }
    }
   private async Task SendEmailAsync(BizFormItem formDataItem)
    {
        try
        {
            if (formDataItem == null)
                return;

            var emailValue = formDataItem.GetValue("email")?.ToString();
            if (string.IsNullOrWhiteSpace(emailValue))
                return;

            string absoluteUrl = GetReferrerUrl();
            if (string.IsNullOrWhiteSpace(absoluteUrl))
                return;

            string personTitle = absoluteUrl.TrimEnd('/').Split('/').Last().Replace("-", " ");
            if (string.IsNullOrWhiteSpace(personTitle))
                return;

            var person = await GetCurrentPersonSafelyAsync().ConfigureAwait(false);
            if (person == null || string.IsNullOrWhiteSpace(person.PersonEmail))
                return;

            string personName = person.PersonFirstName ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(person.PersonSurnamePrefixes))
                personName += " " + person.PersonSurnamePrefixes;
            if (!string.IsNullOrWhiteSpace(person.PersonLastName))
                personName += " " + person.PersonLastName;

            var emailConfig = EmailTemplateInfo.Provider.Get("ContactEmail");
            if (emailConfig == null)
                return;

            string body = $@"
                <p><strong>Name:</strong> {personName}</p>
                <p><strong>From:</strong> {emailValue}</p>
                <p><strong>Message:</strong></p>
                <p>{formDataItem.GetStringValue("description", string.Empty)}</p>";

            string fromEmail = ResolveFromEmailForCurrentChannel();
            if (string.IsNullOrWhiteSpace(fromEmail))
            {
                _logger.LogWarning("No 'From' email configured in EmailInfo settings. Skipping email send.");
                return;
            }

            var email = new EmailMessage
            {
                From = fromEmail,
                Recipients = person.PersonEmail,
                Subject = Convert.ToString(formDataItem.GetValue("title")),
                Body = body,
                EmailFormat = EmailFormatEnum.Html
            };

            using (var scope = _scopeFactory.CreateScope())
            {
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                await emailService.SendEmail(email).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process contact people form submission.");
        }
    }

    private string ResolveFromEmailForCurrentChannel()
    {
        using var scope = _scopeFactory.CreateScope();
        var websiteChannelContext = scope.ServiceProvider.GetRequiredService<IWebsiteChannelContext>();
        var webChannelSettingsInfoProvider = scope.ServiceProvider.GetRequiredService<IInfoProvider<WebChannelSettingsInfo>>();
        var emailInfoProvider = scope.ServiceProvider.GetRequiredService<IInfoProvider<EmailInfo>>();

        int currentWebsiteID = websiteChannelContext.WebsiteChannelID;
        if (currentWebsiteID <= 0)
        {
            _logger.LogWarning("Current website channel ID is not available. Skipping email send.");
            return null;
        }

        var currentChannelSettings = webChannelSettingsInfoProvider
            .Get()
            .WhereEquals(nameof(WebChannelSettingsInfo.WebChannelSettingsChannelID), currentWebsiteID)
            .TopN(1)
            .FirstOrDefault();

        if (currentChannelSettings == null)
        {
            _logger.LogWarning("No WebChannelSettings found for channel ID {ChannelId}. Skipping email send.", currentWebsiteID);
            return null;
        }

        return emailInfoProvider.Get()
            .WhereEquals(nameof(EmailInfo.DisplayName), EmailKey)
            .WhereEquals(nameof(EmailInfo.EmailSettingsWebChannelSettingID), currentChannelSettings.WebChannelSettingsID)
            .TopN(1)
            .GetEnumerableTypedResult()
            .FirstOrDefault()?.FromEmail;
    }

    private async Task<Person> GetCurrentPersonSafelyAsync()
    {
        try
        {
            var httpContext = _httpContextAccessor?.HttpContext;
            if (httpContext == null)
            {
                _logger.LogWarning("No HttpContext available. Cannot resolve current page.");
                return null;
            }

            var contentService = httpContext.RequestServices.GetRequiredService<IContentService>();

            var person = await contentService.RetrieveCurrentPageAsync<Person>(
                new RetrieveCurrentPageParameters
                {
                    LinkedItemsMaxLevel = 4
                },
                CancellationToken.None).ConfigureAwait(false);

            return person;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve current page as Person.");
            return null;
        }
    }

    private string GetReferrerUrl()
        => _httpContextAccessor?.HttpContext?.Request.Headers.Referer.ToString()
           ?? string.Empty;
}