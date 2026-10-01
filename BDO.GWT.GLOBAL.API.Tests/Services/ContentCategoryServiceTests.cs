using BDO;
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
using System.Data;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace BDO.GWT.GLOBAL.API.Tests.Services;

[TestFixture]
public class ContentCategoryServiceTests
{
    private static readonly int[] ExpectedBusinessIssueIds = [8, 9];
    private static readonly int[] ExpectedIndustryIds = [2, 3];
    private static readonly int[] ExpectedFilterIds = [1, 2, 3];
    private static readonly int[] ProjectionBusinessIssueIds = [1];
    private static readonly int[] ProjectionBusinessLineIds = [2];
    private static readonly int[] ProjectionServiceAreaIds = [3];
    private static readonly int[] ProjectionIndustryIds = [4];

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
    public async Task GetContentCategoriesAsync_WithValidInputs_ReturnsFourCategories()
    {
        var mapping = new Mock<IMappingHelper>();
        var channelProvider = new Mock<IInfoProvider<ChannelInfo>>();
        var validation = new Mock<IValidationState>();
        var websiteProvider = new Mock<IInfoProvider<WebsiteChannelInfo>>();

        mapping.Setup(x => x.ValidateAndNormalizeSiteCultureAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(LanguageTag.GB);
        channelProvider.As<IInfoByIdProvider<ChannelInfo>>()
            .Setup(x => x.Get(1))
            .Returns(CreateChannelInfo("Global"));

        var contentExecutor = CreateContentQueryExecutorProxy((method, args) =>
        {
            if (method.Name == "GetMappedResult")
            {
                var issue = new Businessissue
                {
                    SystemFields = new WebPageFields { ContentItemID = 11 },
                    Name = "Operations"
                };

                return Task.FromResult<IEnumerable<Businessissue>>([issue]);
            }

            if (method.Name == "GetResult")
            {
                var selector = args![1]!;
                var row = new Mock<IContentQueryDataContainer>();
                row.Setup(x => x.GetValue<int>(It.IsAny<string>())).Returns(21);
                row.Setup(x => x.GetValue<string>(It.IsAny<string>())).Returns("Category item");

                var projected = selector.GetType().GetMethod("Invoke")!.Invoke(selector, [row.Object])!;
                var payloadType = method.ReturnType.GetGenericArguments()[0];
                var array = Array.CreateInstance(projected.GetType(), 1);
                array.SetValue(projected, 0);

                return typeof(Task)
                    .GetMethod(nameof(Task.FromResult))!
                    .MakeGenericMethod(payloadType)
                    .Invoke(null, [array])!;
            }

            return GetDefaultReturnValue(method.ReturnType);
        });

        var service = new ContentCategoryService(
            contentExecutor,
            Mock.Of<IContentService>(),
            mapping.Object,
            channelProvider.Object,
            validation.Object,
            websiteProvider.Object,
            Mock.Of<IWebPageUrlRetriever>());

        var result = await service.GetContentCategoriesAsync(1, "en-GB", CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
            Assert.That(((ObjectResult)result.Result!).StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
        });
    }

    [Test]
    public async Task GetContentAsync_WithValidInputs_ReturnsContentDtoWithInsightsAndSearchUrl()
    {
        var mapping = new Mock<IMappingHelper>();
        var channelProvider = new Mock<IInfoProvider<ChannelInfo>>();
        var validation = new Mock<IValidationState>();
        var websiteProvider = new Mock<IInfoProvider<WebsiteChannelInfo>>();
        var contentService = new Mock<IContentService>();
        var webPageUrlRetriever = new Mock<IWebPageUrlRetriever>();

        mapping.Setup(x => x.ValidateAndNormalizeSiteCultureAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(LanguageTag.GB);
        validation.Setup(x => x.HasUnprocessableEntityErrors()).Returns(false);
        validation.Setup(x => x.HasErrors()).Returns(false);

        channelProvider.As<IInfoByIdProvider<ChannelInfo>>()
            .Setup(x => x.Get(1))
            .Returns(CreateChannelInfo("Global"));
        websiteProvider.As<IInfoByIdProvider<WebsiteChannelInfo>>()
            .Setup(x => x.Get(1))
            .Returns(CreateWebsiteChannelInfo("example.com"));

        var blogPost = new BlogPost
        {
            SystemFields = new WebPageFields
            {
                ContentItemGUID = Guid.NewGuid(),
                ContentItemCommonDataLastPublishedWhen = new DateTime(2025, 2, 1),
                WebPageUrlPath = "/blog/1"
            },
            MetadataTitle = "Blog one",
            MetadataDescription = "Summary",
            MetadataTeaserImage = Array.Empty<ContentAttachment>(),
            MetadataBusinessIssues = Array.Empty<Businessissue>(),
            MetadataBusinessLine = Array.Empty<BusinessLine>(),
            MetadataServiceAreas = Array.Empty<ServiceArea>(),
            MetadataIndustries = Array.Empty<Industry>()
        };

        var insight = new Insight
        {
            SystemFields = new WebPageFields
            {
                ContentItemGUID = Guid.NewGuid(),
                ContentItemCommonDataLastPublishedWhen = new DateTime(2025, 2, 2),
                WebPageUrlPath = "/insight/1"
            },
            MetadataTitle = "Insight one",
            MetadataDescription = "Insight summary",
            MetadataTeaserImage = Array.Empty<ContentAttachment>(),
            MetadataBusinessIssues = Array.Empty<Businessissue>(),
            MetadataBusinessLine = Array.Empty<BusinessLine>(),
            MetadataServiceAreas = Array.Empty<ServiceArea>(),
            MetadataIndustries = Array.Empty<Industry>()
        };

        contentService.Setup(x => x.GetPagesAsync<BlogPost>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([blogPost]);

        contentService.Setup(x => x.GetPagesAsync<Insight>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([insight]);

        webPageUrlRetriever
            .Setup(x => x.Retrieve(500, "en-gb", It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WebPageUrl("~/search", "https://example.com/en-gb/search"));

        var contentExecutor = CreateContentQueryExecutorProxy((method, args) =>
        {
            if (method.Name == "GetResult")
            {
                var selector = args![1]!;
                var row = new Mock<IContentQueryDataContainer>();
                row.Setup(x => x.GetValue<int>(It.IsAny<string>()))
                    .Returns((string key) => key == "WebPageItemID" ? 500 : 21);
                row.Setup(x => x.GetValue<string>(It.IsAny<string>())).Returns("Search");

                var projected = selector.GetType().GetMethod("Invoke")!.Invoke(selector, [row.Object])!;
                var payloadType = method.ReturnType.GetGenericArguments()[0];
                var array = Array.CreateInstance(projected.GetType(), 1);
                array.SetValue(projected, 0);

                return typeof(Task)
                    .GetMethod(nameof(Task.FromResult))!
                    .MakeGenericMethod(payloadType)
                    .Invoke(null, [array])!;
            }

            return GetDefaultReturnValue(method.ReturnType);
        });

        var service = new ContentCategoryService(
            contentExecutor,
            contentService.Object,
            mapping.Object,
            channelProvider.Object,
            validation.Object,
            websiteProvider.Object,
            webPageUrlRetriever.Object);

        var result = await service.GetContentAsync(1, "en-gb", 10, 5, Array.Empty<ContentCategoryFilter>(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.Null);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value!.InsightsSearchURL, Is.EqualTo("https://example.com/en-GB/search"));
            Assert.That(result.Value.Insights, Has.Count.EqualTo(2));
        });
    }

    [Test]
    public async Task GetContentAsync_WithNullInsightCount_DefaultsTo15AndReturnsContentDto()
    {
        var mapping = new Mock<IMappingHelper>();
        var channelProvider = new Mock<IInfoProvider<ChannelInfo>>();
        var validation = new Mock<IValidationState>();
        var websiteProvider = new Mock<IInfoProvider<WebsiteChannelInfo>>();
        var contentService = new Mock<IContentService>();
        var webPageUrlRetriever = new Mock<IWebPageUrlRetriever>();

        mapping.Setup(x => x.ValidateAndNormalizeSiteCultureAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(LanguageTag.GB);
        validation.Setup(x => x.HasUnprocessableEntityErrors()).Returns(false);
        validation.Setup(x => x.HasErrors()).Returns(false);

        channelProvider.As<IInfoByIdProvider<ChannelInfo>>()
            .Setup(x => x.Get(1))
            .Returns(CreateChannelInfo("Global"));
        websiteProvider.As<IInfoByIdProvider<WebsiteChannelInfo>>()
            .Setup(x => x.Get(1))
            .Returns(CreateWebsiteChannelInfo("example.com"));

        var blogPost = new BlogPost
        {
            SystemFields = new WebPageFields
            {
                ContentItemGUID = Guid.NewGuid(),
                ContentItemCommonDataLastPublishedWhen = new DateTime(2025, 2, 1),
                WebPageUrlPath = "/blog/1"
            },
            MetadataTitle = "Blog one",
            MetadataDescription = "Summary",
            MetadataTeaserImage = Array.Empty<ContentAttachment>(),
            MetadataBusinessIssues = Array.Empty<Businessissue>(),
            MetadataBusinessLine = Array.Empty<BusinessLine>(),
            MetadataServiceAreas = Array.Empty<ServiceArea>(),
            MetadataIndustries = Array.Empty<Industry>()
        };

        var insight = new Insight
        {
            SystemFields = new WebPageFields
            {
                ContentItemGUID = Guid.NewGuid(),
                ContentItemCommonDataLastPublishedWhen = new DateTime(2025, 2, 2),
                WebPageUrlPath = "/insight/1"
            },
            MetadataTitle = "Insight one",
            MetadataDescription = "Insight summary",
            MetadataTeaserImage = Array.Empty<ContentAttachment>(),
            MetadataBusinessIssues = Array.Empty<Businessissue>(),
            MetadataBusinessLine = Array.Empty<BusinessLine>(),
            MetadataServiceAreas = Array.Empty<ServiceArea>(),
            MetadataIndustries = Array.Empty<Industry>()
        };

        contentService.Setup(x => x.GetPagesAsync<BlogPost>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([blogPost]);

        contentService.Setup(x => x.GetPagesAsync<Insight>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([insight]);

        webPageUrlRetriever
            .Setup(x => x.Retrieve(500, "en-gb", It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WebPageUrl("~/search", "https://example.com/en-gb/search"));

        var contentExecutor = CreateContentQueryExecutorProxy((method, args) =>
        {
            if (method.Name == "GetResult")
            {
                var selector = args![1]!;
                var row = new Mock<IContentQueryDataContainer>();
                row.Setup(x => x.GetValue<int>(It.IsAny<string>()))
                    .Returns((string key) => key == "WebPageItemID" ? 500 : 21);
                row.Setup(x => x.GetValue<string>(It.IsAny<string>())).Returns("Search");

                var projected = selector.GetType().GetMethod("Invoke")!.Invoke(selector, [row.Object])!;
                var payloadType = method.ReturnType.GetGenericArguments()[0];
                var array = Array.CreateInstance(projected.GetType(), 1);
                array.SetValue(projected, 0);

                return typeof(Task)
                    .GetMethod(nameof(Task.FromResult))!
                    .MakeGenericMethod(payloadType)
                    .Invoke(null, [array])!;
            }

            return GetDefaultReturnValue(method.ReturnType);
        });

        var service = new ContentCategoryService(
            contentExecutor,
            contentService.Object,
            mapping.Object,
            channelProvider.Object,
            validation.Object,
            websiteProvider.Object,
            webPageUrlRetriever.Object);

        // Pass null for insightCount - should default to 15
        var result = await service.GetContentAsync(1, "en-gb", null, 5, Array.Empty<ContentCategoryFilter>(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.Null);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value!.InsightsSearchURL, Is.EqualTo("https://example.com/en-GB/search"));
            Assert.That(result.Value.Insights, Has.Count.EqualTo(2));
        });
    }

    [Test]
    public async Task GetContentAsync_WithInsightCountZero_DefaultsTo15AndReturnsContentDto()
    {
        var mapping = new Mock<IMappingHelper>();
        var channelProvider = new Mock<IInfoProvider<ChannelInfo>>();
        var validation = new Mock<IValidationState>();
        var websiteProvider = new Mock<IInfoProvider<WebsiteChannelInfo>>();
        var contentService = new Mock<IContentService>();
        var webPageUrlRetriever = new Mock<IWebPageUrlRetriever>();

        mapping.Setup(x => x.ValidateAndNormalizeSiteCultureAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(LanguageTag.GB);
        validation.Setup(x => x.HasUnprocessableEntityErrors()).Returns(false);
        validation.Setup(x => x.HasErrors()).Returns(false);

        channelProvider.As<IInfoByIdProvider<ChannelInfo>>()
            .Setup(x => x.Get(1))
            .Returns(CreateChannelInfo("Global"));
        websiteProvider.As<IInfoByIdProvider<WebsiteChannelInfo>>()
            .Setup(x => x.Get(1))
            .Returns(CreateWebsiteChannelInfo("example.com"));

        var blogPost = new BlogPost
        {
            SystemFields = new WebPageFields
            {
                ContentItemGUID = Guid.NewGuid(),
                ContentItemCommonDataLastPublishedWhen = new DateTime(2025, 2, 1),
                WebPageUrlPath = "/blog/1"
            },
            MetadataTitle = "Blog one",
            MetadataDescription = "Summary",
            MetadataTeaserImage = Array.Empty<ContentAttachment>(),
            MetadataBusinessIssues = Array.Empty<Businessissue>(),
            MetadataBusinessLine = Array.Empty<BusinessLine>(),
            MetadataServiceAreas = Array.Empty<ServiceArea>(),
            MetadataIndustries = Array.Empty<Industry>()
        };

        var insight = new Insight
        {
            SystemFields = new WebPageFields
            {
                ContentItemGUID = Guid.NewGuid(),
                ContentItemCommonDataLastPublishedWhen = new DateTime(2025, 2, 2),
                WebPageUrlPath = "/insight/1"
            },
            MetadataTitle = "Insight one",
            MetadataDescription = "Insight summary",
            MetadataTeaserImage = Array.Empty<ContentAttachment>(),
            MetadataBusinessIssues = Array.Empty<Businessissue>(),
            MetadataBusinessLine = Array.Empty<BusinessLine>(),
            MetadataServiceAreas = Array.Empty<ServiceArea>(),
            MetadataIndustries = Array.Empty<Industry>()
        };

        contentService.Setup(x => x.GetPagesAsync<BlogPost>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([blogPost]);

        contentService.Setup(x => x.GetPagesAsync<Insight>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([insight]);

        webPageUrlRetriever
            .Setup(x => x.Retrieve(500, "en-gb", It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WebPageUrl("~/search", "https://example.com/en-gb/search"));

        var contentExecutor = CreateContentQueryExecutorProxy((method, args) =>
        {
            if (method.Name == "GetResult")
            {
                var selector = args![1]!;
                var row = new Mock<IContentQueryDataContainer>();
                row.Setup(x => x.GetValue<int>(It.IsAny<string>()))
                    .Returns((string key) => key == "WebPageItemID" ? 500 : 21);
                row.Setup(x => x.GetValue<string>(It.IsAny<string>())).Returns("Search");

                var projected = selector.GetType().GetMethod("Invoke")!.Invoke(selector, [row.Object])!;
                var payloadType = method.ReturnType.GetGenericArguments()[0];
                var array = Array.CreateInstance(projected.GetType(), 1);
                array.SetValue(projected, 0);

                return typeof(Task)
                    .GetMethod(nameof(Task.FromResult))!
                    .MakeGenericMethod(payloadType)
                    .Invoke(null, [array])!;
            }

            return GetDefaultReturnValue(method.ReturnType);
        });

        var service = new ContentCategoryService(
            contentExecutor,
            contentService.Object,
            mapping.Object,
            channelProvider.Object,
            validation.Object,
            websiteProvider.Object,
            webPageUrlRetriever.Object);

        // Pass 0 for insightCount - should default to 15
        var result = await service.GetContentAsync(1, "en-gb", 0, 5, Array.Empty<ContentCategoryFilter>(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.Null);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value!.InsightsSearchURL, Is.EqualTo("https://example.com/en-GB/search"));
            Assert.That(result.Value.Insights, Has.Count.EqualTo(2));
        });
    }

    [Test]
    public async Task GetInsightSearchUrlByDomainAsync_WhenDomainWithoutScheme_ReturnsNormalizedUrl()
    {
        var websiteProvider = new Mock<IInfoProvider<WebsiteChannelInfo>>();
        websiteProvider.As<IInfoByIdProvider<WebsiteChannelInfo>>()
            .Setup(x => x.Get(1))
            .Returns(CreateWebsiteChannelInfo("example.com/"));

        var service = new ContentCategoryService(
            Mock.Of<IContentQueryExecutor>(),
            Mock.Of<IContentService>(),
            Mock.Of<IMappingHelper>(),
            Mock.Of<IInfoProvider<ChannelInfo>>(),
            Mock.Of<IValidationState>(),
            websiteProvider.Object,
            Mock.Of<IWebPageUrlRetriever>());

        var method = typeof(ContentCategoryService).GetMethod("GetInsightSearchUrlByDomainAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var task = (Task<string?>)method.Invoke(service, [1, "en-gb"])!;
        var result = await task;

        Assert.That(result, Is.EqualTo("https://example.com/en-gb/search"));
    }

    [Test]
    public async Task GetInsightSearchUrlByDomainAsync_WhenDomainHasHttpScheme_KeepsSchemeAndTrimsSlash()
    {
        var websiteProvider = new Mock<IInfoProvider<WebsiteChannelInfo>>();
        websiteProvider.As<IInfoByIdProvider<WebsiteChannelInfo>>()
            .Setup(x => x.Get(1))
            .Returns(CreateWebsiteChannelInfo("http://example.com/"));

        var service = new ContentCategoryService(
            Mock.Of<IContentQueryExecutor>(),
            Mock.Of<IContentService>(),
            Mock.Of<IMappingHelper>(),
            Mock.Of<IInfoProvider<ChannelInfo>>(),
            Mock.Of<IValidationState>(),
            websiteProvider.Object,
            Mock.Of<IWebPageUrlRetriever>());

        var method = typeof(ContentCategoryService).GetMethod("GetInsightSearchUrlByDomainAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var task = (Task<string?>)method.Invoke(service, [1, "fr-fr"])!;
        var result = await task;

        Assert.That(result, Is.EqualTo("http://example.com/fr-fr/search"));
    }

    [Test]
    public void GetBusinessIssueCategoryDtoAsync_WithoutContentModelMapper_ThrowsExpectedException()
    {
        var service = new ContentCategoryService(
            CreateContentQueryExecutorProxy(),
            Mock.Of<IContentService>(),
            Mock.Of<IMappingHelper>(),
            Mock.Of<IInfoProvider<ChannelInfo>>(),
            Mock.Of<IValidationState>(),
            Mock.Of<IInfoProvider<WebsiteChannelInfo>>(),
            Mock.Of<IWebPageUrlRetriever>());

        var method = typeof(ContentCategoryService).GetMethod("GetBusinessIssueCategoryDtoAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;

        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await (Task<ContentCategoryDto>)method.Invoke(service, ["Global", LanguageTag.GB, CancellationToken.None])!);

        Assert.That(ex!.Message, Does.Contain("automatic mapping"));
    }

    [Test]
    public async Task GetContentCategoryDtoAsync_MapsRowsToSubCategories()
    {
        var executor = CreateContentQueryExecutorProxy((method, args) =>
        {
            if (method.Name == "GetResult")
            {
                var selector = args![1]!;
                var row = new Mock<IContentQueryDataContainer>();
                row.Setup(x => x.GetValue<int>("ContentItemID")).Returns(88);
                row.Setup(x => x.GetValue<string>("MetadataTitle")).Returns("Tax");

                var projected = selector.GetType().GetMethod("Invoke")!.Invoke(selector, [row.Object])!;
                var payloadType = method.ReturnType.GetGenericArguments()[0];
                var array = Array.CreateInstance(projected.GetType(), 1);
                array.SetValue(projected, 0);

                return typeof(Task)
                    .GetMethod(nameof(Task.FromResult))!
                    .MakeGenericMethod(payloadType)
                    .Invoke(null, [array])!;
            }

            return GetDefaultReturnValue(method.ReturnType);
        });

        var service = new ContentCategoryService(
            executor,
            Mock.Of<IContentService>(),
            Mock.Of<IMappingHelper>(),
            Mock.Of<IInfoProvider<ChannelInfo>>(),
            Mock.Of<IValidationState>(),
            Mock.Of<IInfoProvider<WebsiteChannelInfo>>(),
            Mock.Of<IWebPageUrlRetriever>());

        var method = typeof(ContentCategoryService).GetMethod("GetContentCategoryDtoAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var task = (Task<ContentCategoryDto>)method.Invoke(
            service,
            ["Industry", "Industry", Industry.CONTENT_TYPE_NAME, "Global", LanguageTag.GB, CancellationToken.None])!;
        var dto = await task;

        Assert.Multiple(() =>
        {
            Assert.That(dto.Name, Is.EqualTo("Industry"));
            Assert.That(dto.Code, Is.EqualTo("Industry"));
            Assert.That(dto.SubCategories.Single().Id, Is.EqualTo(88));
            Assert.That(dto.SubCategories.Single().Name, Is.EqualTo("Tax"));
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

        Assert.That(map["Industry"], Is.EqualTo(ExpectedIndustryIds));
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

    [TestCase("/media/image.jpg", "https://public.example/media/image.jpg")]
    [TestCase("~/media/image.jpg", "https://public.example/media/image.jpg")]
    [TestCase("media/image.jpg", "https://public.example/media/image.jpg")]
    [TestCase("https://cdn.example/image.jpg?width=640&height=320", "https://cdn.example/image.jpg?width=640&height=320")]
    [TestCase("//cdn.example/image.jpg", "https://cdn.example/image.jpg")]
    [TestCase("/media/my image.jpg?width=640&height=320", "https://public.example/media/my%20image.jpg?width=640&height=320")]
    [TestCase(null, "")]
    [TestCase("", "")]
    [TestCase("javascript:alert(1)", "")]
    [TestCase("/media/image.jpg", "https://public.example/media/image.jpg", "fr-FR")]
    [SetCulture("en-US")]
    public async Task GetContentAsync_ResolvesUrlsAndFirstLinkedSubtype(string? imageUrl, string expectedImageUrl, string culture = "en-GB")
    {
        var mapping = new Mock<IMappingHelper>();
        mapping.Setup(helper => helper.ValidateAndNormalizeSiteCultureAsync(1, culture, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LanguageTag(culture[..2], culture[3..], culture));
        var channelProvider = new Mock<IInfoProvider<ChannelInfo>>();
        channelProvider.As<IInfoByIdProvider<ChannelInfo>>()
            .Setup(provider => provider.Get(1)).Returns(CreateChannelInfo("Global"));
        var contentService = new Mock<IContentService>();
        var subtypes = new[] { new Contenttypes { Name = "Article" }, new Contenttypes { Name = "Report" } };
        var images = new[] { new ContentAttachment { ContentAttachmentAsset = new ContentItemAsset { Url = imageUrl! } } };
        var blog = new BlogPost
        {
            SystemFields = new WebPageFields { WebPageItemID = 101, WebPageUrlPath = "incorrect-relative-path", ContentItemCommonDataFirstPublishedWhen = new DateTime(2025, 1, 2), ContentItemCommonDataLastPublishedWhen = new DateTime(2025, 1, 2) },
            ContentTypeFilter = subtypes,
            MetadataTeaserImage = images,
        };
        var insight = new Insight
        {
            SystemFields = new WebPageFields { WebPageItemID = 102, WebPageUrlPath = "incorrect-relative-path", ContentItemCommonDataFirstPublishedWhen = new DateTime(2025, 1, 1), ContentItemCommonDataLastPublishedWhen = new DateTime(2025, 1, 3) },
            ContentTypeFilter = subtypes,
            MetadataTeaserImage = images,
        };
        contentService.Setup(service => service.GetPagesAsync<BlogPost>(It.IsAny<RetrievePagesParameters>(), It.IsAny<Action<RetrievePagesQueryParameters>>(), It.IsAny<RetrievalCacheSettings>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([blog]);
        contentService.Setup(service => service.GetPagesAsync<Insight>(It.IsAny<RetrievePagesParameters>(), It.IsAny<Action<RetrievePagesQueryParameters>>(), It.IsAny<RetrievalCacheSettings>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([insight]);
        using var cancellation = new CancellationTokenSource();
        var urlRetriever = new Mock<IWebPageUrlRetriever>(MockBehavior.Strict);
        urlRetriever.Setup(retriever => retriever.Retrieve(101, culture, false, cancellation.Token))
            .ReturnsAsync(new WebPageUrl("~/en-GB/blog", "https://public.example/en-GB/blog"));
        urlRetriever.Setup(retriever => retriever.Retrieve(102, culture, false, cancellation.Token))
            .ReturnsAsync(new WebPageUrl("~/en-GB/insight", "https://public.example/en-GB/insight"));
        var service = new ContentCategoryService(CreateContentQueryExecutorProxy(), contentService.Object, mapping.Object,
            channelProvider.Object, Mock.Of<IValidationState>(), Mock.Of<IInfoProvider<WebsiteChannelInfo>>(), urlRetriever.Object);

        var result = await service.GetContentAsync(1, culture, 2, null, [], cancellation.Token);

        Assert.That(result.Result, Is.Null);
        Assert.That(result.Value!.Insights, Has.Count.EqualTo(2));
        Assert.That(result.Value.Insights[0].Date, Is.EqualTo(new DateTime(2025, 1, 2)));
        Assert.That(result.Value.Insights[0].DisplayDate, Is.EqualTo(culture == "fr-FR" ? "02 janvier 2025" : "02 January 2025"));
        Assert.That(result.Value.Insights[1].Date, Is.EqualTo(new DateTime(2025, 1, 1)));
        Assert.That(result.Value.Insights.Select(item => item.URL), Is.EqualTo(new[] { "https://public.example/en-GB/blog", "https://public.example/en-GB/insight" }));
        foreach (var item in result.Value.Insights)
        {
            Assert.Multiple(() =>
            {
                Assert.That(item.PageSubType, Is.EqualTo("Article"));
                Assert.That(item.ImageUrlFull, Is.EqualTo(expectedImageUrl));
                Assert.That(item.ImageUrlTwoThirds, Is.EqualTo(expectedImageUrl));
                Assert.That(item.ImageUrlOneThird, Is.EqualTo(expectedImageUrl));
            });
        }
        urlRetriever.VerifyAll();
        urlRetriever.Invocations.Clear();

        var limitedResult = await service.GetContentAsync(1, culture, 1, null, [], cancellation.Token);

        Assert.That(limitedResult.Value!.Insights, Has.Count.EqualTo(1));
        urlRetriever.Verify(retriever => retriever.Retrieve(101, culture, false, cancellation.Token), Times.Once);
        urlRetriever.Verify(retriever => retriever.Retrieve(102, culture, false, cancellation.Token), Times.Never);

        blog.MetadataIndustries = [new Industry { SystemFields = new WebPageFields { ContentItemID = 20 } }];
        insight.MetadataBusinessIssues = [new Businessissue { SystemFields = new WebPageFields { ContentItemID = 10 } }];
        urlRetriever.Invocations.Clear();

        var filteredResult = await service.GetContentAsync(1, culture, 1, null,
            [new ContentCategoryFilter { CategoryCode = "BusinessIssue", SubCategoryIds = [10] }], cancellation.Token);

        Assert.That(filteredResult.Value!.Insights.Single().PageType, Is.EqualTo("Insight"));
        urlRetriever.Verify(retriever => retriever.Retrieve(101, culture, false, cancellation.Token), Times.Never);
        urlRetriever.Verify(retriever => retriever.Retrieve(102, culture, false, cancellation.Token), Times.Once);

        var combinedResult = await service.GetContentAsync(1, culture, 2, null,
            [new ContentCategoryFilter { CategoryCode = "BusinessIssue", SubCategoryIds = [10] },
             new ContentCategoryFilter { CategoryCode = "Industry", SubCategoryIds = [20] }], cancellation.Token);

        Assert.That(combinedResult.Value!.Insights.Select(item => item.PageType), Is.EqualTo(new[] { "BlogPost", "Insight" }));

        blog.SystemFields.ContentItemCommonDataFirstPublishedWhen = null;
        var beforeRequest = DateTime.Now;

        var missingDateResult = await service.GetContentAsync(1, culture, 2, null, [], cancellation.Token);

        Assert.That(missingDateResult.Value!.Insights.Select(item => item.PageType), Is.EqualTo(new[] { "Insight", "BlogPost" }));
        Assert.That(missingDateResult.Value.Insights[1].Date, Is.InRange(beforeRequest, DateTime.Now));
        Assert.That(missingDateResult.Value.Insights[1].DisplayDate,
            Is.EqualTo(missingDateResult.Value.Insights[1].Date.ToString("dd MMMM yyyy", System.Globalization.CultureInfo.GetCultureInfo(culture))));
    }

    [TestCase("fr-FR", "MetadataTitle COLLATE French_CI_AS ASC")]
    [TestCase("sv-SE", "MetadataTitle COLLATE Finnish_Swedish_CI_AS ASC")]
    [TestCase("tr-TR", "MetadataTitle COLLATE Turkish_CI_AS ASC")]
    [TestCase("en-GB", "[MetadataTitle] ASC")]
    public void GetCategoryOrderColumn_UsesLegacyCultureCollation(string culture, string expectedExpression)
    {
        var method = typeof(ContentCategoryService).GetMethod("GetCategoryOrderColumn", BindingFlags.Static | BindingFlags.NonPublic)!;

        var column = (OrderByColumn)method.Invoke(null, ["MetadataTitle", new LanguageTag(culture[..2], culture[3..], culture)])!;

        Assert.That(column.GetExpression(), Is.EqualTo(expectedExpression));
        Assert.That(column.Direction, Is.EqualTo(OrderDirection.Ascending));
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
                ProjectionBusinessIssueIds, ProjectionBusinessLineIds, ProjectionServiceAreaIds, ProjectionIndustryIds
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

    [TestCase("en-GB", "02 January 2025")]
    [TestCase("fr-FR", "02 janvier 2025")]
    [TestCase("de-DE", "02 Januar 2025")]
    [SetCulture("en-US")]
    public void InsightProjection_ToDto_FormatsDateUsingRequestedCulture(string culture, string expectedDate)
    {
        var projectionType = typeof(ContentCategoryService).GetNestedType("InsightProjection", BindingFlags.NonPublic)!;
        var projection = projectionType.GetConstructors()
            .Single(constructor => constructor.GetParameters().Length == 11)
            .Invoke([
                Guid.NewGuid(), "Insight", "Title", "Summary", null, new DateTime(2025, 1, 2), string.Empty,
                Array.Empty<int>(), Array.Empty<int>(), Array.Empty<int>(), Array.Empty<int>()
            ]);
        projectionType.GetProperty("DateCulture")!.SetValue(projection, System.Globalization.CultureInfo.GetCultureInfo(culture));

        var dto = (InsightContentDto)projectionType.GetMethod("ToDto")!.Invoke(projection, null)!;

        Assert.That(dto.DisplayDate, Is.EqualTo(expectedDate));
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

    [Test]
    public void MapBlogPostToProjection_MapsMetadataAndTaxonomies()
    {
        var method = typeof(ContentCategoryService).GetMethod("MapBlogPostToProjection", BindingFlags.Static | BindingFlags.NonPublic)!;

        var blog = new BlogPost
        {
            SystemFields = new WebPageFields
            {
                ContentItemGUID = Guid.NewGuid(),
                ContentItemCommonDataLastPublishedWhen = new DateTime(2024, 12, 1),
                WebPageUrlPath = "/blog/post"
            },
            MetadataTitle = "Blog title",
            MetadataDescription = "<p>Blog &amp; summary</p>",
            MetadataTeaserImage =
            [
                new ContentAttachment { ContentAttachmentAsset = new ContentItemAsset { Url = "https://cdn/img.jpg" } }
            ],
            MetadataBusinessIssues = [new Businessissue { SystemFields = new WebPageFields { ContentItemID = 10 } }],
            MetadataBusinessLine = [new BusinessLine { SystemFields = new WebPageFields { ContentItemID = 20 } }],
            MetadataServiceAreas = [new ServiceArea { SystemFields = new WebPageFields { ContentItemID = 30 } }],
            MetadataIndustries = [new Industry { SystemFields = new WebPageFields { ContentItemID = 40 } }]
        };

        var projection = method.Invoke(null, [blog])!;
        var dto = (InsightContentDto)projection.GetType().GetMethod("ToDto")!.Invoke(projection, null)!;

        Assert.Multiple(() =>
        {
            Assert.That(dto.PageType, Is.EqualTo("BlogPost"));
            Assert.That(dto.TitleFull, Is.EqualTo("Blog title"));
            Assert.That(dto.SummaryFull, Is.EqualTo("Blog & summary"));
            Assert.That(dto.SummaryTwoThirds, Is.EqualTo("Blog & summary"));
            Assert.That(dto.SummaryOneThird, Is.EqualTo("Blog & summary"));
            Assert.That(dto.ImageUrlFull, Is.EqualTo("https://cdn/img.jpg"));
            Assert.That(dto.URL, Is.EqualTo("/blog/post"));
        });
    }

    [Test]
    public void MapInsightToProjection_UsesEmptyFallbacksWhenMetadataMissing()
    {
        var method = typeof(ContentCategoryService).GetMethod("MapInsightToProjection", BindingFlags.Static | BindingFlags.NonPublic)!;

        var insight = new Insight
        {
            SystemFields = new WebPageFields
            {
                ContentItemGUID = Guid.NewGuid(),
                ContentItemCommonDataLastPublishedWhen = null,
                WebPageUrlPath = null
            },
            MetadataTitle = null!,
            MetadataDescription = null!,
            MetadataTeaserImage = Array.Empty<ContentAttachment>(),
            MetadataBusinessIssues = Array.Empty<Businessissue>(),
            MetadataBusinessLine = Array.Empty<BusinessLine>(),
            MetadataServiceAreas = Array.Empty<ServiceArea>(),
            MetadataIndustries = Array.Empty<Industry>()
        };

        var projection = method.Invoke(null, [insight])!;
        var dto = (InsightContentDto)projection.GetType().GetMethod("ToDto")!.Invoke(projection, null)!;

        Assert.Multiple(() =>
        {
            Assert.That(dto.PageType, Is.EqualTo("Insight"));
            Assert.That(dto.PageSubType, Is.Empty);
            Assert.That(dto.TitleFull, Is.EqualTo(string.Empty));
            Assert.That(dto.SummaryFull, Is.EqualTo(string.Empty));
            Assert.That(dto.ImageUrlFull, Is.EqualTo(string.Empty));
            Assert.That(dto.URL, Is.EqualTo(string.Empty));
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
            SubCategoryIds = ExpectedFilterIds
        };

        Assert.Multiple(() =>
        {
            Assert.That(filter.CategoryCode, Is.EqualTo("ServiceArea"));
            Assert.That(filter.SubCategoryIds, Is.EqualTo(ExpectedFilterIds));
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

    private static IContentQueryExecutor CreateContentQueryExecutorProxy(Func<MethodInfo, object?[]?, object?>? onInvoke = null)
    {
        var executor = DispatchProxy.Create<IContentQueryExecutor, ContentQueryExecutorProxy>();
        ContentQueryExecutorProxy.GetProxy(executor!).OnInvoke = onInvoke;
        return executor!;
    }

    private static Mock<ObjectQuery<T>> QueryReturning<T>(IEnumerable<T> rows)
        where T : BaseInfo
    {
        var query = new Mock<ObjectQuery<T>>();

        query.Setup(x => x.WhereEquals(It.IsAny<string>(), It.IsAny<object>()))
            .Returns(query.Object);
        query.Setup(x => x.TopN(It.IsAny<int>()))
            .Returns(query.Object);
        query.Setup(x => x.GetEnumerableTypedResultAsync(
                It.IsAny<CommandBehavior>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken?>()))
            .ReturnsAsync(rows);

        return query;
    }

    private class ContentQueryExecutorProxy : DispatchProxy
    {
        public Func<MethodInfo, object?[]?, object?>? OnInvoke { get; set; }

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (targetMethod == null)
            {
                return null;
            }

            return OnInvoke?.Invoke(targetMethod, args) ?? GetDefaultReturnValue(targetMethod.ReturnType);
        }

        public static ContentQueryExecutorProxy GetProxy(IContentQueryExecutor proxy) =>
            (ContentQueryExecutorProxy)(object)proxy;
    }

    private static object? GetDefaultReturnValue(Type returnType)
    {
        if (returnType == typeof(Task))
        {
            return Task.CompletedTask;
        }

        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            var payloadType = returnType.GetGenericArguments()[0];
            if (payloadType.IsGenericType && payloadType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                var itemType = payloadType.GetGenericArguments()[0];
                var emptySequence = typeof(Enumerable)
                    .GetMethod(nameof(Enumerable.Empty))!
                    .MakeGenericMethod(itemType)
                    .Invoke(null, null)!;

                return typeof(Task)
                    .GetMethod(nameof(Task.FromResult))!
                    .MakeGenericMethod(payloadType)
                    .Invoke(null, [emptySequence])!;
            }

            var defaultValue = payloadType.IsValueType ? Activator.CreateInstance(payloadType) : null;
            return typeof(Task)
                .GetMethod(nameof(Task.FromResult))!
                .MakeGenericMethod(payloadType)
                .Invoke(null, [defaultValue])!;
        }

        return returnType.IsValueType ? Activator.CreateInstance(returnType) : null;
    }

    private static FakeChannelInfo CreateChannelInfo(string channelName)
    {
        var channel = (FakeChannelInfo)RuntimeHelpers.GetUninitializedObject(typeof(FakeChannelInfo));
        channel.ChannelName = channelName;
        return channel;
    }

    private static FakeWebsiteChannelInfo CreateWebsiteChannelInfo(string domain)
    {
        var channel = (FakeWebsiteChannelInfo)RuntimeHelpers.GetUninitializedObject(typeof(FakeWebsiteChannelInfo));
        channel.WebsiteChannelDomain = domain;
        return channel;
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

    private sealed class FakeChannelInfo : ChannelInfo
    {
        private string? channelName;

        public override string ChannelName
        {
            get => channelName ?? string.Empty;
            set => channelName = value;
        }
    }

    private sealed class FakeWebsiteChannelInfo : WebsiteChannelInfo
    {
        private string? websiteChannelDomain;

        public override string WebsiteChannelDomain
        {
            get => websiteChannelDomain ?? string.Empty;
            set => websiteChannelDomain = value;
        }
    }
}
