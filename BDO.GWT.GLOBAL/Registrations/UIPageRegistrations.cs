using BDO.GWT.Xperience;
using BDO.GWT.Xperience.Admin.Pages.ProjectSettings;
using BDO.GWT.Xperience.Admin.Pages.ProjectSettings.GlobalSettings;
using BDO.GWT.Xperience.Admin.Pages.ProjectSettings.WebChannelSettings;
using GWTXbyKWeb.Pages.ProjectSettings.GlobalSettings;
using Kentico.Xperience.Admin.Base;

[assembly: UIPage(
    parentType: typeof(ProjectSettingsApplication),
    slug: "global-settings",
    uiPageType: typeof(GlobalSettings),
    name: "Global settings",
    templateName: TemplateNames.SECTION_LAYOUT,
    order: 0)]

[assembly: UIPage(
    parentType: typeof(GlobalSettings),
    slug: "keys",
    uiPageType: typeof(GlobalSettingsEditPage),
    name: "Include Custom Scripts",
    templateName: TemplateNames.EDIT,
    order: 0)]

[assembly: UIPage(
    parentType: typeof(GlobalSettings),
    slug: "cookiebot",
    uiPageType: typeof(CookiebootEditPage),
    name: "Cookiebot",
    templateName: TemplateNames.EDIT,
    order: 1)]

[assembly: UIPage(
    parentType: typeof(GlobalSettings),
    slug: "events-webinar",
    uiPageType: typeof(GlobalEventsWebinarEditPage),
    name: "Events & Webinar",
    templateName: TemplateNames.EDIT,
    order: 2)]

[assembly: UIPage(
    parentType: typeof(GlobalSettings),
    slug: "api-permission",
    uiPageType: typeof(ApiPermissionEditPage),
    name: "API Permission",
    templateName: TemplateNames.EDIT,
    order: 2)]




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
    slug: "email",
    uiPageType: typeof(EmailEditPage),
    name: "Email settings",
    templateName: TemplateNames.EDIT,
    order: 3)]

[assembly: UIPage(
    parentType: typeof(WebChannelSettingsEditSection),
    slug: "events-webinar",
    uiPageType: typeof(EventsWebinarEditPage),
    name: "Events & Webinar",
    templateName: TemplateNames.EDIT,
    order: 4)]

[assembly: UIPage(
    parentType: typeof(WebChannelSettingsEditSection),
    slug: "general",
    uiPageType: typeof(GeneralSettingsEditPage),
    name: "General Settings",
    templateName: TemplateNames.EDIT,
    order: 5)]

[assembly: UIPage(
    parentType: typeof(WebChannelSettingsEditSection),
    slug: "edit",
    uiPageType: typeof(SeoSettingsEditPage),
    name: "SEO settings",
    templateName: TemplateNames.EDIT,
    order: 6)]

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
    name: "Include Custom Scripts",
    templateName: TemplateNames.EDIT,
    order: 0)]



[assembly: UIPage(
    parentType: typeof(WebChannelSettingsEditSection),
    slug: "print-or-share",
    uiPageType: typeof(PrintorShareEditPage),
    name: "Print Or Share settings",
    templateName: TemplateNames.EDIT,
    order: 7)]

[assembly: UIPage(
    parentType: typeof(WebChannelSettingsEditSection),
    slug: "filtering",
    uiPageType: typeof(FilteringEditPage),
    name: "Filtering settings",
    templateName: TemplateNames.EDIT,
    order: 8)]

[assembly: UIPage(
    parentType: typeof(WebChannelSettingsEditSection),
    slug: "our-people",
    uiPageType: typeof(OurPeopleInfoEditPage),
    name: "Our People settings",
    templateName: TemplateNames.EDIT,
    order: 9)]

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

