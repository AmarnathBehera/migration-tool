using BDO.GWT.Xperience;
using BDO.GWT.Xperience.Admin.Pages.ProjectSettings.GlobalSettings;
using BDO.GWT.Xperience.Admin.Pages.ProjectSettings.WebChannelSettings;
using GWTXbyKWeb.Pages.BDOIntegrations;
using GWTXbyKWeb.Pages.ProjectSettings.GlobalSettings;
using GWTXbyKWeb.Pages.ProjectSettings.WebChannelSettings;
using Kentico.Xperience.Admin.Base;

// ---- Project Settings: Global settings ----
[assembly: UIPage(
    parentType: typeof(ProjectSettingsApplication),
    slug: "global-settings",
    uiPageType: typeof(GlobalSettingsEditPage),
    name: "Global settings",
    templateName: TemplateNames.EDIT,
    order: 0)]

// ---- Project Settings: Channel settings (unchanged, from WebChannelSettings module) ----
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

// ---- BDO Integrations: entry point — channel listing ----
[assembly: UIPage(
    parentType: typeof(BdoIntegrationsApplication),
    slug: "BDO-Integrations",
    uiPageType: typeof(BdoIntegrationsChannelListingPage),
    name: "BDO Integrations",
    templateName: TemplateNames.LISTING,
    order: 0)]

// ---- BDO Integrations: per-channel edit shell ----
[assembly: UIPage(
    parentType: typeof(BdoIntegrationsChannelListingPage),
    slug: PageParameterConstants.PARAMETERIZED_SLUG,
    uiPageType: typeof(BdoIntegrationsChannelEditSection),
    name: "Edit",
    templateName: TemplateNames.SECTION_LAYOUT,
    order: 0)]

// ---- BDO Integrations: list of integrations for this channel ----
[assembly: UIPage(
    parentType: typeof(BdoIntegrationsChannelEditSection),
    slug: "integrations",
    uiPageType: typeof(BdoIntegrationListingPage),
    name: "Integrations",
    templateName: TemplateNames.LISTING,
    order: 0)]

// ---- BDO Integrations: create a new integration under this channel ----
[assembly: UIPage(
    parentType: typeof(BdoIntegrationListingPage),
    slug: "create",
    uiPageType: typeof(BdoIntegrationsCreatePage),
    name: "Create Form Submission Integration",
    templateName: TemplateNames.EDIT,
    order: 20)]

// ---- BDO Integrations: edit shell for one integration record ----
[assembly: UIPage(
    parentType: typeof(BdoIntegrationListingPage),
    slug: PageParameterConstants.PARAMETERIZED_SLUG,
    uiPageType: typeof(BdoIntegrationsEditSection),
    name: "Edit",
    templateName: TemplateNames.SECTION_LAYOUT,
    order: 10)]

// ---- BDO Integrations: edit form for one integration record ----
[assembly: UIPage(
    parentType: typeof(BdoIntegrationsEditSection),
    slug: "edit",
    uiPageType: typeof(BdoIntegrationEditPage),
    name: "Edit Form Submission subscription",
    templateName: TemplateNames.EDIT,
    order: 0)]