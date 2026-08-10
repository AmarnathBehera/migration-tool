using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Queries;
using BDO.GWT.GLOBAL.API.Services;
using System.Runtime.CompilerServices;

namespace BDO.GWT.GLOBAL.API.Tests.Services;

[TestFixture]
public class ChannelSettingCrudServiceTests
{
    [Test]
    public void ChannelSettingCrudService_ImplementsIChannelSettingCrudService()
    {
        var service = (ChannelSettingCrudService)RuntimeHelpers.GetUninitializedObject(typeof(ChannelSettingCrudService));

        Assert.That(service, Is.InstanceOf<IChannelSettingCrudService>());
    }

    [Test]
    public void ChannelSettingOutput_DefaultValues()
    {
        var output = new ChannelSettingOutput
        {
            CodeName = "BDO.Email",
            DisplayName = "Email",
            InheritFromGlobalSettings = false,
            Type = "Email"
        };

        Assert.Multiple(() =>
        {
            Assert.That(output.CodeName, Is.EqualTo("BDO.Email"));
            Assert.That(output.DisplayName, Is.EqualTo("Email"));
            Assert.That(output.InheritFromGlobalSettings, Is.False);
            Assert.That(output.Type, Is.EqualTo("Email"));
            Assert.That(output.Value, Is.Null);
        });
    }

    [Test]
    public void ChannelSettingOutput_CanSetInheritFromGlobalSettings()
    {
        var output = new ChannelSettingOutput
        {
            CodeName = "BDO.Email",
            DisplayName = "Email",
            InheritFromGlobalSettings = true,
            Type = "Email"
        };

        Assert.That(output.InheritFromGlobalSettings, Is.True);
    }

    [Test]
    public void ChannelSettingOutput_CanStoreValue()
    {
        var value = new Dictionary<string, object> { ["Field"] = "Value" };
        var output = new ChannelSettingOutput
        {
            CodeName = "BDO.SEOSettings",
            DisplayName = "SEO Settings",
            InheritFromGlobalSettings = false,
            Type = "SEOSettings",
            Value = value
        };

        Assert.That(output.Value, Is.SameAs(value));
    }

    [TestCase("Include Custom Scripts")]
    [TestCase("Email")]
    [TestCase("Events & Webinar")]
    [TestCase("General Setting")]
    [TestCase("SEO Settings")]
    [TestCase("Print or Share")]
    [TestCase("Filtering")]
    [TestCase("Our People")]
    public void SettingsQuery_AcceptsAllKnownChannelSettingDisplayNames(string displayName)
    {
        var query = new SettingsQuery { DisplayName = displayName };

        Assert.That(query.DisplayName, Is.EqualTo(displayName));
    }

    [TestCase("BDO.WebChannelSnippet")]
    [TestCase("BDO.Email")]
    [TestCase("BDO.EventsWebinars")]
    [TestCase("BDO.GeneralSetting")]
    [TestCase("BDO.SEOSettings")]
    [TestCase("BDO.PrintorShare")]
    [TestCase("BDO.Filtering")]
    [TestCase("BDO.OurPeople")]
    public void ChannelSettingOutput_AcceptsAllKnownCodeNames(string codeName)
    {
        var output = new ChannelSettingOutput
        {
            CodeName = codeName,
            DisplayName = "Test",
            InheritFromGlobalSettings = false,
            Type = "Test"
        };

        Assert.That(output.CodeName, Is.EqualTo(codeName));
    }
}
