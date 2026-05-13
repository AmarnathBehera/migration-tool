using System.Reflection;
using BDO;
using CMS;
using CMS.DataEngine;
using Module = CMS.DataEngine.Module;

namespace BDO.GWT.GLOBAL.Tests.Azure
{
    public class CustomInitializationModuleTests
    {
        private const BindingFlags PrivateStatic =
            BindingFlags.Static | BindingFlags.NonPublic;

        [Test]
        public void Module_InheritsFromModule()
        {
            Assert.That(typeof(Module).IsAssignableFrom(typeof(CustomInitializationModule)), Is.True);
        }

        [Test]
        public void Module_ExposesParameterlessConstructor()
        {
            var ctor = typeof(CustomInitializationModule).GetConstructor(Type.EmptyTypes);

            Assert.That(ctor, Is.Not.Null);
        }

        [Test]
        public void Module_HasExpectedModuleName()
        {
            var instance = new CustomInitializationModule();

            // ModuleInfo is assigned in the base ctor with the name passed by derived type.
            var nameProperty = typeof(Module).GetProperty("Name",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            Assert.That(nameProperty, Is.Not.Null);
            Assert.That(nameProperty!.GetValue(instance), Is.EqualTo("CustomInit"));
        }

        [Test]
        public void Module_OverridesOnInit()
        {
            var onInit = typeof(CustomInitializationModule).GetMethod(
                "OnInit",
                BindingFlags.Instance |
                BindingFlags.NonPublic |
                BindingFlags.Public |
                BindingFlags.DeclaredOnly,
                binder: null,
                types: Type.EmptyTypes,
                modifiers: null);

            Assert.That(onInit, Is.Not.Null);
            Assert.That(onInit!.DeclaringType, Is.EqualTo(typeof(CustomInitializationModule)));
        }

        [Test]
        public void ChannelStoragePathTemplates_ContainsExpectedKenticoFolders()
        {
            var field = typeof(CustomInitializationModule)
                .GetField("ChannelStoragePathTemplates", PrivateStatic);

            Assert.That(field, Is.Not.Null, "Static field 'ChannelStoragePathTemplates' not found.");

            var templates = (string[])field!.GetValue(null)!;

            Assert.Multiple(() =>
            {
                Assert.That(templates, Is.Not.Null);
                Assert.That(templates, Has.Length.EqualTo(4));
                Assert.That(templates, Does.Contain("~/<siteCodeName>/media/"));
                Assert.That(templates, Does.Contain("~/<siteCodeName>/files/"));
                Assert.That(templates, Does.Contain("~/<siteCodeName>/metafiles/"));
                Assert.That(templates, Does.Contain("~/<siteCodeName>/BizFormFiles/"));
            });
        }

        [Test]
        public void ChannelStoragePathTemplates_AllContainSiteCodeNamePlaceholder()
        {
            var field = typeof(CustomInitializationModule)
                .GetField("ChannelStoragePathTemplates", PrivateStatic);

            var templates = (string[])field!.GetValue(null)!;

            Assert.That(templates, Is.All.Contains("<siteCodeName>"));
        }

        [Test]
        public void MapPathsForAllChannels_IsPrivateStatic()
        {
            var method = typeof(CustomInitializationModule).GetMethod(
                "MapPathsForAllChannels", PrivateStatic);

            Assert.That(method, Is.Not.Null);
            Assert.That(method!.IsPrivate, Is.True);
            Assert.That(method.IsStatic, Is.True);
        }

        [Test]
        public void MapPathToAzureStorageProvider_IsPrivateStatic()
        {
            var method = typeof(CustomInitializationModule).GetMethod(
                "MapPathToAzureStorageProvider", PrivateStatic);

            Assert.That(method, Is.Not.Null);
            Assert.That(method!.IsPrivate, Is.True);
            Assert.That(method.IsStatic, Is.True);
        }

        [Test]
        public void Module_IsRegisteredViaAssemblyAttribute()
        {
            var assembly = typeof(CustomInitializationModule).Assembly;

            var registered = assembly
                .GetCustomAttributes<RegisterModuleAttribute>()
                .Any(a =>
                {
                      var prop = a.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                        .FirstOrDefault(p => p.PropertyType == typeof(Type));
                    return prop != null
                        && (Type?)prop.GetValue(a) == typeof(CustomInitializationModule);
                });


            Assert.That(registered, Is.True,
                "CustomInitializationModule must be registered via [assembly: RegisterModule].");
        }
    }
}
