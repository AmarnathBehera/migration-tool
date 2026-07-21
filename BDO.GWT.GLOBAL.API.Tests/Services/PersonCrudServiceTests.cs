using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;

namespace BDO.GWT.GLOBAL.API.Tests.Services;

public class PersonCrudServiceTests
{
    // These tests exercise null-request guard clauses that return before any dependency is used.
    private static PersonCrudService CreateServiceWithoutConstructor()
    {
        return (PersonCrudService)RuntimeHelpers.GetUninitializedObject(typeof(PersonCrudService));
    }

    [Test]
    public async Task CreatePageAsync_WithNullRequest_ReturnsBadRequestResult()
    {
        var service = CreateServiceWithoutConstructor();

        ActionResult<PageOutput<PersonPageContent>> result = await service.CreatePageAsync(
            channelId: 1,
            nodeParentID: 10,
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
            pageGUID: Guid.NewGuid(),
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
            pageGUID: Guid.NewGuid(),
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
            pageGUID: Guid.NewGuid(),
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
        var request = new PersonPageContent
        {
            MetadataTitle = "Title",
            PersonFirstName = "First",
            PersonLastName = "Last",
        };

        ActionResult<PageOutput<PersonPageContent>> result = await service.CreatePageAsync(
            channelId: 1,
            nodeParentID: 10,
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
        var request = new PersonPageContent
        {
            MetadataTitle = "Title",
            PersonFirstName = "First",
            PersonLastName = "Last",
        };

        ActionResult<PageOutput<PersonPageContent>> result = await service.CreatePageCultureVersionAsync(
            channelId: 1,
            pageGUID: Guid.NewGuid(),
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
        var request = new PersonPageContent
        {
            MetadataTitle = "Title",
            PersonFirstName = "First",
            PersonLastName = "Last",
        };

        ActionResult<PageOutput<PersonPageContent>> result = await service.UpdatePageAsync(
            channelId: 1,
            pageGUID: Guid.NewGuid(),
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
            pageGUID: Guid.NewGuid(),
            culture: "en-gb",
            deleteAllCultures: false,
            cancellationToken: CancellationToken.None));
    }

    [Test]
    public async Task CreatePageAsync_WithInvalidCultureFormat_Returns500()
    {
        // Invalid culture strings still cause a NullReferenceException on the uninitialized
        // validation state / language service — validating the generic catch-all branch.
        var service = CreateServiceWithoutConstructor();
        var request = new PersonPageContent
        {
            MetadataTitle = "Title",
            PersonFirstName = "First",
            PersonLastName = "Last",
        };

        ActionResult<PageOutput<PersonPageContent>> result = await service.CreatePageAsync(
            channelId: 1,
            nodeParentID: 10,
            culture: "not-a-culture!!",
            request: request,
            cancellationToken: CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result.Result!;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
    }
}
