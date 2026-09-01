using BDO.GWT.GLOBAL.API.DTOs;
using BDO.GWT.GLOBAL.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;

namespace BDO.GWT.GLOBAL.API.Tests.Services;

[TestFixture]
public class ContentCategoryServiceTests
{
    private static ContentCategoryService CreateServiceWithoutConstructor() =>
        (ContentCategoryService)RuntimeHelpers.GetUninitializedObject(typeof(ContentCategoryService));

    [Test]
    public void ContentCategoryService_ImplementsIContentCategoryService()
    {
        var service = CreateServiceWithoutConstructor();

        Assert.That(service, Is.InstanceOf<IContentCategoryService>());
    }

    [Test]
    public void IContentCategoryService_HasExpectedMethods()
    {
        var interfaceType = typeof(IContentCategoryService);

        Assert.Multiple(() =>
        {
            Assert.That(interfaceType.GetMethod(nameof(IContentCategoryService.GetContentCategoriesAsync)), Is.Not.Null);
            Assert.That(interfaceType.GetMethod(nameof(IContentCategoryService.GetContentAsync)), Is.Not.Null);
        });
    }

    [Test]
    public async Task GetContentCategoriesAsync_WhenDependenciesUnavailable_Returns500()
    {
        var service = CreateServiceWithoutConstructor();

        var result = await service.GetContentCategoriesAsync(1, "en-GB", CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result.Result!;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
    }

    [Test]
    public async Task GetContentCategoriesAsync_WithNullCulture_WhenDependenciesUnavailable_Returns500()
    {
        var service = CreateServiceWithoutConstructor();

        var result = await service.GetContentCategoriesAsync(1, null!, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result.Result!;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
    }

    [Test]
    public async Task GetContentAsync_WhenDependenciesUnavailable_Returns500()
    {
        var service = CreateServiceWithoutConstructor();

        var result = await service.GetContentAsync(1, "en-GB", 10, 5, Array.Empty<ContentCategoryFilter>(), CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result.Result!;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
    }

    [Test]
    public async Task GetContentAsync_WithNullFilters_WhenDependenciesUnavailable_Returns500()
    {
        var service = CreateServiceWithoutConstructor();

        var result = await service.GetContentAsync(1, "en-GB", null, null, null!, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result.Result!;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
    }

    [TestCase(1, "en-GB")]
    [TestCase(0, "fr-FR")]
    [TestCase(999, null)]
    public async Task GetContentCategoriesAsync_WithVariousInputs_WhenDependenciesUnavailable_Returns500(int channelId, string? culture)
    {
        var service = CreateServiceWithoutConstructor();

        var result = await service.GetContentCategoriesAsync(channelId, culture!, CancellationToken.None);

        var objectResult = result.Result as ObjectResult;
        Assert.That(objectResult, Is.Not.Null);
        Assert.That(objectResult!.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
    }

    #region DTOs

    [Test]
    public void ContentCategoryDto_StoresValues()
    {
        var dto = new ContentCategoryDto
        {
            Name = "Industry",
            Code = "Industry",
            SubCategories = new List<ContentSubCategoryDto>
            {
                new() { Id = 7, Name = "Healthcare" }
            }
        };

        Assert.Multiple(() =>
        {
            Assert.That(dto.Name, Is.EqualTo("Industry"));
            Assert.That(dto.Code, Is.EqualTo("Industry"));
            Assert.That(dto.SubCategories, Has.Count.EqualTo(1));
            Assert.That(dto.SubCategories[0].Id, Is.EqualTo(7));
            Assert.That(dto.SubCategories[0].Name, Is.EqualTo("Healthcare"));
        });
    }

    [Test]
    public void ContentCategoryFilter_StoresValues()
    {
        var filter = new ContentCategoryFilter
        {
            CategoryCode = "ServiceArea",
            SubCategoryIds = new[] { 1, 2, 3 }
        };

        Assert.Multiple(() =>
        {
            Assert.That(filter.CategoryCode, Is.EqualTo("ServiceArea"));
            Assert.That(filter.SubCategoryIds, Is.EqualTo(new[] { 1, 2, 3 }));
        });
    }

    [Test]
    public void ContentCategoryFilter_DefaultValues_AreNull()
    {
        var filter = new ContentCategoryFilter();

        Assert.Multiple(() =>
        {
            Assert.That(filter.CategoryCode, Is.Null);
            Assert.That(filter.SubCategoryIds, Is.Null);
        });
    }

    [Test]
    public void ContentSubCategoryDto_StoresValues()
    {
        var dto = new ContentSubCategoryDto { Id = 42, Name = "Sub" };

        Assert.Multiple(() =>
        {
            Assert.That(dto.Id, Is.EqualTo(42));
            Assert.That(dto.Name, Is.EqualTo("Sub"));
        });
    }

    [Test]
    public void ContentDto_StoresValues()
    {
        var dto = new ContentDto
        {
            InsightsSearchURL = "https://example.com/search",
            Insights = new List<InsightContentDto>()
        };

        Assert.Multiple(() =>
        {
            Assert.That(dto.InsightsSearchURL, Is.EqualTo("https://example.com/search"));
            Assert.That(dto.Insights, Is.Empty);
        });
    }

    [Test]
    public void IContentCategoryService_Methods_HaveExpectedReturnTypes()
    {
        var interfaceType = typeof(IContentCategoryService);

        Assert.Multiple(() =>
        {
            Assert.That(interfaceType.GetMethod("GetContentCategoriesAsync")!.ReturnType,
                Is.EqualTo(typeof(Task<ActionResult<IEnumerable<ContentCategoryDto>>>)));
            Assert.That(interfaceType.GetMethod("GetContentAsync")!.ReturnType,
                Is.EqualTo(typeof(Task<ActionResult<ContentDto>>)));
        });
    }

    #endregion
}
