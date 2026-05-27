using BDO.GWT.Core.Data;
using BDO.GWT.Core.Helpers;

namespace BDO.GWT.Core.Tests.Helpers;

public class VCardHelperTests
{
    [Test]
    public void GenerateVCardString_WithMinimalData_ReturnsVCardWithBeginEndAndVersion()
    {
        var vCard = new VCardData
        {
            FullName = "John Doe"
        };

        var result = VCardHelper.GenerateVCardString(vCard);

        Assert.Multiple(() =>
        {
            Assert.That(result, Does.Contain("BEGIN:VCARD"));
            Assert.That(result, Does.Contain("VERSION:3.0"));
            Assert.That(result, Does.Contain("END:VCARD"));
            Assert.That(result, Does.Contain("FN;CHARSET=utf-8:John Doe"));
            Assert.That(result, Does.Contain("N;CHARSET=utf-8:Doe;John;;;"));
        });
    }

    [Test]
    public void GenerateVCardString_WithSingleWordFullName_UsesNameAsLastName()
    {
        var vCard = new VCardData
        {
            FullName = "Cher"
        };

        var result = VCardHelper.GenerateVCardString(vCard);

        Assert.Multiple(() =>
        {
            Assert.That(result, Does.Contain("FN;CHARSET=utf-8:Cher"));
            Assert.That(result, Does.Contain("N;CHARSET=utf-8:Cher;;;;"));
        });
    }

    [Test]
    public void GenerateVCardString_WithMultiWordFullName_SplitsOnFirstSpace()
    {
        var vCard = new VCardData
        {
            FullName = "John Michael Doe"
        };

        var result = VCardHelper.GenerateVCardString(vCard);

        Assert.That(result, Does.Contain("N;CHARSET=utf-8:Michael Doe;John;;;"));
    }

    [Test]
    public void GenerateVCardString_WithNullFullName_DoesNotIncludeFnProperty()
    {
        var vCard = new VCardData
        {
            FullName = null!
        };

        var result = VCardHelper.GenerateVCardString(vCard);

        Assert.That(result, Does.Not.Contain("FN;CHARSET=utf-8:"));
    }

    [Test]
    public void GenerateVCardString_WithAllSimpleProperties_IncludesAllProperties()
    {
        var revision = new DateTime(2024, 1, 15, 10, 30, 45, DateTimeKind.Utc);
        var vCard = new VCardData
        {
            FullName = "Jane Smith",
            Categories = new[] { "Friends", "Work" },
            EMail = "jane@example.com",
            Source = "https://example.com/jane.vcf",
            Organization = "Acme Corp",
            Title = "Engineer",
            URL = "https://example.com/jane",
            RevisionDate = revision
        };

        var result = VCardHelper.GenerateVCardString(vCard);

        Assert.Multiple(() =>
        {
            Assert.That(result, Does.Contain("CATEGORIES;CHARSET=utf-8:Friends,Work"));
            Assert.That(result, Does.Contain("EMAIL:jane@example.com"));
            Assert.That(result, Does.Contain("SOURCE:https://example.com/jane.vcf"));
            Assert.That(result, Does.Contain("ORG;CHARSET=utf-8:Acme Corp"));
            Assert.That(result, Does.Contain("TITLE;CHARSET=utf-8:Engineer"));
            Assert.That(result, Does.Contain("URL:https://example.com/jane"));
            Assert.That(result, Does.Contain("REV:20240115T103045Z"));
        });
    }

    [Test]
    public void GenerateVCardString_WithNullOptionalProperties_OmitsThem()
    {
        var vCard = new VCardData
        {
            FullName = "Jane Smith"
        };

        var result = VCardHelper.GenerateVCardString(vCard);

        Assert.Multiple(() =>
        {
            Assert.That(result, Does.Not.Contain("CATEGORIES"));
            Assert.That(result, Does.Not.Contain("EMAIL"));
            Assert.That(result, Does.Not.Contain("SOURCE"));
            Assert.That(result, Does.Not.Contain("ORG"));
            Assert.That(result, Does.Not.Contain("TITLE"));
            Assert.That(result, Does.Not.Contain("URL"));
            Assert.That(result, Does.Not.Contain("REV"));
            Assert.That(result, Does.Not.Contain("TEL"));
            Assert.That(result, Does.Not.Contain("ADR"));
            Assert.That(result, Does.Not.Contain("LOGO"));
            Assert.That(result, Does.Not.Contain("PHOTO"));
        });
    }

    [Test]
    public void GenerateVCardString_WithTelephoneNumbers_AppendsValidEntriesOnly()
    {
        var vCard = new VCardData
        {
            FullName = "Jane Smith",
            TelephoneNumbers = new (string?, string?)[]
            {
                ("WORK", "+1-555-1111"),
                ("HOME", "+1-555-2222"),
                (null, "+1-555-3333"),
                ("CELL", null),
                ("", " ")
            }
        };

        var result = VCardHelper.GenerateVCardString(vCard);

        Assert.Multiple(() =>
        {
            Assert.That(result, Does.Contain("TEL;TYPE=WORK:+1-555-1111"));
            Assert.That(result, Does.Contain("TEL;TYPE=HOME:+1-555-2222"));
            Assert.That(result, Does.Not.Contain("+1-555-3333"));
            Assert.That(result, Does.Not.Contain("TYPE=CELL"));
        });
    }

    [Test]
    public void GenerateVCardString_WithLogoAndPhoto_IncludesImageProperties()
    {
        var vCard = new VCardData
        {
            FullName = "Jane Smith",
            LogoFormat = "png",
            LogoBase64 = "AAAA",
            PhotoFormat = "jpeg",
            PhotoBase64 = "BBBB"
        };

        var result = VCardHelper.GenerateVCardString(vCard);

        Assert.Multiple(() =>
        {
            Assert.That(result, Does.Contain("LOGO;TYPE=image/PNG:AAAA"));
            Assert.That(result, Does.Contain("PHOTO;TYPE=image/JPEG:BBBB"));
        });
    }

    [Test]
    public void GenerateVCardString_WithImageFormatButNoBase64_OmitsImageProperty()
    {
        var vCard = new VCardData
        {
            FullName = "Jane Smith",
            LogoFormat = "png",
            LogoBase64 = null
        };

        var result = VCardHelper.GenerateVCardString(vCard);

        Assert.That(result, Does.Not.Contain("LOGO"));
    }

    [Test]
    public void GenerateVCardString_WithLocations_AppendsAddressAndLabel()
    {
        var location = new VCardLocationData
        {
            Address = "123 Main St",
            SecondAddress = "Suite 100",
            City = "Springfield",
            State = "IL",
            Zip = "62701",
            Country = "USA"
        };

        var vCard = new VCardData
        {
            FullName = "Jane Smith",
            Locations = new[] { ("WORK", location) }
        };

        var result = VCardHelper.GenerateVCardString(vCard);

        Assert.Multiple(() =>
        {
            Assert.That(result, Does.Contain("ADR;CHARSET=utf-8;TYPE=WORK:;Suite 100;123 Main St;Springfield;IL;62701;USA"));
            Assert.That(result, Does.Contain("LABEL;CHARSET=utf-8;TYPE=WORK;ENCODING=QUOTED-PRINTABLE:123 Main St=0D=0ASuite 100=0D=0ASpringfield=0D=0AIL=0D=0A62701"));
        });
    }

    [Test]
    public void GenerateVCardString_WithEmptyCategories_StillIncludesCategoriesLine()
    {
        var vCard = new VCardData
        {
            FullName = "Jane Smith",
            Categories = new[] { "Single" }
        };

        var result = VCardHelper.GenerateVCardString(vCard);

        Assert.That(result, Does.Contain("CATEGORIES;CHARSET=utf-8:Single"));
    }

    [Test]
    public void GenerateVCardString_StartsWithBeginAndEndsWithEnd()
    {
        var vCard = new VCardData
        {
            FullName = "Jane Smith"
        };

        var result = VCardHelper.GenerateVCardString(vCard).Trim();

        Assert.Multiple(() =>
        {
            Assert.That(result, Does.StartWith("BEGIN:VCARD"));
            Assert.That(result, Does.EndWith("END:VCARD"));
        });
    }
}
