using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace BDO.GWT.GLOBAL.API.Tests.Services;

[TestFixture]
public class InsightCrudServiceTests
{
    private static InsightCrudService CreateServiceWithoutConstructor() =>
        (InsightCrudService)RuntimeHelpers.GetUninitializedObject(typeof(InsightCrudService));

    [Test]
    public async Task CreatePageAsync_WithNullRequest_ReturnsBadRequest()
    {
        var service = CreateServiceWithoutConstructor();

        var result = await service.CreatePageAsync(
            channelId: 1,
            nodeParentID: 10,
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
            pageGUID: Guid.NewGuid(),
            fromCulture: "en-gb",
            toCulture: "en-us",
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
            pageGUID: Guid.NewGuid(),
            culture: "en-gb",
            request: null!,
            cancellationToken: CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<BadRequestResult>());
    }

    [Test]
    public void InsightCrudService_ImplementsIInsightCrudService()
    {
        var service = CreateServiceWithoutConstructor();

        Assert.That(service, Is.InstanceOf<IInsightCrudService>());
    }

    [Test]
    public void InsightPageContent_DefaultValues_AreInitialized()
    {
        var content = new InsightPageContent();

        Assert.That(content, Is.Not.Null);
    }

    [Test]
    public void PageOutput_InsightPageContent_CanBeConstructed()
    {
        var guid = Guid.NewGuid();
        var content = new InsightPageContent();
        var output = new PageOutput<InsightPageContent>
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
    public async Task GetInsightPagesAsync_WhenDependenciesUnavailable_ReturnsInternalServerError()
    {
        var service = CreateServiceWithoutConstructor();

        var result = await service.GetInsightPagesAsync(1, "en-gb", true, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
        Assert.That(((ObjectResult)result.Result!).StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
    }

    [Test]
    public async Task GetInsightPagesAsync_WithNullCulture_WhenDependenciesUnavailable_ReturnsInternalServerError()
    {
        var service = CreateServiceWithoutConstructor();

        var result = await service.GetInsightPagesAsync(1, null, null, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
        Assert.That(((ObjectResult)result.Result!).StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
    }

    [Test]
    public async Task GetInsightPageAsync_WhenDependenciesUnavailable_ReturnsInternalServerError()
    {
        var service = CreateServiceWithoutConstructor();

        var result = await service.GetInsightPageAsync(1, Guid.NewGuid(), "en-gb", CancellationToken.None);

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
    public void InsightCrudService_Interface_HasExpectedMethods()
    {
        var interfaceType = typeof(IInsightCrudService);

        Assert.Multiple(() =>
        {
            Assert.That(interfaceType.GetMethod("GetInsightPagesAsync"), Is.Not.Null);
            Assert.That(interfaceType.GetMethod("GetInsightPageAsync"), Is.Not.Null);
            Assert.That(interfaceType.GetMethod("CreatePageAsync"), Is.Not.Null);
            Assert.That(interfaceType.GetMethod("CreatePageCultureVersionAsync"), Is.Not.Null);
            Assert.That(interfaceType.GetMethod("UpdatePageAsync"), Is.Not.Null);
            Assert.That(interfaceType.GetMethod("DeletePageAsync"), Is.Not.Null);
        });
    }
}
