using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.UIPages;

namespace BDO.GWT.Xperience.Admin.Pages.ProjectSettings.WebChannelSettings;

internal sealed class GdprActionClientProperties : IActionComponentClientProperties
{
    public string ComponentName { get; init; } = "CustomActionButton";

    public string CommandName { get; set; } = string.Empty;

    public string CommandType { get; set; } = string.Empty;
}

internal static class GdprButtonConfiguration
{
    public static void AddCustomActions(EditConfiguration pageConfiguration)
    {
        pageConfiguration.AdditionalActions.Add(new ButtonEditTemplateActionComponent(
            new Kentico.Xperience.Admin.Base.Action(ActionType.CustomComponent)
            {
                Label = "Search to Text",
                Identifier = "search-to-text",
                ComponentProperties = new GdprActionClientProperties
                {
                    CommandName = nameof(GDPREditPage.FetchTextData),
                    CommandType = nameof(GDPREditPage.FetchTextData)
                },
                Icon = "Search",
                Title = "Search contact and load text"
            }));

        pageConfiguration.AdditionalActions.Add(new ButtonEditTemplateActionComponent(
            new Kentico.Xperience.Admin.Base.Action(ActionType.CustomComponent)
            {
                Label = "Search to XML",
                Identifier = "search-to-xml",
                ComponentProperties = new GdprActionClientProperties
                {
                    CommandName = nameof(GDPREditPage.FetchXmlData),
                    CommandType = nameof(GDPREditPage.FetchXmlData)
                },
                Icon = "CodeTags",
                Title = "Search contact and load XML"
            }));

        pageConfiguration.AdditionalActions.Add(new ButtonEditTemplateActionComponent(
            new Kentico.Xperience.Admin.Base.Action(ActionType.CustomComponent)
            {
                Label = "Delete User",
                Identifier = "delete-user",
                ComponentProperties = new GdprActionClientProperties
                {
                    CommandName = nameof(GDPREditPage.Delete),
                    CommandType = nameof(GDPREditPage.Delete)
                },
                Icon = "Bin",
                Title = "Delete contact by email"
            }));
    }

    public static void AddCommandActions(EditConfiguration pageConfiguration)
    {
        pageConfiguration.AdditionalActions.Add(new ButtonEditTemplateActionComponent(
            new Kentico.Xperience.Admin.Base.Action(ActionType.Command)
            {
                Label = "Search to Text",
                Identifier = "search-to-text",
                Parameter = nameof(GDPREditPage.FetchTextData),
                Icon = "Search",
                Title = "Search contact and load text"
            }));

        pageConfiguration.AdditionalActions.Add(new ButtonEditTemplateActionComponent(
            new Kentico.Xperience.Admin.Base.Action(ActionType.Command)
            {
                Label = "Search to XML",
                Identifier = "search-to-xml",
                Parameter = nameof(GDPREditPage.FetchXmlData),
                Icon = "CodeTags",
                Title = "Search contact and load XML"
            }));

        pageConfiguration.AdditionalActions.Add(new ButtonEditTemplateActionComponent(
            new Kentico.Xperience.Admin.Base.Action(ActionType.Command)
            {
                Label = "Delete User",
                Identifier = "delete-user",
                Parameter = nameof(GDPREditPage.Delete),
                Icon = "Bin",
                Title = "Delete contact by email"
            }));
    }
}
