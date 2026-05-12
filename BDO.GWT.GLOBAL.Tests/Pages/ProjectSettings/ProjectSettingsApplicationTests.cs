using BDO.GWT.Xperience;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.UIPages;

namespace BDO.GWT.GLOBAL.Tests.Pages.ProjectSettings
{
    public class ProjectSettingsApplicationTests
    {
        [Test]
        public void Identifier_HasExpectedValue()
        {
            Assert.That(ProjectSettingsApplication.IDENTIFIER, Is.EqualTo("BDO.Projectsettings"));
        }

        [Test]
        public void Page_InheritsFromApplicationPage()
        {
            Assert.That(typeof(ApplicationPage).IsAssignableFrom(typeof(ProjectSettingsApplication)), Is.True);
        }

        [Test]
        public void Page_HasFourUIPermissionAttributes()
        {
            var attributes = typeof(ProjectSettingsApplication)
                .GetCustomAttributes(typeof(UIPermissionAttribute), inherit: false);

            Assert.That(attributes.Length, Is.EqualTo(4));
        }
    }
}
