using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Services;
using System.Runtime.CompilerServices;

namespace BDO.GWT.GLOBAL.API.Tests.Services;

[TestFixture]
public class ChannelCrudServiceTests
{
    [Test]
    public void ChannelCrudService_ImplementsIChannelCrudService()
    {
        var service = (ChannelCrudService)RuntimeHelpers.GetUninitializedObject(typeof(ChannelCrudService));

        Assert.That(service, Is.InstanceOf<IChannelCrudService>());
    }

    [Test]
    public void ChannelsQuery_DefaultValues_AreNull()
    {
        var query = new ChannelsQuery();

        Assert.That(query.AdministrationDomain, Is.Null);
    }

    [Test]
    public void ChannelsQuery_CanSetAdministrationDomain()
    {
        var query = new ChannelsQuery { AdministrationDomain = "test.example.com" };

        Assert.That(query.AdministrationDomain, Is.EqualTo("test.example.com"));
    }

    [Test]
    public void ChannelOutput_DefaultCultures_IsEmpty()
    {
        var output = new ChannelOutput
        {
            General = new SiteGeneralOutput
            {
                ID = 1,
                GUID = Guid.NewGuid(),
                DisplayName = "Test",
                CodeName = "test",
                WebsitedomainresentationURL = "https://test.com"
            }
        };

        Assert.That(output.Cultures, Is.Empty);
    }

    [Test]
    public void ChannelOutput_General_StoresValues()
    {
        var guid = Guid.NewGuid();
        var output = new ChannelOutput
        {
            General = new SiteGeneralOutput
            {
                ID = 42,
                GUID = guid,
                DisplayName = "My Channel",
                CodeName = "my-channel",
                WebsitedomainresentationURL = "https://my-channel.com",
                DefaultContentCulture = "en-GB"
            }
        };

        Assert.Multiple(() =>
        {
            Assert.That(output.General.ID, Is.EqualTo(42));
            Assert.That(output.General.GUID, Is.EqualTo(guid));
            Assert.That(output.General.DisplayName, Is.EqualTo("My Channel"));
            Assert.That(output.General.CodeName, Is.EqualTo("my-channel"));
            Assert.That(output.General.WebsitedomainresentationURL, Is.EqualTo("https://my-channel.com"));
            Assert.That(output.General.DefaultContentCulture, Is.EqualTo("en-GB"));
        });
    }

    [Test]
    public void CultureSummaryOutput_StoresValues()
    {
        var guid = Guid.NewGuid();
        var culture = new CultureSummaryOutput
        {
            GUID = guid,
            Name = "English (United Kingdom)",
            Code = "en-GB"
        };

        Assert.Multiple(() =>
        {
            Assert.That(culture.GUID, Is.EqualTo(guid));
            Assert.That(culture.Name, Is.EqualTo("English (United Kingdom)"));
            Assert.That(culture.Code, Is.EqualTo("en-GB"));
        });
    }

    [Test]
    public void ChannelOutput_CanSetCultures()
    {
        var cultures = new[]
        {
            new CultureSummaryOutput { GUID = Guid.NewGuid(), Name = "English", Code = "en-GB" },
            new CultureSummaryOutput { GUID = Guid.NewGuid(), Name = "French", Code = "fr-FR" }
        };

        var output = new ChannelOutput
        {
            General = new SiteGeneralOutput
            {
                ID = 1,
                GUID = Guid.NewGuid(),
                DisplayName = "Test",
                CodeName = "test",
                WebsitedomainresentationURL = "https://test.com"
            },
            Cultures = cultures
        };

        Assert.That(output.Cultures, Has.Length.EqualTo(2));
    }
}
