using Kentico.Xperience.Admin.Base;

namespace BDO.GWT.Xperience.Admin.Pages.ProjectSettings.WebChannelSettings;

internal static class GdprButtonConfiguration
{

    public static void AddCommandActions(EditConfiguration pageConfiguration)
    {
        pageConfiguration.AdditionalActions.Add(new ButtonEditTemplateActionComponent(
            new Kentico.Xperience.Admin.Base.Action(ActionType.Command)
            {
                Label = "Search to Text",
                Identifier = "search-to-text",
                Parameter = nameof(GDPRInfoPage.FetchTextData),
                Icon = "Search",
                Title = "Search contact and load text"
            })
        {
            SubmitFormData = true
        });

        pageConfiguration.AdditionalActions.Add(new ButtonEditTemplateActionComponent(
            new Kentico.Xperience.Admin.Base.Action(ActionType.Command)
            {
                Label = "Search to XML",
                Identifier = "search-to-xml",
                Parameter = nameof(GDPRInfoPage.FetchXmlData),
                Icon = "CodeTags",
                Title = "Search contact and load XML"
            })
        {
            SubmitFormData = true
        });

        pageConfiguration.AdditionalActions.Add(new ButtonEditTemplateActionComponent(
            new Kentico.Xperience.Admin.Base.Action(ActionType.Command)
            {
                Label = "Delete",
                Identifier = "delete-user",
                Parameter = nameof(GDPRInfoPage.Delete),
                Icon = "Bin",
                Title = "Delete contact by email"
            })
        {
            SubmitFormData = true
        });

        pageConfiguration.AdditionalActions.Add(new ButtonEditTemplateActionComponent(
            new Kentico.Xperience.Admin.Base.Action(ActionType.Command)
            {
                Label = "Copy",
                Identifier = "copy-user",
                Parameter = nameof(GDPRInfoPage.Copy),
                Icon = "Copy",
                Title = "Copy GDPR results"
            })
        {
            SubmitFormData = true
        });
    }
}
