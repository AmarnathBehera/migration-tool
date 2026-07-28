using BDO.GWT.GLOBAL.Services;
using CMS.DataEngine;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace BDO.GWT.Xperience.Admin.Pages.ProjectSettings.WebChannelSettings;

public class GDPREditPage : InfoEditPage<GDPRInfo>
{
    private readonly IInfoProvider<GDPRInfo> GDPRInfoProvider;
    private readonly IInfoProvider<WebChannelSettingsInfo> webChannelSettingsInfoProvider;
    private readonly IChannelAccessService channelAccessService;
    private readonly IGDPRContactLookupService gdprContactLookupService;
    private readonly IHttpContextAccessor httpContextAccessor;
    protected override string ObjectType => GDPRInfo.OBJECT_TYPE;

    [PageParameter(typeof(IntPageModelBinder))]
    public int WebChannelSettingsId { get; set; }

    [PageParameter(typeof(IntPageModelBinder))]
    public override int ObjectId
    {
        get
        {
            var existing = GDPRInfoProvider.Get()
                .WhereEquals(nameof(GDPRInfo.GDPRWebChannelSettingID), WebChannelSettingsId)
                .FirstOrDefault();

            if (existing != null)
                return existing.GDPRID;

            if (WebChannelSettingsId <= 0)
                return 0;

            // Auto-create an empty GDPRInfo for this channel so the edit form can load.
            var newGDPRInfo = new GDPRInfo
            {
                GDPRWebChannelSettingID = WebChannelSettingsId.ToString()
            };
            GDPRInfoProvider.Set(newGDPRInfo);

            return newGDPRInfo.GDPRID;
        }
        set => _ = value;
    }

    public GDPREditPage(
        IFormComponentMapper formComponentMapper,
        IFormDataBinder formDataBinder,
        IInfoProvider<GDPRInfo> GDPRInfoProvider,
        IInfoProvider<WebChannelSettingsInfo> webChannelSettingsInfoProvider,
        IChannelAccessService channelAccessService,
        IGDPRContactLookupService gdprContactLookupService,
        IHttpContextAccessor httpContextAccessor)
            : base(formComponentMapper, formDataBinder)
    {
        this.GDPRInfoProvider = GDPRInfoProvider;
        this.webChannelSettingsInfoProvider = webChannelSettingsInfoProvider;
        this.channelAccessService = channelAccessService;
        this.gdprContactLookupService = gdprContactLookupService;
        this.httpContextAccessor = httpContextAccessor;
    }

    public override async Task ConfigurePage()
    {
        bool canAccess = await channelAccessService.CanAccessAsync(WebChannelSettingsId);
        if (!canAccess)
        {
            PageConfiguration.SubmitConfiguration.Visible = false;
            PageConfiguration.Headline = "Access Denied";
            return;
        }

        PageConfiguration.UIFormName = "gdprdisplay";
        PageConfiguration.Headline = "GDPR Display";
        PageConfiguration.SubmitConfiguration.Visible = false;
        GdprButtonConfiguration.AddCommandActions(PageConfiguration);

        await base.ConfigurePage();

        // Keep this page command-only (no Save button).
        PageConfiguration.SubmitConfiguration.Visible = false;
    }


    // Loads the GDPR record currently bound to the edit form.
    private GDPRInfo GetCurrentGDPRInfo() => GDPRInfoProvider.Get(ObjectId);

    private async Task<string> ResolveEmailAddressAsync(GDPRInfo gdpr)
    {
        static string ReadFirstNonEmpty(StringValues values)
            => values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v))?.Trim() ?? string.Empty;

        var request = httpContextAccessor.HttpContext?.Request;
        string[] keys = { "EmailAddress", "emailAddress", "Email", "email" };

        if (request != null)
        {
            foreach (var key in keys)
            {
                if (request.Query.TryGetValue(key, out var queryValue))
                {
                    var value = ReadFirstNonEmpty(queryValue);
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        return value;
                    }
                }
            }

            if (request.HasFormContentType)
            {
                var form = await request.ReadFormAsync();
                foreach (var key in keys)
                {
                    if (form.TryGetValue(key, out var formValue))
                    {
                        var value = ReadFirstNonEmpty(formValue);
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            return value;
                        }
                    }
                }
            }
        }

        return gdpr?.EmailAddress?.Trim() ?? string.Empty;
    }

    private async Task<ICommandResponse> BuildFormCommandResponse(bool success, string message, string results = null)
    {
        var gdpr = GetCurrentGDPRInfo();
        if (gdpr != null)
        {
            gdpr.Results = results ?? message;
            GDPRInfoProvider.Set(gdpr);
        }

        var items = await GetFormItems();

        var response = ResponseFrom(new FormSubmissionResult(
            success ? FormSubmissionStatus.ValidationSuccess : FormSubmissionStatus.ValidationFailure)
        {
            Items = await items.OnlyVisible().GetClientProperties(),
        });

        if (success)
        {
            response.AddSuccessMessage(message);
        }
        else
        {
            response.AddErrorMessage(message);
        }

        return response;
    }

    // "Search to Text" button: looks up the contact and stores the plain-text result.
    [PageCommand]
    public async Task<ICommandResponse> FetchTextData()
    {
        var gdpr = GetCurrentGDPRInfo();
        if (gdpr is null)
        {
            return await BuildFormCommandResponse(false, "GDPR record not found.");
        }

        var emailAddress = await ResolveEmailAddressAsync(gdpr);
        if (string.IsNullOrWhiteSpace(emailAddress))
        {
            return await BuildFormCommandResponse(false, "Email address is empty.");
        }

        gdpr.EmailAddress = emailAddress;
        GDPRInfoProvider.Set(gdpr);

        var results = gdprContactLookupService.GetContactAsText(emailAddress);

        return await BuildFormCommandResponse(true, "Data fetched successfully.", results);
    }

    // "Search to XML" button: looks up the contact and stores the XML result.
    [PageCommand]
    public async Task<ICommandResponse> FetchXmlData()
    {
        var gdpr = GetCurrentGDPRInfo();
        if (gdpr is null)
        {
            return await BuildFormCommandResponse(false, "GDPR record not found.");
        }

        var emailAddress = await ResolveEmailAddressAsync(gdpr);
        if (string.IsNullOrWhiteSpace(emailAddress))
        {
            return await BuildFormCommandResponse(false, "Email address is empty.");
        }

        gdpr.EmailAddress = emailAddress;
        GDPRInfoProvider.Set(gdpr);

        var results = gdprContactLookupService.GetContactAsXml(emailAddress);

        return await BuildFormCommandResponse(true, "Data fetched successfully.", results);
    }

    // "Copy" button: returns the stored results so the client can copy them to the clipboard.
    [PageCommand]
    public Task<ICommandResponse<GdprResultsResult>> Copy()
    {
        var gdpr = GetCurrentGDPRInfo();
        var results = gdpr?.Results ?? string.Empty;

        return Task.FromResult(ResponseFrom(new GdprResultsResult { Results = results }));
    }

    // "Delete" button: clears the stored results.
    [PageCommand]
    public async Task<ICommandResponse> Delete()
    {
        var gdpr = GetCurrentGDPRInfo();
        if (gdpr is null)
        {
            return await BuildFormCommandResponse(false, "GDPR record not found.");
        }

        var emailAddress = await ResolveEmailAddressAsync(gdpr);
        if (string.IsNullOrWhiteSpace(emailAddress))
        {
            return await BuildFormCommandResponse(false, "Email address is empty.");
        }

        gdpr.EmailAddress = emailAddress;
        GDPRInfoProvider.Set(gdpr);

        if (!gdprContactLookupService.DeleteContactByEmail(emailAddress))
        {
            return await BuildFormCommandResponse(false, "No contact found to delete.");
        }

        return await BuildFormCommandResponse(true, "Contact deleted successfully.");
    }
}

public class GdprResultsResult
{
    public string Results { get; set; } = string.Empty;
}
