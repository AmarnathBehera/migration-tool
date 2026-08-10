using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace BDO.GWT.GLOBAL.API.Tests.Services;

[TestFixture]
public class LocationOfficeCrudServiceTests
{
    private static LocationOfficeCrudService CreateServiceWithoutConstructor() =>
        (LocationOfficeCrudService)RuntimeHelpers.GetUninitializedObject(typeof(LocationOfficeCrudService));

    [Test]
    public void LocationOfficeCrudService_ImplementsILocationOfficeCrudService()
    {
        var service = CreateServiceWithoutConstructor();

        Assert.That(service, Is.InstanceOf<ILocationOfficeCrudService>());
    }

    [Test]
    public async Task CreatePageAsync_WithNullRequest_ReturnsBadRequest()
    {
        var service = CreateServiceWithoutConstructor();

        var result = await service.CreatePageAsync(
            channelId: 1,
            WebPageParentID: 10,
            culture: "en-gb",
            request: null!,
            cancellationToken: CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<BadRequestResult>());
    }

    [Test]
    public async Task CreatePageCultureVersionAsync_WithNullRequest_ReturnsBadRequest()
    {
        var service = CreateServiceWithoutConstructor();

        var result = await service.CreatePageCultureVersionAsync(
            channelId: 1,
            WebPageItemGuid: Guid.NewGuid(),
            fromCulture: "en-gb",
            toCulture: "fr-fr",
            request: null!,
            cancellationToken: CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<BadRequestResult>());
    }

    [Test]
    public async Task UpdatePageAsync_WithNullRequest_ReturnsBadRequest()
    {
        var service = CreateServiceWithoutConstructor();

        var result = await service.UpdatePageAsync(
            channelId: 1,
            WebPageItemGuid: Guid.NewGuid(),
            culture: "en-gb",
            request: null!,
            cancellationToken: CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<BadRequestResult>());
    }

    [Test]
    public void LocationOfficePageContent_DefaultValues_AreInitialized()
    {
        var content = new LocationOfficePageContent
        {
            MetadataTitle = "Test Office",
            MetadataTeaserImageAltText = "alt",
            OfficeImage = "img.png",
            OfficeImageAltText = "office alt",
            LocationAddressLine1 = "123 Street",
            LocationZip = "12345",
            LocationZipDisplayType = "show",
            LocationCountry = "UK",
            LocationLatitude = "51.5",
            LocationLongitude = "-0.1",
            LocationPhone = "0123456789"
        };

        Assert.That(content, Is.Not.Null);
    }

    [Test]
    public void LocationOfficePageContent_StoresAllRequiredProperties()
    {
        var content = new LocationOfficePageContent
        {
            MetadataTitle = "Test Office",
            MetadataDescription = "Description",
            MetadataTeaserImage = "teaser.png",
            MetadataTeaserImageAltText = "teaser alt",
            MetadataCanonical = "https://example.com/office",
            OfficeImage = "office.png",
            OfficeImageAltText = "office alt",
            LocationAddressLine1 = "123 Street",
            LocationAddressLine2 = "Suite 100",
            LocationZip = "12345",
            LocationZipDisplayType = "show",
            LocationCountry = "UK",
            LocationLatitude = "51.5074",
            LocationLongitude = "-0.1278",
            LocationEmail = "office@example.com",
            LocationType = 1,
            LocationPhone = "0123456789",
            LocationFax = "0123456788",
            OpeningHours = "9am - 5pm",
            LocationPOBoxNumber = "PO Box 1",
            LocationPOCity = "London",
            LocationPOZip = "SW1A 1AA",
            LocationPOZipDisplayType = "show",
            LocationPOCountry = "UK",
            MetadataOGTitle = "OG Title",
            MetadataOGDescription = "OG Description",
            MetadataOGImage = "og.png",
            MetadataOGTwitterImageSizeCheckbox = true,
            MetadataOGTwitterImage = "twitter.png",
            MetadataNofollow = true,
            MetadataNoindex = false,
            IncludeInSitemap = true
        };

        Assert.Multiple(() =>
        {
            Assert.That(content.MetadataTitle, Is.EqualTo("Test Office"));
            Assert.That(content.MetadataDescription, Is.EqualTo("Description"));
            Assert.That(content.MetadataTeaserImage, Is.EqualTo("teaser.png"));
            Assert.That(content.MetadataTeaserImageAltText, Is.EqualTo("teaser alt"));
            Assert.That(content.MetadataCanonical, Is.EqualTo("https://example.com/office"));
            Assert.That(content.OfficeImage, Is.EqualTo("office.png"));
            Assert.That(content.OfficeImageAltText, Is.EqualTo("office alt"));
            Assert.That(content.LocationAddressLine1, Is.EqualTo("123 Street"));
            Assert.That(content.LocationAddressLine2, Is.EqualTo("Suite 100"));
            Assert.That(content.LocationZip, Is.EqualTo("12345"));
            Assert.That(content.LocationZipDisplayType, Is.EqualTo("show"));
            Assert.That(content.LocationCountry, Is.EqualTo("UK"));
            Assert.That(content.LocationLatitude, Is.EqualTo("51.5074"));
            Assert.That(content.LocationLongitude, Is.EqualTo("-0.1278"));
            Assert.That(content.LocationEmail, Is.EqualTo("office@example.com"));
            Assert.That(content.LocationType, Is.EqualTo(1));
            Assert.That(content.LocationPhone, Is.EqualTo("0123456789"));
            Assert.That(content.LocationFax, Is.EqualTo("0123456788"));
            Assert.That(content.OpeningHours, Is.EqualTo("9am - 5pm"));
            Assert.That(content.LocationPOBoxNumber, Is.EqualTo("PO Box 1"));
            Assert.That(content.LocationPOCity, Is.EqualTo("London"));
            Assert.That(content.LocationPOZip, Is.EqualTo("SW1A 1AA"));
            Assert.That(content.LocationPOZipDisplayType, Is.EqualTo("show"));
            Assert.That(content.LocationPOCountry, Is.EqualTo("UK"));
            Assert.That(content.MetadataOGTitle, Is.EqualTo("OG Title"));
            Assert.That(content.MetadataOGDescription, Is.EqualTo("OG Description"));
            Assert.That(content.MetadataOGImage, Is.EqualTo("og.png"));
            Assert.That(content.MetadataOGTwitterImageSizeCheckbox, Is.True);
            Assert.That(content.MetadataOGTwitterImage, Is.EqualTo("twitter.png"));
            Assert.That(content.MetadataNofollow, Is.True);
            Assert.That(content.MetadataNoindex, Is.False);
            Assert.That(content.IncludeInSitemap, Is.True);
        });
    }

    [Test]
    public void LocationOfficePageContent_NullableProperties_AreNullByDefault()
    {
        var content = new LocationOfficePageContent
        {
            MetadataTitle = "Test",
            MetadataTeaserImageAltText = "alt",
            OfficeImage = "img.png",
            OfficeImageAltText = "office alt",
            LocationAddressLine1 = "123 Street",
            LocationZip = "12345",
            LocationZipDisplayType = "show",
            LocationCountry = "UK",
            LocationLatitude = "51.5",
            LocationLongitude = "-0.1",
            LocationPhone = "0123456789"
        };

        Assert.Multiple(() =>
        {
            Assert.That(content.MetadataDescription, Is.Null);
            Assert.That(content.MetadataTeaserImage, Is.Null);
            Assert.That(content.MetadataCanonical, Is.Null);
            Assert.That(content.LocationAddressLine2, Is.Null);
            Assert.That(content.LocationEmail, Is.Null);
            Assert.That(content.LocationFax, Is.Null);
            Assert.That(content.OpeningHours, Is.Null);
            Assert.That(content.LocationPOBoxNumber, Is.Null);
            Assert.That(content.LocationPOCity, Is.Null);
            Assert.That(content.LocationPOZip, Is.Null);
            Assert.That(content.LocationPOZipDisplayType, Is.Null);
            Assert.That(content.LocationPOCountry, Is.Null);
            Assert.That(content.MetadataOGTitle, Is.Null);
            Assert.That(content.MetadataOGDescription, Is.Null);
            Assert.That(content.MetadataOGImage, Is.Null);
            Assert.That(content.MetadataOGTwitterImage, Is.Null);
            Assert.That(content.PublishFrom, Is.Null);
            Assert.That(content.PublishTo, Is.Null);
            Assert.That(content.RedirectOnUnpublish, Is.Null);
        });
    }

    [Test]
    public void LocationOfficePageContent_BooleanDefaults_AreFalse()
    {
        var content = new LocationOfficePageContent
        {
            MetadataTitle = "Test",
            MetadataTeaserImageAltText = "alt",
            OfficeImage = "img.png",
            OfficeImageAltText = "office alt",
            LocationAddressLine1 = "123 Street",
            LocationZip = "12345",
            LocationZipDisplayType = "show",
            LocationCountry = "UK",
            LocationLatitude = "51.5",
            LocationLongitude = "-0.1",
            LocationPhone = "0123456789"
        };

        Assert.Multiple(() =>
        {
            Assert.That(content.MetadataOGTwitterImageSizeCheckbox, Is.False);
            Assert.That(content.MetadataNofollow, Is.False);
            Assert.That(content.MetadataNoindex, Is.False);
            Assert.That(content.IncludeInSitemap, Is.False);
        });
    }

    [Test]
    public void PageOutput_LocationOfficePageContent_CanBeConstructed()
    {
        var guid = Guid.NewGuid();
        var content = new LocationOfficePageContent
        {
            MetadataTitle = "Test Office",
            MetadataTeaserImageAltText = "alt",
            OfficeImage = "img.png",
            OfficeImageAltText = "office alt",
            LocationAddressLine1 = "123 Street",
            LocationZip = "12345",
            LocationZipDisplayType = "show",
            LocationCountry = "UK",
            LocationLatitude = "51.5",
            LocationLongitude = "-0.1",
            LocationPhone = "0123456789"
        };
        var output = new PageOutput<LocationOfficePageContent>
        {
            General = new PageGeneralOutput { PageGuid = guid },
            Content = content
        };

        Assert.Multiple(() =>
        {
            Assert.That(output.General.PageGuid, Is.EqualTo(guid));
            Assert.That(output.Content, Is.SameAs(content));
        });
    }

    [Test]
    public async Task GetPagesAsync_WhenDependenciesUnavailable_ReturnsInternalServerError()
    {
        var service = CreateServiceWithoutConstructor();

        var result = await service.GetPagesAsync(1, "en-gb", true, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
        Assert.That(((ObjectResult)result.Result!).StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
    }

    [Test]
    public async Task GetPagesAsync_WithNullCulture_WhenDependenciesUnavailable_ReturnsInternalServerError()
    {
        var service = CreateServiceWithoutConstructor();

        var result = await service.GetPagesAsync(1, null, null, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
        Assert.That(((ObjectResult)result.Result!).StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
    }

    [Test]
    public async Task GetPageAsync_WhenDependenciesUnavailable_ReturnsInternalServerError()
    {
        var service = CreateServiceWithoutConstructor();

        var result = await service.GetPageAsync(1, Guid.NewGuid(), "en-gb", CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
        Assert.That(((ObjectResult)result.Result!).StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
    }

    [Test]
    public void DeletePageAsync_WhenDependenciesUnavailable_Throws()
    {
        var service = CreateServiceWithoutConstructor();

        Assert.ThrowsAsync<NullReferenceException>(async () =>
            await service.DeletePageAsync(1, Guid.NewGuid(), "en-gb", false, CancellationToken.None));
    }

    [Test]
    public void DeletePageAsync_WithDeleteAllCultures_WhenDependenciesUnavailable_Throws()
    {
        var service = CreateServiceWithoutConstructor();

        Assert.ThrowsAsync<NullReferenceException>(async () =>
            await service.DeletePageAsync(1, Guid.NewGuid(), "en-gb", true, CancellationToken.None));
    }

    [Test]
    public void LocationOfficeCrudService_Interface_HasExpectedMethods()
    {
        var interfaceType = typeof(ILocationOfficeCrudService);

        Assert.Multiple(() =>
        {
            Assert.That(interfaceType.GetMethod("GetPagesAsync"), Is.Not.Null);
            Assert.That(interfaceType.GetMethod("GetPageAsync"), Is.Not.Null);
            Assert.That(interfaceType.GetMethod("CreatePageAsync"), Is.Not.Null);
            Assert.That(interfaceType.GetMethod("CreatePageCultureVersionAsync"), Is.Not.Null);
            Assert.That(interfaceType.GetMethod("UpdatePageAsync"), Is.Not.Null);
            Assert.That(interfaceType.GetMethod("DeletePageAsync"), Is.Not.Null);
        });
    }
}
