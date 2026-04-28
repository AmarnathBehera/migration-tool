// GWTXbyKWeb/FormComponents/CustomAdminModule.cs
using CMS;
using CMS.Core;
using GWTXbyKWeb.FormComponents;
using Kentico.Xperience.Admin.Base;

[assembly: RegisterModule(typeof(CustomAdminModule))]

namespace GWTXbyKWeb.FormComponents
{
    public class CustomAdminModule : AdminModule
    {
        public CustomAdminModule() : base("GWTXbyKWeb.CustomAdmin")
        {
        }

        protected override void OnInit(ModuleInitParameters parameters)
        {
            base.OnInit(parameters);

            RegisterClientModule("bdo", "custom-dropdown");
        }
    }
}