using BDO;
using Kentico.Xperience.Admin.Base;

namespace GWTXbyKWeb.Pages.ProjectSettings.GlobalSettings
{
    [UIEvaluatePermission("Administrator")]
    public class GlobalSettings : ListingPage
    {
        protected override string ObjectType => GlobalsettingskeyInfo.OBJECT_TYPE;
    }
}