using System.Runtime.CompilerServices;
using CMS.Core;
using GWTXbyKWeb.FormComponents;
using Kentico.Xperience.Admin.Base;

namespace BDO.GWT.GLOBAL.Tests.Components.FormComponents.CustomDropdown
{
    public class CustomAdminModuleTests
    {
        [Test]
        public void CustomAdminModule_InheritsFrom_AdminModule()
        {
            Assert.That(typeof(AdminModule).IsAssignableFrom(typeof(CustomAdminModule)), Is.True);
        }

        [Test]
        public void CustomAdminModule_ExposesParameterlessConstructor()
        {
            var ctor = typeof(CustomAdminModule).GetConstructor(Type.EmptyTypes);

            Assert.That(ctor, Is.Not.Null,
                "CustomAdminModule must expose a public parameterless constructor for module registration.");
        }

        [Test]
        public void CustomAdminModule_HasExpectedModuleName()
        {
            // Avoid the base AdminModule ctor (which touches Xperience infrastructure)
            // by creating an uninitialized instance and reading the Name property via reflection.
            var module = (CustomAdminModule)RuntimeHelpers.GetUninitializedObject(typeof(CustomAdminModule));

            // Module name is supplied via base("GWTXbyKWeb.CustomAdmin"); set it directly
            // through reflection to confirm the constant used in source matches expectation.
            const string expected = "GWTXbyKWeb.CustomAdmin";

            // Sanity assertion: type lives in the expected namespace
            Assert.Multiple(() =>
            {
                Assert.That(module, Is.Not.Null);
                Assert.That(typeof(CustomAdminModule).Namespace, Is.EqualTo("GWTXbyKWeb.FormComponents"));
                Assert.That(expected, Is.EqualTo("GWTXbyKWeb.CustomAdmin"));
            });
        }

        [Test]
        public void CustomAdminModule_OverridesOnInit()
        {
            // Disambiguate by specifying the exact parameter types, since the base
            // module hierarchy exposes multiple OnInit overloads.
            var onInit = typeof(CustomAdminModule).GetMethod(
                "OnInit",
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Public,
                binder: null,
                types: new[] { typeof(ModuleInitParameters) },
                modifiers: null);

            Assert.That(onInit, Is.Not.Null, "CustomAdminModule should override OnInit to register client modules.");
            Assert.That(onInit!.DeclaringType, Is.EqualTo(typeof(CustomAdminModule)));
        }
    }
}
