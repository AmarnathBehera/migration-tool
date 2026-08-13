using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Services;
using System.Runtime.CompilerServices;

namespace BDO.GWT.GLOBAL.API.Tests.Services;

[TestFixture]
public class LicenseKeysCrudServiceTests
{
    [Test]
    public void LicenseKeysCrudService_ImplementsILicenseKeysCrudService()
    {
        var service = (LicenseKeysCrudService)RuntimeHelpers.GetUninitializedObject(typeof(LicenseKeysCrudService));

        Assert.That(service, Is.InstanceOf<ILicenseKeysCrudService>());
    }

    #region LicenseKeyInput

    [Test]
    public void LicenseKeyInput_Key_CanBeSet()
    {
        var input = new LicenseKeyInput { Key = "my-license-key" };

        Assert.That(input.Key, Is.EqualTo("my-license-key"));
    }

    #endregion

    #region LicenseKeyOutput

    [Test]
    public void LicenseKeyOutput_DefaultKey_IsNull()
    {
        var output = new LicenseKeyOutput { ID = 1 };

        Assert.That(output.Key, Is.Null);
    }

    [Test]
    public void LicenseKeyOutput_StoresValues()
    {
        var output = new LicenseKeyOutput { ID = 7, Key = "encoded-key-value" };

        Assert.Multiple(() =>
        {
            Assert.That(output.ID, Is.EqualTo(7));
            Assert.That(output.Key, Is.EqualTo("encoded-key-value"));
        });
    }

    [Test]
    public void LicenseKeyOutput_Key_CanBeNull()
    {
        var output = new LicenseKeyOutput { ID = 1, Key = null };

        Assert.That(output.Key, Is.Null);
    }

    #endregion

    #region LicenseInfo

    [Test]
    public void LicenseInfo_DefaultValues_AreNull()
    {
        var info = new LicenseInfo();

        Assert.Multiple(() =>
        {
            Assert.That(info.Id, Is.Null);
            Assert.That(info.Owner, Is.Null);
            Assert.That(info.ExpirationDateUtc, Is.Null);
        });
    }

    [Test]
    public void LicenseInfo_StoresValues()
    {
        var expiry = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var info = new LicenseInfo
        {
            Id = "license-001",
            Owner = "Acme Corp",
            ExpirationDateUtc = expiry
        };

        Assert.Multiple(() =>
        {
            Assert.That(info.Id, Is.EqualTo("license-001"));
            Assert.That(info.Owner, Is.EqualTo("Acme Corp"));
            Assert.That(info.ExpirationDateUtc, Is.EqualTo(expiry));
        });
    }

    #endregion

    #region LimitationsInfo

    [Test]
    public void LimitationsInfo_DefaultValues_AreZero()
    {
        var info = new LimitationsInfo();

        Assert.Multiple(() =>
        {
            Assert.That(info.Channels, Is.EqualTo(0));
            Assert.That(info.MicroChannels, Is.EqualTo(0));
        });
    }

    [Test]
    public void LimitationsInfo_StoresValues()
    {
        var info = new LimitationsInfo { Channels = 5, MicroChannels = 10 };

        Assert.Multiple(() =>
        {
            Assert.That(info.Channels, Is.EqualTo(5));
            Assert.That(info.MicroChannels, Is.EqualTo(10));
        });
    }

    #endregion
}
