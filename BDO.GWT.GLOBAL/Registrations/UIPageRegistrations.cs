using BDO.GWT.Xperience;
using BDO.GWT.Xperience.Admin.Pages.ProjectSettings.GlobalSettings;
using BDO.GWT.Xperience.Admin.Pages.ProjectSettings.WebChannelSettings;
using GWTXbyKWeb.Pages.ProjectSettings.GlobalSettings;
using GWTXbyKWeb.Pages.ProjectSettings.WebChannelSettings;
using Kentico.Xperience.Admin.Base;

[assembly: UIPage(
    parentType: typeof(ProjectSettingsApplication),
    slug: "global-settings",
    uiPageType: typeof(GlobalSettingsEditPage),
    name: "Global settings",
    templateName: TemplateNames.EDIT,
    order: 0)]

[assembly: UIPage(
    parentType: typeof(ProjectSettingsApplication),
    slug: "channel-settings",
    uiPageType: typeof(WebChannelSettingsListingPage),
    name: "Channel settings",
    templateName: TemplateNames.LISTING,
    order: 1)]

[assembly: UIPage(
    parentType: typeof(WebChannelSettingsListingPage),
    slug: PageParameterConstants.PARAMETERIZED_SLUG,
    uiPageType: typeof(WebChannelSettingsEditSection),
    name: "Edit",
    templateName: TemplateNames.SECTION_LAYOUT,
    order: 0)]



[assembly: UIPage(
    parentType: typeof(WebChannelSettingsEditSection),
    slug: "snippets",
    uiPageType: typeof(WebChannelSnippetEditSection),
    name: "Channel snippets",
    templateName: TemplateNames.LISTING,
    order: 10)]

[assembly: UIPage(
    parentType: typeof(WebChannelSettingsEditSection),
    slug: "Key",
    uiPageType: typeof(WebChannelSnippetEditPage),
    name: "Channel Settings",
    templateName: TemplateNames.EDIT,
    order: 0)]

[assembly: UIPage(
    parentType: typeof(WebChannelSettingsEditSection),
    slug: "gdpr",
    uiPageType: typeof(GDPRInfoPage),
    name: "GDPR",
    templateName: TemplateNames.EDIT,
    order: 10)]

[assembly: UIPage(
    parentType: typeof(WebChannelSettingsEditSection),
    slug: "gdprconsents",
    uiPageType: typeof(GDPRConsentEditPage),
    name: "GDPR Consents",
    templateName: TemplateNames.EDIT,
    order: 11)]

[assembly: UIPage(
    parentType: typeof(WebChannelSettingsEditSection),
    slug: "api-permission",
    uiPageType: typeof(ApiPermissionEditPage),
    name: "API Permission",
    templateName: TemplateNames.EDIT,
    order: 12)]

