using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Xperience.Admin.Pages.ProjectSettings.WebChannelSettings;

public class WebChannelSnippetCreatePage : CreatePage<WebChannelSnippetInfo, WebChannelSnippetEditSection>
{
    // Bind the parent object's ID
    [PageParameter(typeof(IntPageModelBinder), typeof(WebChannelSettingsEditSection))]
    public int WebChannelSettingsId { get; set; }

    public WebChannelSnippetCreatePage(IFormComponentMapper formComponentMapper,
        IFormDataBinder formDataBinder,
        IPageLinkGenerator pageLinkGenerator)
        : base(formComponentMapper, formDataBinder, pageLinkGenerator)
    {
    }

    public override Task ConfigurePage()
    {
        // Add the parent object's ID to the URL
        AdditionalLinkParameters.Add(new PageParameterValues
        {
            { typeof(WebChannelSettingsEditSection), WebChannelSettingsId}
        });
        PageConfiguration.UIFormName = "WebChannelSnippetEdit";
        return base.ConfigurePage();
    }

    // Ensure that the child object's foreign key is set
    protected override Task FinalizeInfoObject(WebChannelSnippetInfo infoObject, IFormFieldValueProvider fieldValueProvider, CancellationToken cancellationToken)
    {
        infoObject.WebChannelSnippetWebChannelSettingsID = WebChannelSettingsId;

        return base.FinalizeInfoObject(infoObject, fieldValueProvider, cancellationToken);
    }
}
