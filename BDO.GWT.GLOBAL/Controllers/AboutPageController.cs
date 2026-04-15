using BDO;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.LiveSite.Controllers;
using BDO.GWT.Web.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace GWTXbyKWeb.Controllers
{
    public class AboutPageController : AboutController<AboutPage>
    {
       public AboutPageController(IPageContext aboutPageContext, IAboutPageService<AboutPage> aboutPageService) : base(aboutPageContext, aboutPageService)
        {
        }
    }
}