using CMS;
using CMS.DataEngine;

[assembly: RegisterModule(typeof(BDO.GWT.Xperience.Modules.ProjectSettingsModule))]

namespace BDO.GWT.Xperience.Modules
{
    public class ProjectSettingsModule : Module
    {
        // Module code name must match the module registered in the Xperience administration
        public ProjectSettingsModule()
            : base("BDO.ProjectSettings")
        {
        }

        protected override void OnInit()
        {
            base.OnInit();
        }
    }
}
