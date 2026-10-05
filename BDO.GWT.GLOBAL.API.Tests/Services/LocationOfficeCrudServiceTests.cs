using BDO.GWT.Common.Core.Exceptions;
using BDO.GWT.Core;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Helpers;
using BDO.GWT.GLOBAL.API.Services;
using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
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

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
            Assert.That(((ObjectResult)result.Result!).StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
        });
    }

    [Test]
    public async Task GetPagesAsync_WithNullCulture_WhenDependenciesUnavailable_ReturnsInternalServerError()
    {
        var service = CreateServiceWithoutConstructor();

        var result = await service.GetPagesAsync(1, null, null, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
            Assert.That(((ObjectResult)result.Result!).StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
        });
    }

    [Test]
    public async Task GetPageAsync_WhenDependenciesUnavailable_ReturnsInternalServerError()
    {
        var service = CreateServiceWithoutConstructor();

        var result = await service.GetPageAsync(1, Guid.NewGuid(), "en-gb", CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
            Assert.That(((ObjectResult)result.Result!).StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
        });
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

[TestFixture]
public class LocationOfficeCrudServiceBranchTests
{
    private Mock<IContentService> _contentRetrieverMock = null!;
    private Mock<IValidationState> _validationStateMock = null!;
    private Mock<ICultureService> _cultureServiceMock = null!;
    private Mock<IPageContentValidator> _pageContentValidatorMock = null!;
    private Mock<IInfoProvider<ChannelInfo>> _channelInfoProviderMock = null!;
    private Mock<IWebPageManager> _webPageManagerMock = null!;
    private Mock<IMappingHelper> _mappingHelperMock = null!;

    private static LanguageTag EmptyLocaleCulture => new("en", null, "GB", "");
    private static LanguageTag EmptyLanguageCulture => new("", null, "GB", "en-GB");

    [SetUp]
    public void Setup()
    {
        _contentRetrieverMock = new Mock<IContentService>();
        _validationStateMock = new Mock<IValidationState>();
        _cultureServiceMock = new Mock<ICultureService>();
        _pageContentValidatorMock = new Mock<IPageContentValidator>();
        _channelInfoProviderMock = new Mock<IInfoProvider<ChannelInfo>>();
        _webPageManagerMock = new Mock<IWebPageManager>();
        _mappingHelperMock = new Mock<IMappingHelper>();

        SetupCulture(LanguageTag.GB);
    }

    private LocationOfficeCrudService CreateService()
    {
        var service = (LocationOfficeCrudService)RuntimeHelpers.GetUninitializedObject(typeof(LocationOfficeCrudService));

        SetPrivateField(service, "_contentRetriever", _contentRetrieverMock.Object);
        SetPrivateField(service, "_validationState", _validationStateMock.Object);
        SetPrivateField(service, "_cultureService", _cultureServiceMock.Object);
        SetPrivateField(service, "_pageContentValidator", _pageContentValidatorMock.Object);
        SetPrivateField(service, "_channelInfoProvider", _channelInfoProviderMock.Object);
        SetPrivateField(service, "_webPageManager", _webPageManagerMock.Object);
        SetPrivateField(service, "_mappingHelper", _mappingHelperMock.Object);

        return service;
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null, $"Field '{fieldName}' was not found.");
        field!.SetValue(target, value);
    }

    private void SetupCulture(LanguageTag? culture) =>
        _mappingHelperMock
            .Setup(x => x.ValidateAndNormalizeSiteCultureAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(culture);

    private void SetupCultureSequence(LanguageTag first, LanguageTag second) =>
        _mappingHelperMock
            .SetupSequence(x => x.ValidateAndNormalizeSiteCultureAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(first)
            .ReturnsAsync(second);

    private void SetupChannel(string? channelName)
    {
        FakeChannelInfo? channel = null;
        if (channelName is not null)
        {
            channel = (FakeChannelInfo)RuntimeHelpers.GetUninitializedObject(typeof(FakeChannelInfo));
            channel.ChannelName = channelName;
        }

        _channelInfoProviderMock.As<IInfoByIdProvider<ChannelInfo>>()
            .Setup(x => x.GetAsync(It.IsAny<int>(), It.IsAny<CancellationToken?>()))
            .ReturnsAsync(channel!);
    }

    private void SetupChannelThrows(Exception exception) =>
        _channelInfoProviderMock.As<IInfoByIdProvider<ChannelInfo>>()
            .Setup(x => x.GetAsync(It.IsAny<int>(), It.IsAny<CancellationToken?>()))
            .ThrowsAsync(exception);

    private void SetupOffices(params IEnumerable<LocationOffice>?[] results)
    {
        var sequence = _contentRetrieverMock.SetupSequence(x => x.GetPagesAsync<LocationOffice>(
            It.IsAny<RetrievePagesParameters>(),
            It.IsAny<Action<RetrievePagesQueryParameters>>(),
            It.IsAny<RetrievalCacheSettings>(),
            It.IsAny<CancellationToken>()));

        foreach (var result in results)
        {
            sequence = sequence.ReturnsAsync(result!);
        }
    }

    private static LocationOffice CreateOffice(int id) =>
        new() { SystemFields = new WebPageFields { WebPageItemID = id, ContentItemName = "office" } };

    private static LocationOfficePageContent CreateRequest() => new()
    {
        MetadataTitle = "Office",
        MetadataTeaserImageAltText = "alt",
        OfficeImage = "img.png",
        OfficeImageAltText = "office alt",
        LocationAddressLine1 = "1 Street",
        LocationZip = "123",
        LocationZipDisplayType = "show",
        LocationCountry = "UK",
        LocationLatitude = "51.5",
        LocationLongitude = "-0.1",
        LocationPhone = "0123"
    };

    private static void AssertNotFoundMessage<T>(ActionResult<T> result, string message)
    {
        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
            Assert.That(((NotFoundObjectResult)result.Result!).Value, Is.EqualTo(message));
        });
    }

    private void SetupCreateFlow(int newPageId, bool updateDraft, bool publish)
    {
        _webPageManagerMock.Setup(x => x.Create(It.IsAny<CreateWebPageParameters>(), It.IsAny<CancellationToken>())).ReturnsAsync(newPageId);
        _webPageManagerMock.Setup(x => x.TryUpdateDraft(newPageId, It.IsAny<string>(), It.IsAny<UpdateDraftData>(), It.IsAny<CancellationToken>())).ReturnsAsync(updateDraft);
        _webPageManagerMock.Setup(x => x.TryPublish(newPageId, It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(publish);
    }

    [Test]
    public async Task GetPagesAsync_InvalidCulture_ReturnsNotFoundMessage()
    {
        SetupCulture(EmptyLocaleCulture);

        var result = await CreateService().GetPagesAsync(1, "en-gb", true, CancellationToken.None);

        AssertNotFoundMessage(result, "Invalid culture specified");
    }

    [Test]
    public async Task GetPagesAsync_ChannelNotFound_ReturnsNotFoundMessage()
    {
        SetupChannel(null);

        var result = await CreateService().GetPagesAsync(1, null, null, CancellationToken.None);

        AssertNotFoundMessage(result, "Channel not found");
    }

    [Test]
    public async Task GetPagesAsync_NullResults_ReturnsEmptyListAndPreviewWhenNotOnlyPublished()
    {
        SetupChannel("TestChannel");
        RetrievePagesParameters? captured = null;
        _contentRetrieverMock
            .Setup(x => x.GetPagesAsync<LocationOffice>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .Callback<RetrievePagesParameters, Action<RetrievePagesQueryParameters>, RetrievalCacheSettings, CancellationToken>(
                (p, _, _, _) => captured = p)
            .ReturnsAsync((IEnumerable<LocationOffice>)null!);

        var result = await CreateService().GetPagesAsync(1, "en-gb", false, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Value, Is.Empty);
            Assert.That(captured!.IsForPreview, Is.True);
            Assert.That(captured.ChannelName, Is.EqualTo("TestChannel"));
        });
    }

    [Test]
    public async Task GetPagesAsync_UnprocessableEntity_Returns422()
    {
        SetupChannelThrows(new UnprocessableEntityException());

        var result = await CreateService().GetPagesAsync(1, "en-gb", true, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<UnprocessableEntityObjectResult>());
    }

    [Test]
    public async Task GetPageAsync_PageMissing_ReturnsNotFoundResult()
    {
        SetupChannel("TestChannel");
        SetupOffices(Enumerable.Empty<LocationOffice>());

        var result = await CreateService().GetPageAsync(1, Guid.NewGuid(), "en-gb", CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task GetPageAsync_ChannelNotFound_ReturnsNotFoundMessage()
    {
        SetupChannel(string.Empty);

        var result = await CreateService().GetPageAsync(1, Guid.NewGuid(), null, CancellationToken.None);

        AssertNotFoundMessage(result, "Channel not found");
    }

    [Test]
    public async Task CreatePageAsync_InvalidCulture_ReturnsNotFoundMessage()
    {
        SetupCulture(EmptyLocaleCulture);

        var result = await CreateService().CreatePageAsync(1, 10, "en-gb", CreateRequest(), CancellationToken.None);

        AssertNotFoundMessage(result, "Invalid culture specified");
    }

    [Test]
    public async Task CreatePageAsync_ParentNull_Returns422AndAddsError()
    {
        SetupChannel("TestChannel");
        SetupOffices((IEnumerable<LocationOffice>?)null);

        var result = await CreateService().CreatePageAsync(1, 10, "en-gb", CreateRequest(), CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<UnprocessableEntityObjectResult>());
        _validationStateMock.Verify(x => x.AddUnprocessableEntityError("WebPageParentID", It.IsAny<string>()), Times.Once);
        _pageContentValidatorMock.Verify(x => x.ValidateAndNormalizePageContent(It.IsAny<LocationOfficePageContent>()), Times.Never);
    }

    [Test]
    public async Task CreatePageAsync_ValidationErrors_Returns500FromValidationException()
    {
        SetupChannel("TestChannel");
        SetupOffices(Enumerable.Empty<LocationOffice>());
        _validationStateMock.Setup(x => x.HasErrors()).Returns(true);

        var result = await CreateService().CreatePageAsync(1, 10, "en-gb", CreateRequest(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
            Assert.That(((ObjectResult)result.Result!).StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
        });
        _webPageManagerMock.Verify(x => x.Create(It.IsAny<CreateWebPageParameters>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task CreatePageAsync_UnprocessableValidationErrors_Returns422()
    {
        SetupChannel("TestChannel");
        SetupOffices(Enumerable.Empty<LocationOffice>());
        _validationStateMock.Setup(x => x.HasUnprocessableEntityErrors()).Returns(true);

        var result = await CreateService().CreatePageAsync(1, 10, "en-gb", CreateRequest(), CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<UnprocessableEntityObjectResult>());
    }

    [Test]
    public async Task CreatePageAsync_UpdateDraftFails_ReturnsNotFoundMessage()
    {
        SetupChannel("TestChannel");
        SetupOffices(Enumerable.Empty<LocationOffice>());
        SetupCreateFlow(5, updateDraft: false, publish: true);

        var result = await CreateService().CreatePageAsync(1, 10, "en-gb", CreateRequest(), CancellationToken.None);

        AssertNotFoundMessage(result, "Failed to update page draft");
    }

    [Test]
    public async Task CreatePageAsync_PublishFails_ReturnsNotFoundMessage()
    {
        SetupChannel("TestChannel");
        SetupOffices(Enumerable.Empty<LocationOffice>());
        SetupCreateFlow(5, updateDraft: true, publish: false);

        var result = await CreateService().CreatePageAsync(1, 10, "en-gb", CreateRequest(), CancellationToken.None);

        AssertNotFoundMessage(result, "Failed to publish updated page");
    }

    [Test]
    public async Task CreatePageAsync_CreatedPageNotFound_ReturnsBadRequest_AndUsesTitleAndParent()
    {
        SetupChannel("TestChannel");
        SetupOffices(Enumerable.Empty<LocationOffice>(), Enumerable.Empty<LocationOffice>());
        SetupCreateFlow(5, updateDraft: true, publish: true);
        CreateWebPageParameters? captured = null;
        _webPageManagerMock
            .Setup(x => x.Create(It.IsAny<CreateWebPageParameters>(), It.IsAny<CancellationToken>()))
            .Callback<CreateWebPageParameters, CancellationToken>((p, _) => captured = p)
            .ReturnsAsync(5);

        var result = await CreateService().CreatePageAsync(1, 10, "en-gb", CreateRequest(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<BadRequestResult>());
            Assert.That(captured!.ParentWebPageItemID, Is.EqualTo(10));
            Assert.That(captured.DisplayName, Is.EqualTo("Office"));
        });
    }

    [Test]
    public async Task CreatePageAsync_UnexpectedException_Returns500WithMessage()
    {
        SetupChannelThrows(new InvalidOperationException("boom"));

        var result = await CreateService().CreatePageAsync(1, 10, "en-gb", CreateRequest(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(((ObjectResult)result.Result!).StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
            Assert.That(((ObjectResult)result.Result!).Value, Is.EqualTo("boom"));
        });
    }

    [Test]
    public async Task CreatePageCultureVersionAsync_InvalidFromCulture_ReturnsNotFoundMessage()
    {
        SetupCulture(EmptyLanguageCulture);

        var result = await CreateService().CreatePageCultureVersionAsync(1, Guid.NewGuid(), "en-gb", "fr-fr", CreateRequest(), CancellationToken.None);

        AssertNotFoundMessage(result, "Invalid from culture specified");
    }

    [Test]
    public async Task CreatePageCultureVersionAsync_InvalidToCulture_ReturnsNotFoundMessage()
    {
        SetupCultureSequence(LanguageTag.GB, EmptyLanguageCulture);

        var result = await CreateService().CreatePageCultureVersionAsync(1, Guid.NewGuid(), "en-gb", "fr-fr", CreateRequest(), CancellationToken.None);

        AssertNotFoundMessage(result, "Invalid to culture specified");
    }

    [Test]
    public async Task CreatePageCultureVersionAsync_SourceMissing_ReturnsNotFoundResult()
    {
        SetupChannel("TestChannel");
        SetupOffices(Enumerable.Empty<LocationOffice>());

        var result = await CreateService().CreatePageCultureVersionAsync(1, Guid.NewGuid(), null, null, CreateRequest(), CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task CreatePageCultureVersionAsync_SystemFieldsMissing_ReturnsNotFoundMessage()
    {
        SetupChannel("TestChannel");
        SetupOffices(new[] { new LocationOffice { SystemFields = null! } });

        var result = await CreateService().CreatePageCultureVersionAsync(1, Guid.NewGuid(), "en-gb", "en-gb", CreateRequest(), CancellationToken.None);

        AssertNotFoundMessage(result, "Page system fields are missing");
    }

    [Test]
    public async Task CreatePageCultureVersionAsync_VariantCreationFails_DoesNotPublish()
    {
        SetupChannel("TestChannel");
        SetupOffices(new[] { CreateOffice(8) });
        _webPageManagerMock.Setup(x => x.TryCreateLanguageVariant(It.IsAny<CMS.Websites.CreateLanguageVariantParameters>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await CreateService().CreatePageCultureVersionAsync(1, Guid.NewGuid(), "en-gb", "en-gb", CreateRequest(), CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
        _webPageManagerMock.Verify(x => x.TryPublish(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task CreatePageCultureVersionAsync_PublishFails_ReturnsNotFoundMessage()
    {
        SetupChannel("TestChannel");
        SetupOffices(new[] { CreateOffice(8) });
        _webPageManagerMock.Setup(x => x.TryCreateLanguageVariant(It.IsAny<CMS.Websites.CreateLanguageVariantParameters>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _webPageManagerMock.Setup(x => x.TryUpdateDraft(8, It.IsAny<string>(), It.IsAny<UpdateDraftData>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _webPageManagerMock.Setup(x => x.TryPublish(8, It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await CreateService().CreatePageCultureVersionAsync(1, Guid.NewGuid(), "en-gb", "en-gb", CreateRequest(), CancellationToken.None);

        AssertNotFoundMessage(result, "Failed to publish updated page");
    }

    [Test]
    public async Task CreatePageCultureVersionAsync_VariantNotFoundAfterPublish_ReturnsBadRequest()
    {
        SetupChannel("TestChannel");
        SetupOffices(new[] { CreateOffice(8) }, Enumerable.Empty<LocationOffice>());
        _webPageManagerMock.Setup(x => x.TryCreateLanguageVariant(It.IsAny<CMS.Websites.CreateLanguageVariantParameters>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _webPageManagerMock.Setup(x => x.TryUpdateDraft(8, It.IsAny<string>(), It.IsAny<UpdateDraftData>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _webPageManagerMock.Setup(x => x.TryPublish(8, It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await CreateService().CreatePageCultureVersionAsync(1, Guid.NewGuid(), "en-gb", "en-gb", CreateRequest(), CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<BadRequestResult>());
    }

    [Test]
    public async Task UpdatePageAsync_PageMissing_ReturnsNotFoundResult()
    {
        SetupChannel("TestChannel");
        SetupOffices((IEnumerable<LocationOffice>?)null);

        var result = await CreateService().UpdatePageAsync(1, Guid.NewGuid(), "en-gb", CreateRequest(), CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task UpdatePageAsync_SystemFieldsMissing_ReturnsNotFoundMessage()
    {
        SetupChannel("TestChannel");
        SetupOffices(new[] { new LocationOffice { SystemFields = null! } });

        var result = await CreateService().UpdatePageAsync(1, Guid.NewGuid(), "en-gb", CreateRequest(), CancellationToken.None);

        AssertNotFoundMessage(result, "Page system fields are missing");
    }

    [Test]
    public async Task UpdatePageAsync_UpdateDraftFails_CreatesDraftThenReturnsNotFound()
    {
        SetupChannel("TestChannel");
        SetupOffices(new[] { CreateOffice(3) });
        _webPageManagerMock.Setup(x => x.TryUpdateDraft(3, It.IsAny<string>(), It.IsAny<UpdateDraftData>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await CreateService().UpdatePageAsync(1, Guid.NewGuid(), "en-gb", CreateRequest(), CancellationToken.None);

        AssertNotFoundMessage(result, "Failed to update page draft");
        _webPageManagerMock.Verify(x => x.TryCreateDraft(3, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task UpdatePageAsync_PublishFails_ReturnsNotFoundMessage()
    {
        SetupChannel("TestChannel");
        SetupOffices(new[] { CreateOffice(3) });
        _webPageManagerMock.Setup(x => x.TryUpdateDraft(3, It.IsAny<string>(), It.IsAny<UpdateDraftData>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _webPageManagerMock.Setup(x => x.TryPublish(3, It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await CreateService().UpdatePageAsync(1, Guid.NewGuid(), "en-gb", CreateRequest(), CancellationToken.None);

        AssertNotFoundMessage(result, "Failed to publish updated page");
    }

    [Test]
    public async Task UpdatePageAsync_UpdatedPageNotFound_ReturnsBadRequest()
    {
        SetupChannel("TestChannel");
        SetupOffices(new[] { CreateOffice(3) }, Enumerable.Empty<LocationOffice>());
        _webPageManagerMock.Setup(x => x.TryUpdateDraft(3, It.IsAny<string>(), It.IsAny<UpdateDraftData>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _webPageManagerMock.Setup(x => x.TryPublish(3, It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await CreateService().UpdatePageAsync(1, Guid.NewGuid(), "en-gb", CreateRequest(), CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<BadRequestResult>());
    }

    [Test]
    public void DeletePageAsync_InvalidCulture_Throws()
    {
        SetupCulture(EmptyLocaleCulture);

        var ex = Assert.ThrowsAsync<NotFoundException>(() =>
            CreateService().DeletePageAsync(1, Guid.NewGuid(), "en-gb", false, CancellationToken.None));

        Assert.That(ex!.Message, Is.EqualTo("Invalid culture specified"));
    }

    [Test]
    public void DeletePageAsync_ChannelNotFound_Throws()
    {
        SetupChannel(null);

        var ex = Assert.ThrowsAsync<NotFoundException>(() =>
            CreateService().DeletePageAsync(1, Guid.NewGuid(), null, false, CancellationToken.None));

        Assert.That(ex!.Message, Is.EqualTo("Channel not found"));
    }

    [Test]
    public void DeletePageAsync_SingleCulture_PageMissing_Throws()
    {
        SetupChannel("TestChannel");
        SetupOffices(Enumerable.Empty<LocationOffice>());

        Assert.ThrowsAsync<NotFoundException>(() =>
            CreateService().DeletePageAsync(1, Guid.NewGuid(), "en-gb", false, CancellationToken.None));
    }

    [Test]
    public async Task DeletePageAsync_SingleCulture_DeletesPermanently()
    {
        SetupChannel("TestChannel");
        SetupOffices(new[] { CreateOffice(12) });
        DeleteWebPageParameters? captured = null;
        _webPageManagerMock
            .Setup(x => x.Delete(It.IsAny<DeleteWebPageParameters>(), It.IsAny<CancellationToken>()))
            .Callback<DeleteWebPageParameters, CancellationToken>((p, _) => captured = p)
            .Returns(Task.CompletedTask);

        await CreateService().DeletePageAsync(1, Guid.NewGuid(), "en-gb", false, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(captured!.WebPageItemID, Is.EqualTo(12));
            Assert.That(captured.Permanently, Is.True);
        });
    }

    [Test]
    public async Task DeletePageAsync_AllCultures_DeletesOnlyFoundVariants()
    {
        SetupChannel("TestChannel");
        _cultureServiceMock
            .Setup(x => x.GetSiteCulturesLanguageCodeAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<LanguageTag> { LanguageTag.GB, LanguageTag.GB });
        SetupOffices(new[] { CreateOffice(4) }, (IEnumerable<LocationOffice>?)null);

        await CreateService().DeletePageAsync(1, Guid.NewGuid(), "en-gb", true, CancellationToken.None);

        _webPageManagerMock.Verify(x => x.Delete(It.IsAny<DeleteWebPageParameters>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    private sealed class FakeChannelInfo : ChannelInfo
    {
        private string? channelName;

        public override string ChannelName
        {
            get => channelName ?? string.Empty;
            set => channelName = value;
        }
    }
}
