using CMS;
using CMS.Core;
using GWTXbyKWeb.FormComponents;
using Kentico.Xperience.Admin.Base;

[assembly: RegisterModule(typeof(GlobalLocationDefaultModule))]

namespace GWTXbyKWeb.FormComponents
{
    public class GlobalLocationDefaultModule : AdminModule
    {
        public GlobalLocationDefaultModule() : base("GWTXbyKWeb.GlobalLocationDefault")
        {
        }

        protected override void OnInit(ModuleInitParameters parameters)
        {
            base.OnInit(parameters);

            RegisterClientModule("bdo", "GlobalLocationDefault-components");
        }
    }
}