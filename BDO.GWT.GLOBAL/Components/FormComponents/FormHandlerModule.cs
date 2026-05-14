using BDO.GWT.Core;
using BDO.GWT.GLOBAL.Services;
using BDO.GWT.Xperience.Core.LiveSite.Components.FormComponents;
using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.EmailEngine;
using CMS.EmailLibrary;
using CMS.OnlineForms;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

[assembly: RegisterModule(typeof(FormHandlerModule))]

namespace BDO.GWT.Xperience.Core.LiveSite.Components.FormComponents;

public partial class FormHandlerModule : Module
{
    private IHttpContextAccessor _httpContextAccessor;
    private ILogger<FormHandlerModule> _logger;
    private IServiceScopeFactory _scopeFactory;

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

            var uri = new Uri(absoluteUrl);
            string nodeAliasPath = uri.AbsolutePath.TrimEnd('/');
            string pathWithoutCulture = RemoveCultureFromPath(nodeAliasPath);

            string[] parts = nodeAliasPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            string culture = parts.Length > 0 && CultureRegex().IsMatch(parts[0])
                ? NormalizeCulture(parts[0])
                : "en-GB";

            string personTitle = absoluteUrl.TrimEnd('/').Split('/').Last().Replace("-", " ");
            if (string.IsNullOrWhiteSpace(personTitle))
                return;

            var person = await GetPersonSafelyAsync(pathWithoutCulture, culture).ConfigureAwait(false);

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

            var email = new EmailMessage
            {
                From = "no-reply@yourdomain.com",
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

    private async Task<Person> GetPersonSafelyAsync(string pathWithoutCulture, string culture)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(pathWithoutCulture))
                return null;

            using var scope = _scopeFactory.CreateScope();
            var contentService = scope.ServiceProvider.GetRequiredService<IContentService>();

            var people = await contentService.RetrievePagesAsync<Person>(
                new RetrievePagesParameters(),
                query => query.Where(w => w.WhereEquals("WebPageItemTreePath", pathWithoutCulture)).TopN(1),
                CancellationToken.None).ConfigureAwait(false);

            return people?.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to retrieve person for path '{Path}' (culture: '{Culture}').",
                pathWithoutCulture, culture);

            return null;
        }
    }

    private string GetReferrerUrl()
        => _httpContextAccessor?.HttpContext?.Request.Headers.Referer.ToString()
           ?? string.Empty;

    private static string RemoveCultureFromPath(string nodeAliasPath)
    {
        if (string.IsNullOrWhiteSpace(nodeAliasPath))
            return string.Empty;

        return CulturePrefixRegex().Replace(nodeAliasPath, string.Empty);
    }

    private static string NormalizeCulture(string culture)
    {
        if (string.IsNullOrWhiteSpace(culture) || !culture.Contains('-'))
            return culture;

        var parts = culture.Split('-');
        return $"{parts[0].ToLowerInvariant()}-{parts[1].ToUpperInvariant()}";
    }

    [GeneratedRegex(@"^[a-zA-Z]{2}-[a-zA-Z]{2}$")]
    private static partial Regex CultureRegex();

    [GeneratedRegex(@"^/[a-z]{2}-[a-z]{2}", RegexOptions.IgnoreCase)]
    private static partial Regex CulturePrefixRegex();
}