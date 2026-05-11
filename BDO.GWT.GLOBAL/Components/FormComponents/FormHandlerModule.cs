using BDO.GWT.Core.Helpers;
using BDO.GWT.Web.Components.FormComponents;
using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.EmailEngine;
using CMS.EmailLibrary;
using CMS.Helpers;
using CMS.OnlineForms;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Text.RegularExpressions;

[assembly: RegisterModule(typeof(FormHandlerModule))]

namespace BDO.GWT.Web.Components.FormComponents;

public class FormHandlerModule : Module
{
    public FormHandlerModule()
        : base("CustomFormHandlers")
    {
    }

    protected override void OnInit()
    {
        base.OnInit();
        BizFormItemEvents.Insert.After += FormItem_InsertAfterHandler;
    }

    private void FormItem_InsertAfterHandler(object sender, BizFormItemEventArgs e)
    {
        BizFormItem formDataItem = e.Item;

        if (formDataItem == null)
        {
            return;
        }
        BizFormInfo formInfo = e.Item.BizFormInfo;

        if (formInfo == null)
        {
            return;
        }
        string formName = formInfo.FormName; // code name (optional)

        switch (formName?.ToLowerInvariant())
        {
            case "bdo_contactuswithsaveurl":
                {
                    string absoluteUrl = RequestContext.URLReferrer;
                    formDataItem.SetValue("Url", absoluteUrl);
                    formDataItem.Update();
                    break;
                }

            case "bdo_contactpeoplewithemail":
                {
                    SendEmail(formDataItem);
                    break;
                }

            default: return;
        }



    }

    private void SendEmail(BizFormItem formDataItem)
    {
        var scopeFactory = Service.Resolve<IServiceScopeFactory>();

        using var scope = scopeFactory.CreateScope();

            if (formDataItem == null)
            {
                return;
            }

            var emailValue = formDataItem.GetValue("email")?.ToString();
            if (string.IsNullOrWhiteSpace(emailValue))
            {
                return;
            }

            string absoluteUrl = RequestContext.URLReferrer;
            if (string.IsNullOrWhiteSpace(absoluteUrl))
            {
                return;
            }

            var uri = new Uri(absoluteUrl);

            string nodeAliasPath = uri.AbsolutePath.TrimEnd('/');
            string pathWithoutCulture = RemoveCultureFromPath(nodeAliasPath);

            string[] parts = nodeAliasPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            string? culture = parts.Length > 0 ? parts[0] : null;
            string personTitle = absoluteUrl.TrimEnd('/').Split('/').Last().Replace("-", " ");

            if (string.IsNullOrWhiteSpace(personTitle))
            {
                return;
            }
            var personInfo = GetPersonEmailSafely(personTitle, pathWithoutCulture, culture ?? string.Empty);

            if (personInfo == null || string.IsNullOrWhiteSpace(personInfo.Email))
            {
                return;
            }

            string toEmail = personInfo.Email;
            if (string.IsNullOrWhiteSpace(toEmail))
            {
                return;
            }

            var emailTemplateInfo = EmailTemplateInfo.Provider.Get("ContactEmail");
            if (emailTemplateInfo == null)
            {
                return;
            }

            var body = emailTemplateInfo.EmailTemplateCode
                .Replace("{%name%}", personInfo.Name)
                .Replace("{%email%}", emailValue)
                .Replace("{%description%}", Convert.ToString(formDataItem.GetValue("description")));

            var email = new EmailMessage
            {
                From = emailTemplateInfo.GetStringValue("TemplateFrom", ""),
                Recipients = personInfo.Email,
                Subject = Convert.ToString(formDataItem.GetValue("title")),
                Body = body,
                EmailFormat = EmailFormatEnum.Html
            };

            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            emailService.SendEmail(email);
        
    }

    private static string RemoveCultureFromPath(string nodeAliasPath)
    {
        if (string.IsNullOrWhiteSpace(nodeAliasPath))
            return string.Empty;

        return Regex.Replace(nodeAliasPath, @"^/[a-z]{2}-[a-z]{2}", "");
    }

    private static PersonEmailInfo GetPersonEmailSafely(string personTitle, string pathWithoutCulture, string culture)
    {
        var scopeFactory = Service.Resolve<IServiceScopeFactory>();

        using var scope = scopeFactory.CreateScope();
        var serviceProvider = scope.ServiceProvider;


            if (string.IsNullOrWhiteSpace(pathWithoutCulture))
            {
                return null;
            }

            var personHelper = serviceProvider.GetService<IPersonHelper>();
            if (personHelper == null)
            {
                return null;
            }

            return personHelper
                .GetPersonPageEmailId(personTitle, pathWithoutCulture, culture)
                .GetAwaiter()
                .GetResult();
        
    }

}




