using BDO;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.LiveSite.Controllers;
using BDO.GWT.Web.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace GWTXbyKWeb.Controllers
{
    public class SectionAboutController : AboutController<SectionAbout>
    {
        public SectionAboutController(IPageContext sectionAboutContext, IAboutPageService<SectionAbout> aboutPageService) : base(sectionAboutContext, aboutPageService)
        {
        }
    }
}