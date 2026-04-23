using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.Services;

namespace BDO.GWT.Web.Controllers
{
    public class SectionAboutController : AboutController<SectionAbout>
    {
        public SectionAboutController(IPageContext sectionAboutContext, IAboutPageService<SectionAbout> aboutPageService) : base(sectionAboutContext, aboutPageService)
        {
        }
    }
}