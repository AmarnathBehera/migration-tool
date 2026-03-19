using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using System.Threading.Tasks;

namespace BDO.GWT.Xperience.Admin.Pages.ProjectSettings.WebChannelSettings;

public class WebChannelSnippetEditPage : InfoEditPage<WebChannelSnippetInfo>
{
    [PageParameter(typeof(IntPageModelBinder), typeof(WebChannelSnippetEditSection))]
    public override int ObjectId { get; set; }

    public WebChannelSnippetEditPage(IFormComponentMapper formComponentMapper, IFormDataBinder formDataBinder)
        : base(formComponentMapper, formDataBinder)
    {
    }

    public override Task ConfigurePage()
    {
        PageConfiguration.UIFormName = "webchannelsnippetedit";
        return base.ConfigurePage();
    }
}
