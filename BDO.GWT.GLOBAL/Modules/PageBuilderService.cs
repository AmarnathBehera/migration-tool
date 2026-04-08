using BDO.GWT.Domain.Services;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Xperience.LiveSite.Services;

public class PageBuilderService : IPageBuilderService
{
    private const string MainArea = "mainArea";

    private readonly List<string> _widgets = [], _advancedWidgets = [], _sections = [], _advancedSections = [];
    private string _identifier = MainArea;
    private readonly ISettingsService _settingsService;

    public PageBuilderService(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public IPageBuilderService AddDefaultWidgets()
    {
        _widgets.AddRange(new[]
        {
            ComponentIdentifiers.Widgets.Imagewithtext,
            ComponentIdentifiers.Widgets.ExternalNavigation
        });
        return this;
    }

    public IPageBuilderService AddWidgets(IEnumerable<string> widgets)
    {
        _widgets.AddRange(widgets);
        return this;
    }

    public IPageBuilderService AddAdvancedWidgets(IEnumerable<string> widgets)
    {
        _advancedWidgets.AddRange(widgets);
        return this;
    }

    public IPageBuilderService AddSections(IEnumerable<string> sections)
    {
        _sections.AddRange(sections);
        return this;
    }

    public IPageBuilderService AddAdvancedSections(IEnumerable<string> advancedSections)
    {
        _advancedSections.AddRange(advancedSections);
        return this;
    }

    public IPageBuilderService AddIdentifier(string identifier)
    {
        _identifier = identifier;
        return this;
    }

    public async Task<IHtmlContent> BuildEditableAreaAsync(IHtmlHelper htmlHelper, CancellationToken cancellationToken)
    {
        var (widgets, sections) = await GetWidgetsAndSectionsListAsync(cancellationToken);
        var areaId = _identifier;

        var options = new EditableAreaOptions();

        if (widgets.Any())
        {
            options.AllowedWidgets = widgets;
        }

        if (sections.Any())
        {
            options.AllowedSections = sections;
            options.DefaultSectionIdentifier = sections.Count == 1 ? sections.First() : null;
        }
        else
        {
            options.AllowedSections =
            [
                ComponentIdentifiers.Sections.Container
            ];
        }

        ClearValues();
        return await Kentico.Web.Mvc.HtmlHelperExtensions.Kentico(htmlHelper).EditableAreaAsync(areaId, options);
    }

    public async Task<IHtmlContent> BuildWidgetZoneAsync(IHtmlHelper htmlHelper, List<string> allowedWidgets, CancellationToken cancellationToken)
    {
        var widgetId = _identifier;
        var widgets = (await GetWidgetsAndSectionsListAsync(cancellationToken)).Item1.Intersect(allowedWidgets);
        ClearValues();
        return await Kentico.Web.Mvc.HtmlHelperExtensions.Kentico(htmlHelper).WidgetZoneAsync(widgetId, widgets);
    }

    public IHtmlContent GetPageBuilderScripts(IHtmlHelper htmlHelper) =>
        Kentico.Web.Mvc.HtmlHelperExtensions.Kentico(htmlHelper).PageBuilderScripts();


    public IHtmlContent GetPageBuilderStyles(IHtmlHelper htmlHelper) =>
        Kentico.Web.Mvc.HtmlHelperExtensions.Kentico(htmlHelper).PageBuilderStyles();

    private void ClearValues()
    {
        _widgets.Clear();
        _sections.Clear();
        _advancedWidgets.Clear();
        _advancedSections.Clear();
        _identifier = MainArea;
    }

    private async Task<(List<string>, List<string>)> GetWidgetsAndSectionsListAsync(CancellationToken cancellationToken)
    {
        //var setting = await _settingsService.GetCurrentWebsiteSettingAsync("",
        //                                                                   cancellationToken);

        //if (setting != null && (bool)setting.Value!)
        //{
        //    _widgets.AddRange(_advancedWidgets);
        //    _sections.AddRange(_advancedSections);
        //}

        return (_widgets.Distinct().ToList(), _sections.Distinct().ToList());
    }
}
