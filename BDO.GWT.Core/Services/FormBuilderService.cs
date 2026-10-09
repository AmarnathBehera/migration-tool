using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Helper;
using CMS.ContentEngine;
using CMS.DataEngine;
using Kentico.Content.Web.Mvc;
using Kentico.Forms.Web.Mvc.Widgets;
using Kentico.PageBuilder.Web.Mvc;
using Kentico.Xperience.Admin.Base.Forms;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Threading.Tasks;
using XperienceCommunity.Localization; // or the correct namespace

namespace BDO.GWT.Core.Services;

public interface IFormBuilderService
{
    Task<IHtmlContent> GetFormBuilderWidget(IHtmlHelper htmlHelper, string formIdentifier);
}

public class FormBuilderService : IFormBuilderService
{
    private readonly ICultureContext _cultureContext;
    private readonly ILocalizationHelper _localizationHelper;
    public FormBuilderService(ILocalizationHelper localizationHelper, ICultureContext cultureContext)
    {
        _localizationHelper = localizationHelper;
        _cultureContext = cultureContext;
    }

    public async Task<IHtmlContent> GetFormBuilderWidget(IHtmlHelper htmlHelper, string formIdentifier)
    {
        var widgetProperties = new FormWidgetProperties
        {
            SelectedForm = new List<ObjectRelatedItem>
            {
                new() { ObjectCodeName = formIdentifier }
            },
            AfterSubmitMode = FormAfterSubmitModeConstants.DISPLAY_MESSAGE,
            AfterSubmitDisplayText = await _localizationHelper.GetTranslationAsync(_cultureContext.CurrentLanguageTag, ResourceStringKey.LiveSite.People.contactForm, CancellationToken.None),
        };
        return await Kentico.Web.Mvc.HtmlHelperExtensions.Kentico(htmlHelper).RenderStandaloneWidgetAsync(SystemComponentIdentifiers.FORM_WIDGET_IDENTIFIER, widgetProperties);
    }
}
