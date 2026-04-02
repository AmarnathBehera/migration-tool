using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IPageBuilderService
{
    IPageBuilderService AddAdvancedSections(IEnumerable<string> advancedSections);

    IPageBuilderService AddAdvancedWidgets(IEnumerable<string> widgets);

    IPageBuilderService AddIdentifier(string identifier);

    IPageBuilderService AddSections(IEnumerable<string> sections);

    IPageBuilderService AddWidgets(IEnumerable<string> widgets);

    IPageBuilderService AddDefaultWidgets();

    Task<IHtmlContent> BuildEditableAreaAsync(IHtmlHelper htmlHelper, CancellationToken cancellationToken);

    Task<IHtmlContent> BuildWidgetZoneAsync(IHtmlHelper htmlHelper, List<string> allowedWidgets, CancellationToken cancellationToken);

    IHtmlContent GetPageBuilderScripts(IHtmlHelper htmlHelper);

    IHtmlContent GetPageBuilderStyles(IHtmlHelper htmlHelper);
}