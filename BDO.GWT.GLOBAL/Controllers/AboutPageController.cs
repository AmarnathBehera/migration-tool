using BDO;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Controllers;
using BDO.GWT.Web.Core.Services;

namespace BDO.GWT.Web.Controllers
{
    public class AboutPageController : AboutController<AboutPage>
    {
       public AboutPageController(IPageContext aboutPageContext, IAboutPageService<AboutPage> aboutPageService) : base(aboutPageContext, aboutPageService)
        {
        }
    }
}