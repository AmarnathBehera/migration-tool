using CMS.Base;
using CMS.Membership;
using Kentico.Forms.Web.Mvc;

namespace BDO.GWT.Xperience.Core.LiveSite.Components.VisibilityConditions;

public class AdministratorVisibilityCondition : VisibilityCondition
{
    public override bool IsVisible()
    {
        return MembershipContext.AuthenticatedUser.IsAdministrator();
    }
}
