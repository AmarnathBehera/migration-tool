using BDO.GWT.Common.Core.Exceptions;
using BDO.GWT.Core;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.GLOBAL.API.DTOs;
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
public class ContentCategoryServiceTests
{
    private static readonly int[] ExpectedBusinessIssueIds = [8, 9];

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

    #region Service branch coverage

    [Test]
    public async Task GetContentCategoriesAsync_WhenCultureValidationThrowsUnprocessableEntity_Returns422()
    {
        var harness = new ContentCategoryHarness();
        var errors = new Dictionary<string, string> { ["culture"] = "invalid" };

        harness.MappingHelper
            .Setup(x => x.ValidateAndNormalizeSiteCultureAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnprocessableEntityException());
        harness.ValidationState.Setup(x => x.GetErrors()).Returns(errors);

        var result = await harness.Build().GetContentCategoriesAsync(5, "en-GB", CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<UnprocessableEntityObjectResult>());
            Assert.That(((UnprocessableEntityObjectResult)result.Result!).Value, Is.SameAs(errors));
        });
    }

    [Test]
    public async Task GetContentCategoriesAsync_WhenCultureLocaleIsEmpty_ReturnsNotFound()
    {
        var harness = new ContentCategoryHarness();
        harness.MappingHelper
            .Setup(x => x.ValidateAndNormalizeSiteCultureAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LanguageTag("en", "GB", string.Empty));

        var result = await harness.Build().GetContentCategoriesAsync(5, "en-GB", CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
            Assert.That(((NotFoundObjectResult)result.Result!).Value, Is.EqualTo("Invalid culture specified"));
        });
    }

    [Test]
    public async Task GetContentAsync_WhenCultureLocaleIsEmpty_ReturnsNotFound()
    {
        var harness = new ContentCategoryHarness();
        harness.MappingHelper
            .Setup(x => x.ValidateAndNormalizeSiteCultureAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LanguageTag("en", "GB", string.Empty));

        var result = await harness.Build().GetContentAsync(5, "en-GB", null, null, [], CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
            Assert.That(((NotFoundObjectResult)result.Result!).Value, Is.EqualTo("Invalid culture specified"));
        });
    }

    [Test]
    public void ValidateContentCategoryFilters_WhenInvalidCategoryCode_ThrowsAndRegistersValidationError()
    {
        var harness = new ContentCategoryHarness();
        harness.ValidationState.Setup(x => x.HasUnprocessableEntityErrors()).Returns(true);

        var service = harness.Build();
        var ex = Assert.ThrowsAsync<UnprocessableEntityException>(async () =>
            await InvokePrivateAsync<Task>(
                service,
                "ValidateContentCategoryFilters",
                [new[] { new ContentCategoryFilter { CategoryCode = "Invalid", SubCategoryIds = [1] } }]));

        Assert.That(ex, Is.Not.Null);
        harness.ValidationState.Verify(
            x => x.AddUnprocessableEntityError(nameof(ContentCategoryFilter.CategoryCode), It.Is<string>(m => m.Contains("Not valid category code"))),
            Times.Once);
    }

    [Test]
    public void ValidateContentCategoryFilters_WhenSubcategoriesMissing_ThrowsAndRegistersValidationError()
    {
        var harness = new ContentCategoryHarness();
        harness.ValidationState.Setup(x => x.HasUnprocessableEntityErrors()).Returns(true);

        var service = harness.Build();

        Assert.ThrowsAsync<UnprocessableEntityException>(async () =>
            await InvokePrivateAsync<Task>(
                service,
                "ValidateContentCategoryFilters",
                [new[] { new ContentCategoryFilter { CategoryCode = "Industry", SubCategoryIds = [] } }]));

        harness.ValidationState.Verify(
            x => x.AddUnprocessableEntityError(nameof(ContentCategoryFilter.SubCategoryIds), It.Is<string>(m => m.Contains("empty"))),
            Times.Once);
    }

    [Test]
    public async Task ValidateContentCategoryFilters_WhenValidFilters_DoesNotAddErrors()
    {
        var harness = new ContentCategoryHarness();
        harness.ValidationState.Setup(x => x.HasUnprocessableEntityErrors()).Returns(false);
        harness.ValidationState.Setup(x => x.HasErrors()).Returns(false);

        var service = harness.Build();

        await InvokePrivateAsync<Task>(
            service,
            "ValidateContentCategoryFilters",
            [new[] { new ContentCategoryFilter { CategoryCode = "Industry", SubCategoryIds = [1, 2] } }]);

        harness.ValidationState.Verify(x => x.AddUnprocessableEntityError(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task BuildSubCategoryIdsByCategoryCode_FillsMissingCodesWithEmptyArrays()
    {
        var method = typeof(ContentCategoryService).GetMethod("BuildSubCategoryIdsByCategoryCode", BindingFlags.Static | BindingFlags.NonPublic)!;

        var map = (Dictionary<string, int[]>)method.Invoke(null, [new[]
        {
            new ContentCategoryFilter { CategoryCode = "BusinessIssue", SubCategoryIds = [8, 9] },
            new ContentCategoryFilter { CategoryCode = null, SubCategoryIds = [99] }
        }])!;

        Assert.Multiple(() =>
        {
            Assert.That(map["BusinessIssue"], Is.EqualTo(ExpectedBusinessIssueIds));
            Assert.That(map["BusinessLine"], Is.Empty);
            Assert.That(map["ServiceArea"], Is.Empty);
            Assert.That(map["Industry"], Is.Empty);
        });
    }

    [Test]
    public void BuildSubCategoryIdsByCategoryCode_WithRepeatedCategory_UsesLastValue()
    {
        var method = typeof(ContentCategoryService).GetMethod("BuildSubCategoryIdsByCategoryCode", BindingFlags.Static | BindingFlags.NonPublic)!;

        var map = (Dictionary<string, int[]>)method.Invoke(null, [new[]
        {
            new ContentCategoryFilter { CategoryCode = "Industry", SubCategoryIds = [1] },
            new ContentCategoryFilter { CategoryCode = "Industry", SubCategoryIds = [2, 3] }
        }])!;

        Assert.That(map["Industry"], Is.EqualTo(new[] { 2, 3 }));
    }

    [Test]
    public void BuildSubCategoryIdsByCategoryCode_IgnoresEntriesWithNullSubCategoryIds()
    {
        var method = typeof(ContentCategoryService).GetMethod("BuildSubCategoryIdsByCategoryCode", BindingFlags.Static | BindingFlags.NonPublic)!;

        var map = (Dictionary<string, int[]>)method.Invoke(null, [new[]
        {
            new ContentCategoryFilter { CategoryCode = "ServiceArea", SubCategoryIds = null }
        }])!;

        Assert.That(map["ServiceArea"], Is.Empty);
    }

    [Test]
    public void BuildSubCategoryIdsByCategoryCode_AlwaysContainsAllValidCodes()
    {
        var method = typeof(ContentCategoryService).GetMethod("BuildSubCategoryIdsByCategoryCode", BindingFlags.Static | BindingFlags.NonPublic)!;

        var map = (Dictionary<string, int[]>)method.Invoke(null, [Array.Empty<ContentCategoryFilter>()])!;

        Assert.Multiple(() =>
        {
            Assert.That(map.ContainsKey("BusinessIssue"), Is.True);
            Assert.That(map.ContainsKey("BusinessLine"), Is.True);
            Assert.That(map.ContainsKey("ServiceArea"), Is.True);
            Assert.That(map.ContainsKey("Industry"), Is.True);
        });
    }

    [Test]
    public void InsightProjection_ToDto_MapsExpectedValues()
    {
        var projectionType = typeof(ContentCategoryService)
            .GetNestedType("InsightProjection", BindingFlags.NonPublic)!;

        var projection = projectionType
            .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Single(c => c.GetParameters().Length == 11)
            .Invoke([
                Guid.NewGuid(), "BlogPost", "Title", "Summary", "https://img", new DateTime(2025, 1, 15), "/url",
                new[] { 1 }, new[] { 2 }, new[] { 3 }, new[] { 4 }
            ]);

        var dto = (InsightContentDto)projectionType.GetMethod("ToDto")!.Invoke(projection, null)!;

        Assert.Multiple(() =>
        {
            Assert.That(dto.PageType, Is.EqualTo("BlogPost"));
            Assert.That(dto.TitleFull, Is.EqualTo("Title"));
            Assert.That(dto.SummaryFull, Is.EqualTo("Summary"));
            Assert.That(dto.ImageUrlFull, Is.EqualTo("https://img"));
            Assert.That(dto.URL, Is.EqualTo("/url"));
            Assert.That(dto.DisplayDate, Is.EqualTo("15 January 2025"));
        });
    }

    [Test]
    public void InsightProjection_ToDto_UsesFallbacksForNullImageAndDefaultDate()
    {
        var projectionType = typeof(ContentCategoryService)
            .GetNestedType("InsightProjection", BindingFlags.NonPublic)!;

        var projection = projectionType
            .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Single(c => c.GetParameters().Length == 11)
            .Invoke([
                Guid.NewGuid(), "Insight", "Title", "Summary", null, default(DateTime), string.Empty,
                Array.Empty<int>(), Array.Empty<int>(), Array.Empty<int>(), Array.Empty<int>()
            ]);

        var dto = (InsightContentDto)projectionType.GetMethod("ToDto")!.Invoke(projection, null)!;

        Assert.Multiple(() =>
        {
            Assert.That(dto.ImageUrlFull, Is.EqualTo(string.Empty));
            Assert.That(dto.ImageUrlTwoThirds, Is.EqualTo(string.Empty));
            Assert.That(dto.ImageUrlOneThird, Is.EqualTo(string.Empty));
            Assert.That(dto.DisplayDate, Is.EqualTo(string.Empty));
        });
    }

    #endregion

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

    private static async Task<T> InvokePrivateAsync<T>(object instance, string methodName, object[] args)
    {
        var method = instance.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)!;

        try
        {
            var result = method.Invoke(instance, args);

            if (result is Task task)
            {
                await task;
            }

            return result is T typed ? typed : default!;
        }
        catch (TargetInvocationException ex) when (ex.InnerException is not null)
        {
            throw ex.InnerException;
        }
    }

    private sealed class ContentCategoryHarness
    {
        public Mock<IContentQueryExecutor> ContentQueryExecutor { get; } = new();
        public Mock<IContentService> ContentService { get; } = new();
        public Mock<IMappingHelper> MappingHelper { get; } = new();
        public Mock<IInfoProvider<ChannelInfo>> ChannelInfoProvider { get; } = new();
        public Mock<IValidationState> ValidationState { get; } = new();
        public Mock<IInfoProvider<WebsiteChannelInfo>> WebsiteChannelInfoProvider { get; } = new();
        public Mock<IWebPageUrlRetriever> WebPageUrlRetriever { get; } = new();

        public ContentCategoryService Build() => new(
            ContentQueryExecutor.Object,
            ContentService.Object,
            MappingHelper.Object,
            ChannelInfoProvider.Object,
            ValidationState.Object,
            WebsiteChannelInfoProvider.Object,
            WebPageUrlRetriever.Object);
    }
}
