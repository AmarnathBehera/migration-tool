using CMS;
using CMS.DataEngine;

[assembly: RegisterModule(typeof(MyXperienceSite.Modules.SampleProjectSettingsModule))]

namespace MyXperienceSite.Modules
{
    public class SampleProjectSettingsModule : Module
    {
        // Module constructor must pass the module code name to the base Module class
        public SampleProjectSettingsModule()
            : base("BDO.SampleProjectSetting")
        {
        }

        protected override void OnInit()
        {
            // No runtime permission creation here to avoid referencing CMS.Membership types
            base.OnInit();
        }
    }
}
