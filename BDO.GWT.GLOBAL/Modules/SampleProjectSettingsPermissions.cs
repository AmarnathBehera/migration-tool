using CMS.DataEngine;

namespace MyXperienceSite.Modules
{
    public static class SampleProjectSettingsPermissions
    {
        // Use a module-prefixed permission name to avoid collisions
        public const string View = "BDO.SampleProjectSetting.View";
        public const string Modify = "BDO.SampleProjectSetting.Modify";

        public static string[] All => new[] { View, Modify };
    }
}
