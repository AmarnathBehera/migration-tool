using BDO.GWT.Core.ValueObjects;
using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace BDO.GWT.GLOBAL.API.Tests.Services;

public class PersonCrudServiceTests
{
    private static PersonCrudService CreateServiceWithoutConstructor()
    {
        return (PersonCrudService)RuntimeHelpers.GetUninitializedObject(typeof(PersonCrudService));
    }

    private static MethodInfo GetPrivateMethod(string name) =>
        typeof(PersonCrudService).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new MissingMethodException(nameof(PersonCrudService), name);

    private static PersonPageContent CreateFullPersonPageContent()
    {
        return new PersonPageContent
        {
            MetadataTitle = "Jane Doe",
            PersonFirstName = "Jane",
            PersonSurnamePrefixes = "van der",
            PersonLastName = "Doe",
            MetadataTeaserImage = "teaser.jpg",
            MetadataTeaserImageAltText = "Jane Doe headshot",
            MetadataCanonical = "https://example.com/people/jane-doe",
            MetadataDescription = "Partner in the audit practice.",
            PersonJobTitle = "Partner",
            PersonPhone = "+1 555 0100",
            PersonMobilePhone = "+1 555 0101",
            PersonDirectDialNumber = "+1 555 0102",
            PersonEmail = "jane.doe@example.com",
            EnableForm = true,
            EnableVCard = true,
            PersonCredentials = new List<string> { "CPA", "MBA" },
            MetadataCity = new List<string> { "London" },
            MetadataOfficeLocation = new List<string> { "London Office" },
            MetadataProfileGroups = new List<string> { "Audit" },
            MetadataBusinessLines = new List<string> { "Assurance" },
            MetadataServiceAreas = new List<string> { "Financial Services" },
            MetadataServices = new List<string> { "Audit Services" },
            MetadataSubServices = new List<string> { "External Audit" },
            MetadataIndustryCategories = new List<string> { "Financial" },
            MetadataIndustries = new List<string> { "Banking" },
            MetadataIndustryServices = new List<string> { "Banking Audit" },
            MetadataSpecialtiesCategories = new List<string> { "Risk" },
            MetadataSpecialtiesAreas = new List<string> { "Regulatory Risk" },
            MetadataSpecialtiesPages = new List<string> { "Risk Advisory" },
            MicrositeMasterPages = new List<string> { "Global People" },
            MicrositePages = new List<string> { "Leadership" },
            Facebook = "https://facebook.com/janedoe",
            Twitter = "https://twitter.com/janedoe",
            LinkedIn = "https://linkedin.com/in/janedoe",
            MetadataOGTitle = "Jane Doe | BDO",
            MetadataOGDescription = "Meet Jane Doe, Partner at BDO.",
            MetadataOGImage = "og.jpg",
            MetadataOGTwitterImageSizeCheckbox = true,
            MetadataOGTwitterImage = "og-twitter.jpg",
            MetadataNofollow = false,
            MetadataNoindex = false,
            IncludeInSitemap = true,
            PublishFrom = new DateTime(2026, 1, 1),
            PublishTo = new DateTime(2027, 1, 1),
            RedirectOnUnpublish = "/people",
        };
    }

    [Test]
    public async Task CreatePageAsync_WithNullRequest_ReturnsBadRequestResult()
    {
        var service = CreateServiceWithoutConstructor();

        ActionResult<PageOutput<PersonPageContent>> result = await service.CreatePageAsync(
            channelId: 1,
            WebPageParentID: 10,
            culture: "en-gb",
            request: null!,
            cancellationToken: CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<BadRequestResult>());
    }

    [Test]
    public async Task CreatePageCultureVersionAsync_WithNullRequest_ReturnsBadRequestResult()
    {
        var service = CreateServiceWithoutConstructor();

        ActionResult<PageOutput<PersonPageContent>> result = await service.CreatePageCultureVersionAsync(
            channelId: 1,
            WebPageItemGuid: Guid.NewGuid(),
            fromCulture: "en-gb",
            toCulture: "en-us",
            request: null!,
            cancellationToken: CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<BadRequestResult>());
    }

    [Test]
    public async Task UpdatePageAsync_WithNullRequest_ReturnsBadRequestResult()
    {
        var service = CreateServiceWithoutConstructor();

        ActionResult<PageOutput<PersonPageContent>> result = await service.UpdatePageAsync(
            channelId: 1,
            WebPageItemGuid: Guid.NewGuid(),
            culture: "en-gb",
            request: null!,
            cancellationToken: CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<BadRequestResult>());
    }

    [Test]
    public async Task GetPersonPagesAsync_WhenDependenciesUnavailable_Returns500()
    {
        var service = CreateServiceWithoutConstructor();

        ActionResult<IEnumerable<PageOutput<PersonPageContent>>> result = await service.GetPersonPagesAsync(
            channelId: 1,
            culture: "en-gb",
            onlyPublished: true,
            cancellationToken: CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result.Result!;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
    }

    [Test]
    public async Task GetPersonPageAsync_WhenDependenciesUnavailable_Returns500()
    {
        var service = CreateServiceWithoutConstructor();

        ActionResult<PageOutput<PersonPageContent>> result = await service.GetPersonPageAsync(
            channelId: 1,
            WebPageItemGuid: Guid.NewGuid(),
            culture: "en-gb",
            cancellationToken: CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result.Result!;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
    }

    [Test]
    public async Task CreatePageAsync_WhenDependenciesUnavailable_Returns500()
    {
        var service = CreateServiceWithoutConstructor();
        var request = CreateFullPersonPageContent();

        ActionResult<PageOutput<PersonPageContent>> result = await service.CreatePageAsync(
            channelId: 1,
            WebPageParentID: 10,
            culture: "en-gb",
            request: request,
            cancellationToken: CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result.Result!;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
    }

    [Test]
    public async Task CreatePageCultureVersionAsync_WhenDependenciesUnavailable_Returns500()
    {
        var service = CreateServiceWithoutConstructor();
        var request = CreateFullPersonPageContent();

        ActionResult<PageOutput<PersonPageContent>> result = await service.CreatePageCultureVersionAsync(
            channelId: 1,
            WebPageItemGuid: Guid.NewGuid(),
            fromCulture: "en-gb",
            toCulture: "fr-fr",
            request: request,
            cancellationToken: CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result.Result!;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
    }

    [Test]
    public async Task UpdatePageAsync_WhenDependenciesUnavailable_Returns500()
    {
        var service = CreateServiceWithoutConstructor();
        var request = CreateFullPersonPageContent();

        ActionResult<PageOutput<PersonPageContent>> result = await service.UpdatePageAsync(
            channelId: 1,
            WebPageItemGuid: Guid.NewGuid(),
            culture: "en-gb",
            request: request,
            cancellationToken: CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result.Result!;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
    }

    [Test]
    public void DeletePageAsync_WhenDependenciesUnavailable_Throws()
    {
        var service = CreateServiceWithoutConstructor();

        Assert.ThrowsAsync<NullReferenceException>(async () => await service.DeletePageAsync(
            channelId: 1,
            WebPageItemGuid: Guid.NewGuid(),
            culture: "en-gb",
            deleteAllCultures: false,
            cancellationToken: CancellationToken.None));
    }

    [Test]
    public async Task CreatePageAsync_WithInvalidCultureFormat_Returns500()
    {
        var service = CreateServiceWithoutConstructor();
        var request = CreateFullPersonPageContent();

        ActionResult<PageOutput<PersonPageContent>> result = await service.CreatePageAsync(
            channelId: 1,
            WebPageParentID: 10,
            culture: "not-a-culture!!",
            request: request,
            cancellationToken: CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result.Result!;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
    }

    [Test]
    public void PersonCrudService_Implements_IPersonCrudService()
    {
        Assert.That(typeof(IPersonCrudService).IsAssignableFrom(typeof(PersonCrudService)), Is.True);
    }

    [TestCase(0, 0, null)]
    [TestCase(-1, -1, "")]
    [TestCase(int.MaxValue, int.MaxValue, "fr-fr")]
    public async Task CreatePageAsync_WithNullRequest_ReturnsBadRequestResult_Variants(int channelId, int nodeParentID, string? culture)
    {
        var service = CreateServiceWithoutConstructor();

        var result = await service.CreatePageAsync(
            channelId,
            nodeParentID,
            culture,
            request: null!,
            CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<BadRequestResult>());
    }

    [TestCase(null, null)]
    [TestCase("en-gb", "fr-fr")]
    [TestCase("", "")]
    public async Task CreatePageCultureVersionAsync_WithNullRequest_ReturnsBadRequestResult_Variants(string? fromCulture, string? toCulture)
    {
        var service = CreateServiceWithoutConstructor();

        var result = await service.CreatePageCultureVersionAsync(
            channelId: 1,
            WebPageItemGuid: Guid.NewGuid(),
            fromCulture,
            toCulture,
            request: null!,
            CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<BadRequestResult>());
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("en-gb")]
    [TestCase("es-es")]
    public async Task UpdatePageAsync_WithNullRequest_ReturnsBadRequestResult_Variants(string? culture)
    {
        var service = CreateServiceWithoutConstructor();

        var result = await service.UpdatePageAsync(
            channelId: 1,
            WebPageItemGuid: Guid.NewGuid(),
            culture,
            request: null!,
            CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<BadRequestResult>());
    }

    [Test]
    public async Task CreatePageAsync_WithNullRequest_HonorsCancelledToken_StillReturnsBadRequest()
    {
        var service = CreateServiceWithoutConstructor();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await service.CreatePageAsync(
            channelId: 1,
            WebPageParentID: 10,
            culture: "en-gb",
            request: null!,
            cts.Token);

        Assert.That(result.Result, Is.InstanceOf<BadRequestResult>());
    }

    [Test]
    public async Task UpdatePageAsync_WithNullRequest_HonorsCancelledToken_StillReturnsBadRequest()
    {
        var service = CreateServiceWithoutConstructor();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await service.UpdatePageAsync(
            channelId: 1,
            WebPageItemGuid: Guid.NewGuid(),
            culture: "en-gb",
            request: null!,
            cts.Token);

        Assert.That(result.Result, Is.InstanceOf<BadRequestResult>());
    }

    [Test]
    public async Task CreatePageCultureVersionAsync_WithNullRequest_HonorsCancelledToken_StillReturnsBadRequest()
    {
        var service = CreateServiceWithoutConstructor();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await service.CreatePageCultureVersionAsync(
            channelId: 1,
            WebPageItemGuid: Guid.NewGuid(),
            fromCulture: "en-gb",
            toCulture: "fr-fr",
            request: null!,
            cts.Token);

        Assert.That(result.Result, Is.InstanceOf<BadRequestResult>());
    }

    [Test]
    public async Task CreatePageAsync_WithNullRequest_ReturnsResult_WithNullValue()
    {
        var service = CreateServiceWithoutConstructor();

        var result = await service.CreatePageAsync(1, 1, "en-gb", null!, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Result, Is.Not.Null);
            Assert.That(result.Value, Is.Null);
        });
    }

    [Test]
    public void DeletePageAsync_AllCultures_WhenDependenciesUnavailable_Throws()
    {
        var service = CreateServiceWithoutConstructor();

        Assert.ThrowsAsync<NullReferenceException>(async () => await service.DeletePageAsync(
            channelId: 1,
            WebPageItemGuid: Guid.NewGuid(),
            culture: "en-gb",
            deleteAllCultures: true,
            cancellationToken: CancellationToken.None));
    }

    [TestCase("en-gb")]
    [TestCase("fr-fr")]
    [TestCase("es-es")]
    public async Task GetPersonPageAsync_WhenDependenciesUnavailable_Returns500_ForAnyCulture(string culture)
    {
        var service = CreateServiceWithoutConstructor();

        var result = await service.GetPersonPageAsync(
            channelId: 1,
            WebPageItemGuid: Guid.NewGuid(),
            culture,
            CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result.Result!;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
    }

    [TestCase(true)]
    [TestCase(false)]
    [TestCase(null)]
    public async Task GetPersonPagesAsync_WhenDependenciesUnavailable_Returns500_ForAnyPublishedFlag(bool? onlyPublished)
    {
        var service = CreateServiceWithoutConstructor();

        var result = await service.GetPersonPagesAsync(
            channelId: 1,
            culture: "en-gb",
            onlyPublished,
            CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result.Result!;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
    }

    [Test]
    public async Task DeletePageAsync_WithEmptyGuid_ThrowsDueToUnavailableChannelProvider()
    {
        var service = CreateServiceWithoutConstructor();

        Assert.ThrowsAsync<NullReferenceException>(async () => await service.DeletePageAsync(
            channelId: 1,
            WebPageItemGuid: Guid.Empty,
            culture: "en-gb",
            deleteAllCultures: false,
            cancellationToken: CancellationToken.None));
    }

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(int.MinValue)]
    public async Task DeletePageAsync_WithVariousChannelIds_ThrowsDueToUnavailableChannelProvider(int channelId)
    {
        var service = CreateServiceWithoutConstructor();

        Assert.ThrowsAsync<NullReferenceException>(async () => await service.DeletePageAsync(
            channelId,
            WebPageItemGuid: Guid.NewGuid(),
            culture: "en-gb",
            deleteAllCultures: false,
            cancellationToken: CancellationToken.None));
    }

    [Test]
    public async Task GetPersonPageAsync_WithEmptyGuid_Returns500()
    {
        var service = CreateServiceWithoutConstructor();

        var result = await service.GetPersonPageAsync(
            channelId: 1,
            WebPageItemGuid: Guid.Empty,
            culture: "en-gb",
            CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result.Result!;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
    }

    [Test]
    public async Task GetPersonPagesAsync_WithCancelledToken_StillReturns500()
    {
        var service = CreateServiceWithoutConstructor();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await service.GetPersonPagesAsync(
            channelId: 1,
            culture: "en-gb",
            onlyPublished: true,
            cts.Token);

        Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result.Result!;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
    }

    [Test]
    public void CreatePersonItemData_WithFullContent_ThrowsDueToUnavailableMappingHelper()
    {
        var service = CreateServiceWithoutConstructor();
        var method = GetPrivateMethod("CreatePersonItemData");
        var request = CreateFullPersonPageContent();

        var invokeResult = method.Invoke(service, new object[] { request, CancellationToken.None });
        var task = (Task)invokeResult!;

        Assert.ThrowsAsync<NullReferenceException>(async () => await task);
    }

    [Test]
    public void CreatePersonItemData_WithEmptyContent_ThrowsDueToUnavailableMappingHelper()
    {
        var service = CreateServiceWithoutConstructor();
        var method = GetPrivateMethod("CreatePersonItemData");
        var request = new PersonPageContent
        {
            MetadataTitle = string.Empty,
            PersonFirstName = string.Empty,
            PersonLastName = string.Empty,
            MetadataTeaserImageAltText = string.Empty,
        };

        var invokeResult = method.Invoke(service, new object[] { request, CancellationToken.None });
        var task = (Task)invokeResult!;

        Assert.ThrowsAsync<NullReferenceException>(async () => await task);
    }
}