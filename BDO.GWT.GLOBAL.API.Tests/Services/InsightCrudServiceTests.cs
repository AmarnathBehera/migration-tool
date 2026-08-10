using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Services;
using Microsoft.AspNetCore.Mvc;
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
}
