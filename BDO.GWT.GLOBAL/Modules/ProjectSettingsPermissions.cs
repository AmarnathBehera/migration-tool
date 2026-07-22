using CMS.DataEngine;

namespace BDO.GWT.Xperience.Modules
{
    public static class ProjectSettingsPermissions
    {
        // Use a module-prefixed permission name to avoid collisions
        public const string View = "BDO.ProjectSettings.View";
        public const string Modify = "BDO.ProjectSettings.Modify";

        public static string[] All => new[] { View, Modify };
    }
}
