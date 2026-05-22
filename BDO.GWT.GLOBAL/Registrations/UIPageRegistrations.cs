using BDO.GWT.Xperience;
using BDO.GWT.Xperience.Admin.Pages.ProjectSettings.GlobalSettings;
using BDO.GWT.Xperience.Admin.Pages.ProjectSettings.WebChannelSettings;
using Kentico.Xperience.Admin.Base;

[assembly: UIPage(
    parentType: typeof(ProjectSettingsApplication),
    slug: "global-settings",
    uiPageType: typeof(GlobalSettingsListingPage),
    name: "Global settings",
    templateName: TemplateNames.LISTING,
      order: 0)]

[assembly: UIPage(
    parentType: typeof(ProjectSettingsApplication),
    slug: "Cookiebot",
    uiPageType: typeof(CookiebootEditPage),
    name: "Cookiebot",
    templateName: TemplateNames.EDIT,
    order: 1)]


[assembly: UIPage(
    parentType: typeof(GlobalSettingsListingPage),
    slug: PageParameterConstants.PARAMETERIZED_SLUG,
    uiPageType: typeof(GlobalSettingsEditSection),
    name: "Edit",
    templateName: TemplateNames.SECTION_LAYOUT,
    order: 10)]

[assembly: UIPage(
    parentType: typeof(GlobalSettingsEditSection),
    slug: "edit",
    uiPageType: typeof(GlobalSettingsEditPage),
    name: "Edit global settings key",
    templateName: TemplateNames.EDIT,
    order: 0)]

[assembly: UIPage(
    parentType: typeof(GlobalSettingsListingPage),
    slug: "create",
    uiPageType: typeof(GlobalSettingsCreatePage),
    name: "Create global settings key",
    templateName: TemplateNames.EDIT,
    order: 20)]

[assembly: UIPage(
    parentType: typeof(ProjectSettingsApplication),
    slug: "channel-settings",
    uiPageType: typeof(WebChannelSettingsListingPage),
    name: "Channel settings",
    templateName: TemplateNames.LISTING,
    order: 0)]

[assembly: UIPage(
    parentType: typeof(WebChannelSettingsListingPage),
    slug: PageParameterConstants.PARAMETERIZED_SLUG,
    uiPageType: typeof(WebChannelSettingsEditSection),
    name: "Edit",
    templateName: TemplateNames.SECTION_LAYOUT,
    order: 0)]

[assembly: UIPage(
    parentType: typeof(WebChannelSettingsEditSection),
    slug: "Email",
    uiPageType: typeof(EmailEditPage),
    name: "Email settings",
    templateName: TemplateNames.EDIT,
    order: 6)]
[assembly: UIPage(
    parentType: typeof(WebChannelSettingsEditSection),
    slug: "General",
    uiPageType: typeof(GeneralSettingsEditPage),
    name: "General Settings",
    templateName: TemplateNames.EDIT,
    order: 6)]

[assembly: UIPage(
    parentType: typeof(WebChannelSettingsEditSection),
    slug: "edit",
    uiPageType: typeof(SeoSettingsEditPage),
    name: "SEO settings",
    templateName: TemplateNames.EDIT,
    order: 5)]

[assembly: UIPage(
    parentType: typeof(WebChannelSettingsEditSection),
    slug: "snippets",
    uiPageType: typeof(WebChannelSnippetListingPage),
    name: "Channel snippets",
    templateName: TemplateNames.LISTING,
    order: 10)]

[assembly: UIPage(
    parentType: typeof(WebChannelSnippetListingPage),
    slug: PageParameterConstants.PARAMETERIZED_SLUG,
    uiPageType: typeof(WebChannelSnippetEditSection),
    name: "Edit snippets",
    templateName: TemplateNames.SECTION_LAYOUT,
    order: 0)]

[assembly: UIPage(
    parentType: typeof(WebChannelSnippetEditSection),
    slug: "edit",
    uiPageType: typeof(WebChannelSnippetEditPage),
    name: "Edit snippet",
    templateName: TemplateNames.EDIT,
    order: 0)]

[assembly: UIPage(
    parentType: typeof(WebChannelSnippetListingPage),
    slug: "create",
    uiPageType: typeof(WebChannelSnippetCreatePage),
    name: "Create snippet",
    templateName: TemplateNames.EDIT,
    order: 20)]

[assembly: UIPage(
    parentType: typeof(WebChannelSettingsEditSection),
    slug: "print-or-share",          
    uiPageType: typeof(PrintorShareEditPage),
    name: "Print Or Share settings",
    templateName: TemplateNames.EDIT,
    order: 7)]

[assembly: UIPage(
    parentType: typeof(WebChannelSettingsEditSection),
    slug: "filtering-or-share",
    uiPageType: typeof(FilteringEditPage),
    name: "Filtering settings",
    templateName: TemplateNames.EDIT,
    order: 8)]
