using CMS.DataEngine;

namespace BDO.GWT.GLOBAL.Tests.Modules
{
    public class ChannelSettingsInitializerTests
    {
        [Test]
        public void Initializer_InheritsFromModule()
        {
            Assert.That(typeof(Module).IsAssignableFrom(typeof(ChannelSettingsInitializer)), Is.True);
        }

        [Test]
        public void Initializer_ExposesParameterlessConstructor()
        {
            var ctor = typeof(ChannelSettingsInitializer).GetConstructor(Type.EmptyTypes);

            Assert.That(ctor, Is.Not.Null);
        }

        [Test]
        public void Initializer_OverridesOnInit()
        {
            var onInit = typeof(ChannelSettingsInitializer).GetMethod(
                "OnInit",
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.DeclaredOnly);

            Assert.That(onInit, Is.Not.Null);
            Assert.That(onInit!.DeclaringType, Is.EqualTo(typeof(ChannelSettingsInitializer)));
        }
    }
}
