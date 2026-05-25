using BDO;
using Kentico.Xperience.Admin.Base;
using System.Threading.Tasks;

namespace GWTXbyKWeb.Pages.ProjectSettings.GlobalSettings
{
    [UIEvaluatePermission("Administrator")]
    public class GlobalSettings : ListingPage
    {
        protected override string ObjectType => GlobalsettingskeyInfo.OBJECT_TYPE;

        public override Task ConfigurePage()
        {
            // No columns needed — this is just a section container
            // Kentico will render the child pages (keys + cookiebot) as sub-nav
            return base.ConfigurePage();
        }
    }
}