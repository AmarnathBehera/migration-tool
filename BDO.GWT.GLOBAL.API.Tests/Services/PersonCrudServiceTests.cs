using BDO.GWT.Common.Core.Exceptions;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.Core;
using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Helpers;
using BDO.GWT.GLOBAL.API.Services;
using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Websites;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
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

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
            Assert.That(((ObjectResult)result.Result!).StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
        });
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

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
            Assert.That(((ObjectResult)result.Result!).StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
        });
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

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
            Assert.That(((ObjectResult)result.Result!).StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
        });
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

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
            Assert.That(((ObjectResult)result.Result!).StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
        });
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

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
            Assert.That(((ObjectResult)result.Result!).StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
        });
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

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
            Assert.That(((ObjectResult)result.Result!).StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
        });
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

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
            Assert.That(((ObjectResult)result.Result!).StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
        });
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

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
            Assert.That(((ObjectResult)result.Result!).StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
        });
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

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
            Assert.That(((ObjectResult)result.Result!).StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
        });
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

    // -------------------------------------------------------------------------
    // PersonPageContent data model tests
    // -------------------------------------------------------------------------

    [Test]
    public void PersonPageContent_RequiredProperties_CanBeSet()
    {
        var content = new PersonPageContent
        {
            MetadataTitle = "Jane Doe",
            PersonFirstName = "Jane",
            PersonLastName = "Doe",
            MetadataTeaserImageAltText = "Jane headshot"
        };

        Assert.Multiple(() =>
        {
            Assert.That(content.MetadataTitle, Is.EqualTo("Jane Doe"));
            Assert.That(content.PersonFirstName, Is.EqualTo("Jane"));
            Assert.That(content.PersonLastName, Is.EqualTo("Doe"));
            Assert.That(content.MetadataTeaserImageAltText, Is.EqualTo("Jane headshot"));
        });
    }

    [Test]
    public void PersonPageContent_NullableStringProperties_AreNullByDefault()
    {
        var content = new PersonPageContent
        {
            MetadataTitle = "Test",
            PersonFirstName = "Test",
            PersonLastName = "User",
            MetadataTeaserImageAltText = "alt"
        };

        Assert.Multiple(() =>
        {
            Assert.That(content.PersonSurnamePrefixes, Is.Null);
            Assert.That(content.MetadataTeaserImage, Is.Null);
            Assert.That(content.MetadataCanonical, Is.Null);
            Assert.That(content.MetadataDescription, Is.Null);
            Assert.That(content.PersonJobTitle, Is.Null);
            Assert.That(content.PersonPhone, Is.Null);
            Assert.That(content.PersonMobilePhone, Is.Null);
            Assert.That(content.PersonDirectDialNumber, Is.Null);
            Assert.That(content.PersonEmail, Is.Null);
            Assert.That(content.Facebook, Is.Null);
            Assert.That(content.Twitter, Is.Null);
            Assert.That(content.LinkedIn, Is.Null);
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
    public void PersonPageContent_BooleanProperties_DefaultToFalse()
    {
        var content = new PersonPageContent
        {
            MetadataTitle = "Test",
            PersonFirstName = "Test",
            PersonLastName = "User",
            MetadataTeaserImageAltText = "alt"
        };

        Assert.Multiple(() =>
        {
            Assert.That(content.EnableForm, Is.False);
            Assert.That(content.EnableVCard, Is.False);
            Assert.That(content.MetadataOGTwitterImageSizeCheckbox, Is.False);
            Assert.That(content.MetadataNofollow, Is.False);
            Assert.That(content.MetadataNoindex, Is.False);
            Assert.That(content.IncludeInSitemap, Is.False);
        });
    }

    [Test]
    public void PersonPageContent_ListProperties_AreEmptyByDefault()
    {
        var content = new PersonPageContent
        {
            MetadataTitle = "Test",
            PersonFirstName = "Test",
            PersonLastName = "User",
            MetadataTeaserImageAltText = "alt"
        };

        Assert.Multiple(() =>
        {
            Assert.That(content.PersonCredentials, Is.Empty);
            Assert.That(content.MetadataCity, Is.Empty);
            Assert.That(content.MetadataOfficeLocation, Is.Empty);
            Assert.That(content.MetadataProfileGroups, Is.Empty);
            Assert.That(content.MetadataBusinessLines, Is.Empty);
            Assert.That(content.MetadataServiceAreas, Is.Empty);
            Assert.That(content.MetadataServices, Is.Empty);
            Assert.That(content.MetadataSubServices, Is.Empty);
            Assert.That(content.MetadataIndustryCategories, Is.Empty);
            Assert.That(content.MetadataIndustries, Is.Empty);
            Assert.That(content.MetadataIndustryServices, Is.Empty);
            Assert.That(content.MetadataSpecialtiesCategories, Is.Empty);
            Assert.That(content.MetadataSpecialtiesAreas, Is.Empty);
            Assert.That(content.MetadataSpecialtiesPages, Is.Empty);
            Assert.That(content.MicrositeMasterPages, Is.Empty);
            Assert.That(content.MicrositePages, Is.Empty);
        });
    }

    [Test]
    public void PersonPageContent_AllNullableStringProperties_CanBeSet()
    {
        var publishFrom = new DateTime(2025, 1, 1);
        var publishTo = new DateTime(2026, 1, 1);

        var content = new PersonPageContent
        {
            MetadataTitle = "Jane Doe",
            PersonFirstName = "Jane",
            PersonSurnamePrefixes = "van der",
            PersonLastName = "Doe",
            MetadataTeaserImage = "teaser.jpg",
            MetadataTeaserImageAltText = "alt",
            MetadataCanonical = "https://example.com/jane",
            MetadataDescription = "Senior Partner",
            PersonJobTitle = "Partner",
            PersonPhone = "+1 555 0100",
            PersonMobilePhone = "+1 555 0101",
            PersonDirectDialNumber = "+1 555 0102",
            PersonEmail = "jane@example.com",
            Facebook = "https://facebook.com/jane",
            Twitter = "https://twitter.com/jane",
            LinkedIn = "https://linkedin.com/in/jane",
            MetadataOGTitle = "Jane Doe | BDO",
            MetadataOGDescription = "Meet Jane",
            MetadataOGImage = "og.jpg",
            MetadataOGTwitterImage = "og-twitter.jpg",
            PublishFrom = publishFrom,
            PublishTo = publishTo,
            RedirectOnUnpublish = "/people"
        };

        Assert.Multiple(() =>
        {
            Assert.That(content.PersonSurnamePrefixes, Is.EqualTo("van der"));
            Assert.That(content.MetadataTeaserImage, Is.EqualTo("teaser.jpg"));
            Assert.That(content.MetadataCanonical, Is.EqualTo("https://example.com/jane"));
            Assert.That(content.MetadataDescription, Is.EqualTo("Senior Partner"));
            Assert.That(content.PersonJobTitle, Is.EqualTo("Partner"));
            Assert.That(content.PersonPhone, Is.EqualTo("+1 555 0100"));
            Assert.That(content.PersonMobilePhone, Is.EqualTo("+1 555 0101"));
            Assert.That(content.PersonDirectDialNumber, Is.EqualTo("+1 555 0102"));
            Assert.That(content.PersonEmail, Is.EqualTo("jane@example.com"));
            Assert.That(content.Facebook, Is.EqualTo("https://facebook.com/jane"));
            Assert.That(content.Twitter, Is.EqualTo("https://twitter.com/jane"));
            Assert.That(content.LinkedIn, Is.EqualTo("https://linkedin.com/in/jane"));
            Assert.That(content.MetadataOGTitle, Is.EqualTo("Jane Doe | BDO"));
            Assert.That(content.MetadataOGDescription, Is.EqualTo("Meet Jane"));
            Assert.That(content.MetadataOGImage, Is.EqualTo("og.jpg"));
            Assert.That(content.MetadataOGTwitterImage, Is.EqualTo("og-twitter.jpg"));
            Assert.That(content.PublishFrom, Is.EqualTo(publishFrom));
            Assert.That(content.PublishTo, Is.EqualTo(publishTo));
            Assert.That(content.RedirectOnUnpublish, Is.EqualTo("/people"));
        });
    }

    [Test]
    public void PersonPageContent_BooleanProperties_CanBeSetToTrue()
    {
        var content = new PersonPageContent
        {
            MetadataTitle = "Test",
            PersonFirstName = "Test",
            PersonLastName = "User",
            MetadataTeaserImageAltText = "alt",
            EnableForm = true,
            EnableVCard = true,
            MetadataOGTwitterImageSizeCheckbox = true,
            MetadataNofollow = true,
            MetadataNoindex = true,
            IncludeInSitemap = true
        };

        Assert.Multiple(() =>
        {
            Assert.That(content.EnableForm, Is.True);
            Assert.That(content.EnableVCard, Is.True);
            Assert.That(content.MetadataOGTwitterImageSizeCheckbox, Is.True);
            Assert.That(content.MetadataNofollow, Is.True);
            Assert.That(content.MetadataNoindex, Is.True);
            Assert.That(content.IncludeInSitemap, Is.True);
        });
    }

    [Test]
    public void PersonPageContent_AllListProperties_CanBePopulated()
    {
        var content = new PersonPageContent
        {
            MetadataTitle = "Test",
            PersonFirstName = "Test",
            PersonLastName = "User",
            MetadataTeaserImageAltText = "alt",
            PersonCredentials = ["CPA", "MBA"],
            MetadataCity = ["London"],
            MetadataOfficeLocation = ["London Office"],
            MetadataProfileGroups = ["Audit"],
            MetadataBusinessLines = ["Assurance"],
            MetadataServiceAreas = ["Financial Services"],
            MetadataServices = ["Audit Services"],
            MetadataSubServices = ["External Audit"],
            MetadataIndustryCategories = ["Financial"],
            MetadataIndustries = ["Banking"],
            MetadataIndustryServices = ["Banking Audit"],
            MetadataSpecialtiesCategories = ["Risk"],
            MetadataSpecialtiesAreas = ["Regulatory Risk"],
            MetadataSpecialtiesPages = ["Risk Advisory"],
            MicrositeMasterPages = ["Global People"],
            MicrositePages = ["Leadership"]
        };

        Assert.Multiple(() =>
        {
            Assert.That(content.PersonCredentials, Has.Count.EqualTo(2));
            Assert.That(content.MetadataCity, Has.Count.EqualTo(1));
            Assert.That(content.MetadataOfficeLocation, Has.Count.EqualTo(1));
            Assert.That(content.MetadataProfileGroups, Has.Count.EqualTo(1));
            Assert.That(content.MetadataBusinessLines, Has.Count.EqualTo(1));
            Assert.That(content.MetadataServiceAreas, Has.Count.EqualTo(1));
            Assert.That(content.MetadataServices, Has.Count.EqualTo(1));
            Assert.That(content.MetadataSubServices, Has.Count.EqualTo(1));
            Assert.That(content.MetadataIndustryCategories, Has.Count.EqualTo(1));
            Assert.That(content.MetadataIndustries, Has.Count.EqualTo(1));
            Assert.That(content.MetadataIndustryServices, Has.Count.EqualTo(1));
            Assert.That(content.MetadataSpecialtiesCategories, Has.Count.EqualTo(1));
            Assert.That(content.MetadataSpecialtiesAreas, Has.Count.EqualTo(1));
            Assert.That(content.MetadataSpecialtiesPages, Has.Count.EqualTo(1));
            Assert.That(content.MicrositeMasterPages, Has.Count.EqualTo(1));
            Assert.That(content.MicrositePages, Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void PersonPageContent_ListProperties_ContainCorrectValues()
    {
        var content = new PersonPageContent
        {
            MetadataTitle = "Test",
            PersonFirstName = "Test",
            PersonLastName = "User",
            MetadataTeaserImageAltText = "alt",
            PersonCredentials = ["CPA", "MBA"],
            MetadataCity = ["London", "Paris"],
            MetadataOfficeLocation = ["London Office", "Paris Office"]
        };

        Assert.Multiple(() =>
        {
            Assert.That(content.PersonCredentials, Does.Contain("CPA"));
            Assert.That(content.PersonCredentials, Does.Contain("MBA"));
            Assert.That(content.MetadataCity, Does.Contain("London"));
            Assert.That(content.MetadataCity, Does.Contain("Paris"));
            Assert.That(content.MetadataOfficeLocation, Does.Contain("London Office"));
        });
    }

    [Test]
    public void PersonPageContent_FullModel_MatchesCreateFullPersonPageContent()
    {
        var content = CreateFullPersonPageContent();

        Assert.Multiple(() =>
        {
            Assert.That(content.MetadataTitle, Is.EqualTo("Jane Doe"));
            Assert.That(content.PersonFirstName, Is.EqualTo("Jane"));
            Assert.That(content.PersonSurnamePrefixes, Is.EqualTo("van der"));
            Assert.That(content.PersonLastName, Is.EqualTo("Doe"));
            Assert.That(content.MetadataTeaserImage, Is.EqualTo("teaser.jpg"));
            Assert.That(content.MetadataTeaserImageAltText, Is.EqualTo("Jane Doe headshot"));
            Assert.That(content.MetadataCanonical, Is.EqualTo("https://example.com/people/jane-doe"));
            Assert.That(content.MetadataDescription, Is.EqualTo("Partner in the audit practice."));
            Assert.That(content.PersonJobTitle, Is.EqualTo("Partner"));
            Assert.That(content.PersonPhone, Is.EqualTo("+1 555 0100"));
            Assert.That(content.PersonMobilePhone, Is.EqualTo("+1 555 0101"));
            Assert.That(content.PersonDirectDialNumber, Is.EqualTo("+1 555 0102"));
            Assert.That(content.PersonEmail, Is.EqualTo("jane.doe@example.com"));
            Assert.That(content.EnableForm, Is.True);
            Assert.That(content.EnableVCard, Is.True);
            Assert.That(content.Facebook, Is.EqualTo("https://facebook.com/janedoe"));
            Assert.That(content.Twitter, Is.EqualTo("https://twitter.com/janedoe"));
            Assert.That(content.LinkedIn, Is.EqualTo("https://linkedin.com/in/janedoe"));
            Assert.That(content.MetadataOGTitle, Is.EqualTo("Jane Doe | BDO"));
            Assert.That(content.MetadataOGDescription, Is.EqualTo("Meet Jane Doe, Partner at BDO."));
            Assert.That(content.MetadataOGImage, Is.EqualTo("og.jpg"));
            Assert.That(content.MetadataOGTwitterImageSizeCheckbox, Is.True);
            Assert.That(content.MetadataOGTwitterImage, Is.EqualTo("og-twitter.jpg"));
            Assert.That(content.MetadataNofollow, Is.False);
            Assert.That(content.MetadataNoindex, Is.False);
            Assert.That(content.IncludeInSitemap, Is.True);
            Assert.That(content.PublishFrom, Is.EqualTo(new DateTime(2026, 1, 1)));
            Assert.That(content.PublishTo, Is.EqualTo(new DateTime(2027, 1, 1)));
            Assert.That(content.RedirectOnUnpublish, Is.EqualTo("/people"));
        });
    }

    [Test]
    public void PersonCrudService_Interface_HasExpectedMethods()
    {
        var interfaceType = typeof(IPersonCrudService);

        Assert.Multiple(() =>
        {
            Assert.That(interfaceType.GetMethod("GetPersonPagesAsync"), Is.Not.Null);
            Assert.That(interfaceType.GetMethod("GetPersonPageAsync"), Is.Not.Null);
            Assert.That(interfaceType.GetMethod("CreatePageAsync"), Is.Not.Null);
            Assert.That(interfaceType.GetMethod("CreatePageCultureVersionAsync"), Is.Not.Null);
            Assert.That(interfaceType.GetMethod("UpdatePageAsync"), Is.Not.Null);
            Assert.That(interfaceType.GetMethod("DeletePageAsync"), Is.Not.Null);
        });
    }

    [Test]
    public async Task GetPersonPagesAsync_WithNullCulture_Returns500()
    {
        var service = CreateServiceWithoutConstructor();

        var result = await service.GetPersonPagesAsync(
            channelId: 1,
            culture: null,
            onlyPublished: true,
            cancellationToken: CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result.Result!;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
    }

    [Test]
    public async Task GetPersonPageAsync_WithNullCulture_Returns500()
    {
        var service = CreateServiceWithoutConstructor();

        var result = await service.GetPersonPageAsync(
            channelId: 1,
            WebPageItemGuid: Guid.NewGuid(),
            culture: null,
            cancellationToken: CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result.Result!;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
    }

    [Test]
    public async Task CreatePageAsync_WithNullCulture_AndNullRequest_ReturnsBadRequest()
    {
        var service = CreateServiceWithoutConstructor();

        var result = await service.CreatePageAsync(
            channelId: 1,
            WebPageParentID: 10,
            culture: null,
            request: null!,
            cancellationToken: CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<BadRequestResult>());
    }

    [Test]
    public async Task UpdatePageAsync_WithNullCulture_AndNullRequest_ReturnsBadRequest()
    {
        var service = CreateServiceWithoutConstructor();

        var result = await service.UpdatePageAsync(
            channelId: 1,
            WebPageItemGuid: Guid.NewGuid(),
            culture: null,
            request: null!,
            cancellationToken: CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<BadRequestResult>());
    }

    [Test]
    public async Task CreatePageCultureVersionAsync_WithNullCultures_AndNullRequest_ReturnsBadRequest()
    {
        var service = CreateServiceWithoutConstructor();

        var result = await service.CreatePageCultureVersionAsync(
            channelId: 1,
            WebPageItemGuid: Guid.NewGuid(),
            fromCulture: null,
            toCulture: null,
            request: null!,
            cancellationToken: CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<BadRequestResult>());
    }

    [Test]
    public async Task CreatePageAsync_WithValidRequest_WhenMappingHelperNull_Returns500()
    {
        var service = CreateServiceWithoutConstructor();
        var request = new PersonPageContent
        {
            MetadataTitle = "Test",
            PersonFirstName = "Test",
            PersonLastName = "User",
            MetadataTeaserImageAltText = "alt"
        };

        var result = await service.CreatePageAsync(
            channelId: 1,
            WebPageParentID: 10,
            culture: "en-gb",
            request: request,
            cancellationToken: CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
            Assert.That(((ObjectResult)result.Result!).StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
        });
    }

    [Test]
    public async Task UpdatePageAsync_WithValidRequest_WhenMappingHelperNull_Returns500()
    {
        var service = CreateServiceWithoutConstructor();
        var request = new PersonPageContent
        {
            MetadataTitle = "Test",
            PersonFirstName = "Test",
            PersonLastName = "User",
            MetadataTeaserImageAltText = "alt"
        };

        var result = await service.UpdatePageAsync(
            channelId: 1,
            WebPageItemGuid: Guid.NewGuid(),
            culture: "en-gb",
            request: request,
            cancellationToken: CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
            Assert.That(((ObjectResult)result.Result!).StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
        });
    }

    [Test]
    public async Task CreatePageCultureVersionAsync_WithValidRequest_WhenMappingHelperNull_Returns500()
    {
        var service = CreateServiceWithoutConstructor();
        var request = new PersonPageContent
        {
            MetadataTitle = "Test",
            PersonFirstName = "Test",
            PersonLastName = "User",
            MetadataTeaserImageAltText = "alt"
        };

        var result = await service.CreatePageCultureVersionAsync(
            channelId: 1,
            WebPageItemGuid: Guid.NewGuid(),
            fromCulture: "en-gb",
            toCulture: "fr-fr",
            request: request,
            cancellationToken: CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
            Assert.That(((ObjectResult)result.Result!).StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
        });
    }
}



[TestFixture]
public class PersonCrudServiceCoverageTests
{
    private Mock<IContentService> _contentRetrieverMock = null!;
    private Mock<IValidationState> _validationStateMock = null!;
    private Mock<ICultureService> _cultureServiceMock = null!;
    private Mock<IPageContentValidator> _pageContentValidatorMock = null!;
    private Mock<IInfoProvider<ChannelInfo>> _channelInfoProviderMock = null!;
    private Mock<IInfoByIdProvider<ChannelInfo>> _channelByIdProviderMock = null!;
    private Mock<IWebPageManager> _webPageManagerMock = null!;
    private Mock<IMappingHelper> _mappingHelperMock = null!;

    private static LanguageTag ValidCulture => LanguageTag.GB;
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
        _channelByIdProviderMock = _channelInfoProviderMock.As<IInfoByIdProvider<ChannelInfo>>();
        _webPageManagerMock = new Mock<IWebPageManager>();
        _mappingHelperMock = new Mock<IMappingHelper>();
    }

    #region Helpers

    private PersonCrudService CreateService()
    {
        var service = (PersonCrudService)RuntimeHelpers.GetUninitializedObject(typeof(PersonCrudService));

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
        if (field is null)
        {
            Assert.Fail($"Field '{fieldName}' was not found on {target.GetType().Name}.");
        }

        field!.SetValue(target, value);
    }

    private void SetupCulture(LanguageTag? culture) =>
        _mappingHelperMock
            .Setup(x => x.ValidateAndNormalizeSiteCultureAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(culture);

    private void SetupChannel(ChannelInfo? channel) =>
        _channelByIdProviderMock
            .Setup(x => x.GetAsync(It.IsAny<int>(), It.IsAny<CancellationToken?>()))
            .ReturnsAsync(channel!);

    private static PersonPageContent CreateRequest() => new()
    {
        MetadataTitle = "Jane Doe",
        PersonFirstName = "Jane",
        PersonLastName = "Doe",
        MetadataTeaserImageAltText = "alt",
    };

    private static void AssertNotFoundWithMessage<T>(ActionResult<T> result, string expectedMessage)
    {
        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
        Assert.That(((NotFoundObjectResult)result.Result!).Value, Is.EqualTo(expectedMessage));
    }

    #endregion

    #region GetPersonPagesAsync

    [Test]
    public async Task GetPersonPagesAsync_InvalidCulture_ReturnsNotFoundWithMessage()
    {
        SetupCulture(EmptyLocaleCulture);

        var result = await CreateService().GetPersonPagesAsync(1, "en-gb", true, CancellationToken.None);

        AssertNotFoundWithMessage(result, "Invalid culture specified");
    }

    [Test]
    public async Task GetPersonPagesAsync_ChannelNotFound_ReturnsNotFoundWithMessage()
    {
        SetupCulture(ValidCulture);
        SetupChannel(null);

        var result = await CreateService().GetPersonPagesAsync(1, "en-gb", true, CancellationToken.None);

        AssertNotFoundWithMessage(result, "Channel not found");
    }

    [Test]
    public async Task GetPersonPagesAsync_NullCultureFromHelper_FallsBackToDefault_AndChecksChannel()
    {
        SetupCulture(null);
        SetupChannel(null);

        var result = await CreateService().GetPersonPagesAsync(5, null, null, CancellationToken.None);

        AssertNotFoundWithMessage(result, "Channel not found");
        _channelByIdProviderMock.Verify(x => x.GetAsync(5, It.IsAny<CancellationToken?>()), Times.Once);
    }

    [Test]
    public async Task GetPersonPagesAsync_NullCulture_PassesDefaultLanguageToHelper()
    {
        SetupCulture(ValidCulture);
        SetupChannel(null);

        await CreateService().GetPersonPagesAsync(3, null, true, CancellationToken.None);

        _mappingHelperMock.Verify(x => x.ValidateAndNormalizeSiteCultureAsync(3, LanguageTag.GB.Language, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetPersonPagesAsync_UnprocessableEntityFromHelper_Returns422()
    {
        _mappingHelperMock
            .Setup(x => x.ValidateAndNormalizeSiteCultureAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnprocessableEntityException());

        var result = await CreateService().GetPersonPagesAsync(1, "en-gb", true, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<UnprocessableEntityObjectResult>());
    }

    [Test]
    public async Task GetPersonPagesAsync_GenericException_Returns500WithMessage()
    {
        _mappingHelperMock
            .Setup(x => x.ValidateAndNormalizeSiteCultureAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("boom"));

        var result = await CreateService().GetPersonPagesAsync(1, "en-gb", true, CancellationToken.None);

        var obj = result.Result as ObjectResult;
        Assert.That(obj, Is.Not.Null);
        Assert.That(obj!.StatusCode, Is.EqualTo(500));
        Assert.That(obj.Value, Is.EqualTo("boom"));
    }

    [Test]
    public async Task GetPersonPagesAsync_ChannelNotFound_DoesNotQueryPages()
    {
        SetupCulture(ValidCulture);
        SetupChannel(null);

        await CreateService().GetPersonPagesAsync(1, "en-gb", false, CancellationToken.None);

        _contentRetrieverMock.VerifyNoOtherCalls();
    }

    #endregion

    #region GetPersonPageAsync

    [Test]
    public async Task GetPersonPageAsync_InvalidCulture_ReturnsNotFoundWithMessage()
    {
        SetupCulture(EmptyLocaleCulture);

        var result = await CreateService().GetPersonPageAsync(1, Guid.NewGuid(), "en-gb", CancellationToken.None);

        AssertNotFoundWithMessage(result, "Invalid culture specified");
    }

    [Test]
    public async Task GetPersonPageAsync_ChannelNotFound_ReturnsNotFoundWithMessage()
    {
        SetupCulture(ValidCulture);
        SetupChannel(null);

        var result = await CreateService().GetPersonPageAsync(1, Guid.NewGuid(), "en-gb", CancellationToken.None);

        AssertNotFoundWithMessage(result, "Channel not found");
    }

    [Test]
    public async Task GetPersonPageAsync_UnprocessableEntity_Returns422()
    {
        _mappingHelperMock
            .Setup(x => x.ValidateAndNormalizeSiteCultureAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnprocessableEntityException());

        var result = await CreateService().GetPersonPageAsync(1, Guid.NewGuid(), "en-gb", CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<UnprocessableEntityObjectResult>());
    }

    [Test]
    public async Task GetPersonPageAsync_ChannelProviderThrows_Returns500()
    {
        SetupCulture(ValidCulture);
        _channelByIdProviderMock
            .Setup(x => x.GetAsync(It.IsAny<int>(), It.IsAny<CancellationToken?>()))
            .ThrowsAsync(new InvalidOperationException("db down"));

        var result = await CreateService().GetPersonPageAsync(1, Guid.NewGuid(), "en-gb", CancellationToken.None);

        Assert.That(((ObjectResult)result.Result!).StatusCode, Is.EqualTo(500));
        Assert.That(((ObjectResult)result.Result!).Value, Is.EqualTo("db down"));
    }

    #endregion

    #region CreatePageAsync

    [Test]
    public async Task CreatePageAsync_InvalidCulture_ReturnsNotFoundWithMessage()
    {
        SetupCulture(EmptyLocaleCulture);

        var result = await CreateService().CreatePageAsync(1, 10, "en-gb", CreateRequest(), CancellationToken.None);

        AssertNotFoundWithMessage(result, "Invalid culture specified");
    }

    [Test]
    public async Task CreatePageAsync_ChannelNotFound_ReturnsNotFoundWithMessage()
    {
        SetupCulture(ValidCulture);
        SetupChannel(null);

        var result = await CreateService().CreatePageAsync(1, 10, "en-gb", CreateRequest(), CancellationToken.None);

        AssertNotFoundWithMessage(result, "Channel not found");
    }

    [Test]
    public async Task CreatePageAsync_ChannelNotFound_DoesNotValidateOrCreate()
    {
        SetupCulture(ValidCulture);
        SetupChannel(null);

        await CreateService().CreatePageAsync(1, 10, "en-gb", CreateRequest(), CancellationToken.None);

        _pageContentValidatorMock.Verify(x => x.ValidateAndNormalizePageContent(It.IsAny<PersonPageContent>()), Times.Never);
        _webPageManagerMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task CreatePageAsync_UnprocessableEntity_Returns422()
    {
        _mappingHelperMock
            .Setup(x => x.ValidateAndNormalizeSiteCultureAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnprocessableEntityException());

        var result = await CreateService().CreatePageAsync(1, 10, "en-gb", CreateRequest(), CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<UnprocessableEntityObjectResult>());
    }

    [Test]
    public async Task CreatePageAsync_NullRequest_DoesNotCallMappingHelper()
    {
        var result = await CreateService().CreatePageAsync(1, 10, "en-gb", null!, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<BadRequestResult>());
        _mappingHelperMock.VerifyNoOtherCalls();
    }

    #endregion

    #region CreatePageCultureVersionAsync

    [Test]
    public async Task CreatePageCultureVersionAsync_InvalidFromCulture_ReturnsNotFoundWithMessage()
    {
        SetupCulture(EmptyLanguageCulture);

        var result = await CreateService().CreatePageCultureVersionAsync(1, Guid.NewGuid(), "en-gb", "fr-fr", CreateRequest(), CancellationToken.None);

        AssertNotFoundWithMessage(result, "Invalid from culture specified");
    }

    [Test]
    public async Task CreatePageCultureVersionAsync_InvalidToCulture_ReturnsNotFoundWithMessage()
    {
        _mappingHelperMock
            .SetupSequence(x => x.ValidateAndNormalizeSiteCultureAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ValidCulture)
            .ReturnsAsync(EmptyLanguageCulture);

        var result = await CreateService().CreatePageCultureVersionAsync(1, Guid.NewGuid(), "en-gb", "fr-fr", CreateRequest(), CancellationToken.None);

        AssertNotFoundWithMessage(result, "Invalid to culture specified");
    }

    [Test]
    public async Task CreatePageCultureVersionAsync_ChannelNotFound_ReturnsNotFoundWithMessage()
    {
        SetupCulture(ValidCulture);
        SetupChannel(null);

        var result = await CreateService().CreatePageCultureVersionAsync(1, Guid.NewGuid(), "en-gb", "fr-fr", CreateRequest(), CancellationToken.None);

        AssertNotFoundWithMessage(result, "Channel not found");
    }

    [Test]
    public async Task CreatePageCultureVersionAsync_ValidatesBothCultures()
    {
        SetupCulture(ValidCulture);
        SetupChannel(null);

        await CreateService().CreatePageCultureVersionAsync(1, Guid.NewGuid(), "en-gb", "fr-fr", CreateRequest(), CancellationToken.None);

        _mappingHelperMock.Verify(x => x.ValidateAndNormalizeSiteCultureAsync(1, "en-gb", It.IsAny<CancellationToken>()), Times.Once);
        _mappingHelperMock.Verify(x => x.ValidateAndNormalizeSiteCultureAsync(1, "fr-fr", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task CreatePageCultureVersionAsync_NullCultures_UseDefaultLanguage()
    {
        SetupCulture(ValidCulture);
        SetupChannel(null);

        await CreateService().CreatePageCultureVersionAsync(1, Guid.NewGuid(), null, null, CreateRequest(), CancellationToken.None);

        _mappingHelperMock.Verify(x => x.ValidateAndNormalizeSiteCultureAsync(1, LanguageTag.GB.Language, It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Test]
    public async Task CreatePageCultureVersionAsync_UnprocessableEntity_Returns422()
    {
        _mappingHelperMock
            .Setup(x => x.ValidateAndNormalizeSiteCultureAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnprocessableEntityException());

        var result = await CreateService().CreatePageCultureVersionAsync(1, Guid.NewGuid(), "en-gb", "fr-fr", CreateRequest(), CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<UnprocessableEntityObjectResult>());
    }

    #endregion

    #region UpdatePageAsync

    [Test]
    public async Task UpdatePageAsync_InvalidCulture_ReturnsNotFoundWithMessage()
    {
        SetupCulture(EmptyLocaleCulture);

        var result = await CreateService().UpdatePageAsync(1, Guid.NewGuid(), "en-gb", CreateRequest(), CancellationToken.None);

        AssertNotFoundWithMessage(result, "Invalid culture specified");
    }

    [Test]
    public async Task UpdatePageAsync_ChannelNotFound_ReturnsNotFoundWithMessage()
    {
        SetupCulture(ValidCulture);
        SetupChannel(null);

        var result = await CreateService().UpdatePageAsync(1, Guid.NewGuid(), "en-gb", CreateRequest(), CancellationToken.None);

        AssertNotFoundWithMessage(result, "Channel not found");
    }

    [Test]
    public async Task UpdatePageAsync_NullCulture_PassesDefaultLocaleToHelper()
    {
        SetupCulture(ValidCulture);
        SetupChannel(null);

        await CreateService().UpdatePageAsync(2, Guid.NewGuid(), null, CreateRequest(), CancellationToken.None);

        _mappingHelperMock.Verify(x => x.ValidateAndNormalizeSiteCultureAsync(2, LanguageTag.GB.Locale, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task UpdatePageAsync_UnprocessableEntity_Returns422()
    {
        _mappingHelperMock
            .Setup(x => x.ValidateAndNormalizeSiteCultureAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnprocessableEntityException());

        var result = await CreateService().UpdatePageAsync(1, Guid.NewGuid(), "en-gb", CreateRequest(), CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<UnprocessableEntityObjectResult>());
    }

    [Test]
    public async Task UpdatePageAsync_GenericException_Returns500WithMessage()
    {
        _mappingHelperMock
            .Setup(x => x.ValidateAndNormalizeSiteCultureAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("update failed"));

        var result = await CreateService().UpdatePageAsync(1, Guid.NewGuid(), "en-gb", CreateRequest(), CancellationToken.None);

        Assert.That(((ObjectResult)result.Result!).StatusCode, Is.EqualTo(500));
        Assert.That(((ObjectResult)result.Result!).Value, Is.EqualTo("update failed"));
    }

    #endregion

    #region DeletePageAsync

    [Test]
    public void DeletePageAsync_InvalidCulture_ThrowsNotFoundException()
    {
        SetupCulture(EmptyLocaleCulture);

        var ex = Assert.ThrowsAsync<NotFoundException>(() =>
            CreateService().DeletePageAsync(1, Guid.NewGuid(), "en-gb", false, CancellationToken.None));

        Assert.That(ex!.Message, Does.Contain("Invalid culture specified"));
    }

    [Test]
    public void DeletePageAsync_ChannelNotFound_ThrowsNotFoundException()
    {
        SetupCulture(ValidCulture);
        SetupChannel(null);

        var ex = Assert.ThrowsAsync<NotFoundException>(() =>
            CreateService().DeletePageAsync(1, Guid.NewGuid(), "en-gb", true, CancellationToken.None));

        Assert.That(ex!.Message, Does.Contain("Channel not found"));
    }

    [Test]
    public void DeletePageAsync_ChannelNotFound_DoesNotQueryCulturesOrDelete()
    {
        SetupCulture(ValidCulture);
        SetupChannel(null);

        Assert.ThrowsAsync<NotFoundException>(() =>
            CreateService().DeletePageAsync(1, Guid.NewGuid(), "en-gb", true, CancellationToken.None));

        _cultureServiceMock.VerifyNoOtherCalls();
        _webPageManagerMock.VerifyNoOtherCalls();
    }

    [Test]
    public void DeletePageAsync_NullCulture_UsesDefaultLocale()
    {
        SetupCulture(ValidCulture);
        SetupChannel(null);

        Assert.ThrowsAsync<NotFoundException>(() =>
            CreateService().DeletePageAsync(4, Guid.NewGuid(), null, false, CancellationToken.None));

        _mappingHelperMock.Verify(x => x.ValidateAndNormalizeSiteCultureAsync(4, LanguageTag.GB.Locale, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion
}
