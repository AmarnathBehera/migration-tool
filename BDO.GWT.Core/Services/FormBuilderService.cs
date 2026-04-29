using CMS.DataEngine;
using Kentico.Content.Web.Mvc;
using Kentico.Forms.Web.Mvc.Widgets;
using Kentico.PageBuilder.Web.Mvc;
using Kentico.Xperience.Admin.Base.Forms;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BDO.GWT.Core.Services;

public interface IFormBuilderService
{
    Task<IHtmlContent> GetFormBuilderWidget(IHtmlHelper htmlHelper, string formIdentifier);
}

public class FormBuilderService : IFormBuilderService
{
    public async Task<IHtmlContent> GetFormBuilderWidget(IHtmlHelper htmlHelper, string formIdentifier)
    {
        var widgetProperties = new FormWidgetProperties
        {
            SelectedForm = new List<ObjectRelatedItem>
            {
                new() { ObjectCodeName = formIdentifier }
            }
        };

        return await Kentico.Web.Mvc.HtmlHelperExtensions.Kentico(htmlHelper).RenderStandaloneWidgetAsync(SystemComponentIdentifiers.FORM_WIDGET_IDENTIFIER, widgetProperties);
    }
}
