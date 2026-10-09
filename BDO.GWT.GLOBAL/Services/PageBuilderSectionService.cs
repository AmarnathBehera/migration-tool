using BDO.GWT.Xperience;
using Kentico.PageBuilder.Web.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace BDO.GWT.GLOBAL.Services;

public interface IPageBuilderSectionService
{
    List<string> GetAllowedWidgets(string sectionIdentifier);
}

public class PageBuilderSectionService : IPageBuilderSectionService
{
    private static readonly Dictionary<string, string[]> AllowedSectionsByWidget = new()
    {
        [ComponentIdentifiers.Widgets.SocialMediaButtons] = [
            ComponentIdentifiers.Sections.DefaultSection,
            ComponentIdentifiers.Sections.Container,
        ],
    };

    private static readonly Dictionary<string, string[]> AllowedWidgetsBySection = new()
    {
        [ComponentIdentifiers.Sections.FourColumns] = [
            ComponentIdentifiers.Widgets.Button,
            ComponentIdentifiers.Widgets.SocialMediaButtons,
            ComponentIdentifiers.Widgets.HTML,
            ComponentIdentifiers.Widgets.RichText,
        ],
        [ComponentIdentifiers.Sections.FiveColumns] = [
            ComponentIdentifiers.Widgets.Button,
            ComponentIdentifiers.Widgets.SocialMediaButtons,
            ComponentIdentifiers.Widgets.HTML,
            ComponentIdentifiers.Widgets.RichText,
        ],
    };

    private readonly List<string> _registeredWidgets = [];

    public PageBuilderSectionService()
    {
        PopulateRegisteredWidgets();
    }

    public List<string> GetAllowedWidgets(string sectionIdentifier)
    {
        if (AllowedWidgetsBySection.TryGetValue(sectionIdentifier, out var explicitlyAllowedWidgetIdentifiers))
        {
            return explicitlyAllowedWidgetIdentifiers.ToList();
        }

        var allowedWidgetIdentifiers = _registeredWidgets
            .Where(widgetIdentifier => !AllowedSectionsByWidget.TryGetValue(widgetIdentifier, out var allowedSectionIdentifiers) || allowedSectionIdentifiers.Contains(sectionIdentifier))
            .ToList();

        return allowedWidgetIdentifiers;
    }

    private void PopulateRegisteredWidgets()
    {
        var registerWidgetAttributes = typeof(PageBuilderComponentRegister).Assembly
            .GetCustomAttributes(typeof(RegisterWidgetAttribute), false)
            .Cast<RegisterWidgetAttribute>();

        foreach (var attribute in registerWidgetAttributes)
        {
            _registeredWidgets.Add(attribute.Identifier);
        }

        var kenticoRegisterWidgetAttributes = typeof(RegisterWidgetAttribute).Assembly
            .GetCustomAttributes(typeof(RegisterWidgetAttribute), false)
            .Cast<RegisterWidgetAttribute>();

        foreach (var attribute in kenticoRegisterWidgetAttributes)
        {
            _registeredWidgets.Add(attribute.Identifier);
        }
    }
}
