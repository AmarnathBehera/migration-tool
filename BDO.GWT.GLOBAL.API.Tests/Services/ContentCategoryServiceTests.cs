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

using CMS.Helpers;

using CMS.Websites;

using Microsoft.Extensions.Configuration;

using Microsoft.Extensions.Logging;

using Moq;

using NUnit.Framework;

using System.Reflection;

using System.Runtime.CompilerServices;

using System.Runtime.ExceptionServices;

namespace BDO.GWT.GLOBAL.API.Tests.Services;

[TestFixture]

public class ContentCategoryServiceTests

{

    private const string PageBase = "https://public.example";

    private static readonly int[] Ids1 = [1];

    private static readonly int[] Ids2And3 = [2, 3];

    private static readonly int[] Ids3And4 = [3, 4];

    private static readonly int[] Ids8And9 = [8, 9];

    private static readonly int[] Ids10 = [10];

    private static readonly int[] Ids20 = [20];

    private static readonly int[] Ids99 = [99];

    private static readonly string[] ExpectedCategoryCodes = ["BusinessIssue", "BusinessLine", "ServiceArea", "Industry"];

    private static readonly string[] ExpectedCategoryNames = ["Business Issue", "Business Line", "Service Area", "Industry"];

    private static readonly string[] BlogThenInsight = ["BlogPost", "Insight"];

    private static readonly string[] InsightThenBlog = ["Insight", "BlogPost"];

    private static readonly string[] ExpectedLinkingFieldNames =

    [

        nameof(BlogPost.MetadataBusinessIssues),

        nameof(BlogPost.MetadataBusinessLine),

        nameof(BlogPost.MetadataServiceAreas),

        nameof(BlogPost.MetadataIndustries),

    ];

    #region Interface / basic structure

    [Test]

    public void ContentCategoryService_ImplementsInterface()

    {

        Assert.That(typeof(IContentCategoryService).IsAssignableFrom(typeof(ContentCategoryService)), Is.True);

    }

    [Test]

    public void Interface_HasExpectedMethodsAndReturnTypes()

    {

        var type = typeof(IContentCategoryService);

        Assert.Multiple(() =>

        {

            Assert.That(type.GetMethod(nameof(IContentCategoryService.GetContentCategoriesAsync))!.ReturnType,

                Is.EqualTo(typeof(Task<IEnumerable<ContentCategoryDto>>)));

            Assert.That(type.GetMethod(nameof(IContentCategoryService.GetContentAsync))!.ReturnType,

                Is.EqualTo(typeof(Task<ContentDto>)));

        });

    }

    #endregion

    #region GetContentCategoriesAsync

    [Test]

    public async Task GetContentCategoriesAsync_ReturnsFourCategoriesWithSubCategories()

    {

        var harness = new Harness();

        var result = (await harness.Build().GetContentCategoriesAsync(1, "en-GB", CancellationToken.None)).ToList();

        Assert.Multiple(() =>

        {

            Assert.That(result, Has.Count.EqualTo(4));

            Assert.That(result.Select(x => x.Code), Is.EqualTo(ExpectedCategoryCodes));

            Assert.That(result.Select(x => x.Name), Is.EqualTo(ExpectedCategoryNames));

            foreach (var category in result)

            {

                Assert.That(category.SubCategories, Has.Count.EqualTo(1));

                Assert.That(category.SubCategories[0].Id, Is.EqualTo(88));

                Assert.That(category.SubCategories[0].Name, Is.EqualTo("Tax"));

            }

        });

    }

    [Test]

    public async Task GetContentCategoriesAsync_LoadsEachCategoryThroughCache()

    {

        var harness = new Harness();

        await harness.Build().GetContentCategoriesAsync(1, "en-GB", CancellationToken.None);

        Assert.That(harness.CacheProxy.LoadCount, Is.EqualTo(4));

    }

    [Test]

    public async Task GetContentCategoriesAsync_PassesRequestedCultureToMappingHelper()

    {

        var harness = new Harness();

        await harness.Build().GetContentCategoriesAsync(1, "fr-FR", CancellationToken.None);

        harness.Mapping.Verify(x => x.ValidateAndNormalizeSiteCultureAsync(1, "fr-FR", It.IsAny<CancellationToken>()), Times.Once);

    }

    [Test]

    public void GetContentCategoriesAsync_WhenChannelNotFound_ThrowsNotFound()

    {

        var harness = new Harness { Channel = null };

        Assert.ThrowsAsync<NotFoundException>(async () =>

            await harness.Build().GetContentCategoriesAsync(1, "en-GB", CancellationToken.None));

    }

    [Test]

    public void GetContentCategoriesAsync_WhenChannelNameEmpty_ThrowsNotFound()

    {

        var harness = new Harness { Channel = CreateChannelInfo(string.Empty) };

        Assert.ThrowsAsync<NotFoundException>(async () =>

            await harness.Build().GetContentCategoriesAsync(1, "en-GB", CancellationToken.None));

    }

    [Test]

    public void GetContentCategoriesAsync_WhenCultureCannotBeNormalized_ThrowsValidationException()

    {

        var harness = new Harness { Tag = null };

        Assert.ThrowsAsync<ValidationException>(async () =>

            await harness.Build().GetContentCategoriesAsync(1, "xx", CancellationToken.None));

        harness.Validation.Verify(x => x.AddError("culture", It.Is<string>(m => m.Contains("xx"))), Times.Once);

    }

    [Test]

    public void GetContentCategoriesAsync_WhenLocaleIsEmpty_ThrowsValidationException()

    {

        var harness = new Harness { Tag = new LanguageTag("en", "GB", string.Empty) };

        Assert.ThrowsAsync<ValidationException>(async () =>

            await harness.Build().GetContentCategoriesAsync(1, "en-GB", CancellationToken.None));

    }

    [Test]

    public void GetContentCategoriesAsync_WhenOnlyUnprocessableErrorsExist_ThrowsUnprocessableEntity()

    {

        var harness = new Harness { Tag = null };

        harness.Validation.Setup(x => x.HasErrors()).Returns(false);

        harness.Validation.Setup(x => x.HasUnprocessableEntityErrors()).Returns(true);

        Assert.ThrowsAsync<UnprocessableEntityException>(async () =>

            await harness.Build().GetContentCategoriesAsync(1, "en-GB", CancellationToken.None));

    }

    #endregion

    #region GetContentAsync

    [Test]

    public async Task GetContentAsync_ReturnsInsightsOrderedByDateAndSearchUrl()

    {

        var harness = new Harness();

        harness.BlogPosts.Add(NewBlog(101, new DateTime(2025, 1, 2)));

        harness.Insights.Add(NewInsight(102, new DateTime(2025, 1, 1)));

        var result = await harness.Build().GetContentAsync(1, "en-GB", 10, 5, [], CancellationToken.None);

        Assert.Multiple(() =>

        {

            Assert.That(result.InsightsSearchURL, Does.Contain("search").IgnoreCase.Or.Contain("500"));

            Assert.That(result.Insights, Has.Count.EqualTo(2));

            Assert.That(result.Insights[0].PageType, Is.EqualTo("BlogPost"));

            Assert.That(result.Insights[0].Date, Is.EqualTo(new DateTime(2025, 1, 2)));

            Assert.That(result.Insights[0].URL, Does.Contain("101"));

            Assert.That(result.Insights[1].PageType, Is.EqualTo("Insight"));

            Assert.That(result.Insights[1].URL, Does.Contain("102"));

        });

    }

    [Test]

    public async Task GetContentAsync_MapsTitleSummaryAndImageFallbacks()

    {

        var harness = new Harness();

        harness.BlogPosts.Add(NewBlog(101, new DateTime(2025, 1, 2)));

        var result = await harness.Build().GetContentAsync(1, "en-GB", 10, null, [], CancellationToken.None);

        var dto = result.Insights.Single();

        Assert.Multiple(() =>

        {

            Assert.That(dto.TitleFull, Is.EqualTo("Blog title"));

            Assert.That(dto.TitleTwoThirds, Is.EqualTo("Blog title"));

            Assert.That(dto.TitleOneThird, Is.EqualTo("Blog title"));

            Assert.That(dto.SummaryFull, Does.Contain("summary"));

            Assert.That(dto.SummaryFull, Does.Not.Contain("<p>"));

            Assert.That(dto.SummaryTwoThirds, Is.EqualTo(dto.SummaryFull));

            Assert.That(dto.SummaryOneThird, Is.EqualTo(dto.SummaryFull));

            Assert.That(dto.ImageUrlFull, Is.EqualTo(string.Empty));

            Assert.That(dto.ImageUrlTwoThirds, Is.EqualTo(string.Empty));

            Assert.That(dto.ImageUrlOneThird, Is.EqualTo(string.Empty));

            Assert.That(dto.PageSubType, Is.EqualTo(string.Empty));

        });

    }

    [Test]

    public async Task GetContentAsync_UsesFirstContentTypeFilterAsSubType()

    {

        var harness = new Harness();

        var blog = NewBlog(101, new DateTime(2025, 1, 2));

        blog.ContentTypeFilter = new[] { new Contenttypes { Name = "Article" }, new Contenttypes { Name = "Report" } };

        harness.BlogPosts.Add(blog);

        var result = await harness.Build().GetContentAsync(1, "en-GB", 10, null, [], CancellationToken.None);

        Assert.That(result.Insights.Single().PageSubType, Is.EqualTo("Article"));

    }

    [Test]

    public async Task GetContentAsync_WithNullTitleAndDescription_UsesEmptyStrings()

    {

        var harness = new Harness();

        var insight = NewInsight(102, new DateTime(2025, 1, 1));

        insight.MetadataTitle = null!;

        insight.MetadataDescription = null!;

        harness.Insights.Add(insight);

        var result = await harness.Build().GetContentAsync(1, "en-GB", 10, null, [], CancellationToken.None);

        var dto = result.Insights.Single();

        Assert.Multiple(() =>

        {

            Assert.That(dto.TitleFull, Is.EqualTo(string.Empty));

            Assert.That(dto.SummaryFull, Is.EqualTo(string.Empty));

        });

    }

    [Test]

    public async Task GetContentAsync_FormatsDisplayDateWithDefaultFormat()

    {

        var harness = new Harness();

        harness.BlogPosts.Add(NewBlog(101, new DateTime(2025, 1, 2)));

        var result = await harness.Build().GetContentAsync(1, "en-GB", 10, null, [], CancellationToken.None);

        Assert.That(result.Insights.Single().DisplayDate, Is.EqualTo("02 January 2025"));

    }

    [Test]

    public async Task GetContentAsync_FormatsDisplayDateWithConfiguredFormat()

    {

        var harness = new Harness();

        harness.Config["Content:DisplayDateFormat"] = "yyyy-MM-dd";

        harness.BlogPosts.Add(NewBlog(101, new DateTime(2025, 1, 2)));

        var result = await harness.Build().GetContentAsync(1, "en-GB", 10, null, [], CancellationToken.None);

        Assert.That(result.Insights.Single().DisplayDate, Is.EqualTo("2025-01-02"));

    }

    [Test]

    public async Task GetContentAsync_WhenPublishDateMissing_UsesCurrentDateAndSortsLast()

    {

        var harness = new Harness();

        harness.BlogPosts.Add(NewBlog(101, null));

        harness.Insights.Add(NewInsight(102, new DateTime(2025, 1, 1)));

        var before = DateTime.Now;

        var result = await harness.Build().GetContentAsync(1, "en-GB", 10, null, [], CancellationToken.None);

        Assert.Multiple(() =>

        {

            Assert.That(result.Insights.Select(x => x.PageType), Is.EqualTo(InsightThenBlog));

            Assert.That(result.Insights[1].Date, Is.InRange(before, DateTime.Now));

        });

    }

    [Test]

    public async Task GetContentAsync_WhenCultureNotAvailableOnServer_FallsBackAndStillReturns()

    {

        var harness = new Harness { Tag = new LanguageTag("xx", "YY", "!!bad-culture") };

        harness.BlogPosts.Add(NewBlog(101, new DateTime(2025, 1, 2)));

        var result = await harness.Build().GetContentAsync(1, "xx-YY", 10, null, [], CancellationToken.None);

        Assert.Multiple(() =>

        {

            Assert.That(result.Insights, Has.Count.EqualTo(1));

            Assert.That(result.Insights[0].DisplayDate, Is.Not.Empty);

        });

    }

    [Test]

    public async Task GetContentAsync_DeduplicatesPagesByWebPageItemId()

    {

        var harness = new Harness();

        harness.BlogPosts.Add(NewBlog(101, new DateTime(2025, 1, 2)));

        harness.Insights.Add(NewInsight(101, new DateTime(2025, 1, 2)));

        var result = await harness.Build().GetContentAsync(1, "en-GB", 10, null, [], CancellationToken.None);

        Assert.Multiple(() =>

        {

            Assert.That(result.Insights, Has.Count.EqualTo(1));

            Assert.That(result.Insights[0].PageType, Is.EqualTo("BlogPost"));

        });

    }

    [TestCase(1, 1)]

    [TestCase(2, 2)]

    [TestCase(null, 2)]

    [TestCase(0, 2)]

    [TestCase(-5, 2)]

    public async Task GetContentAsync_LimitsResultsToRequestedCount(int? insightCount, int expected)

    {

        var harness = new Harness();

        harness.BlogPosts.Add(NewBlog(101, new DateTime(2025, 1, 2)));

        harness.Insights.Add(NewInsight(102, new DateTime(2025, 1, 1)));

        var result = await harness.Build().GetContentAsync(1, "en-GB", insightCount, null, [], CancellationToken.None);

        Assert.That(result.Insights, Has.Count.EqualTo(expected));

    }

    [Test]

    public async Task GetContentAsync_WithNoPages_ReturnsEmptyInsightsButSearchUrl()

    {

        var harness = new Harness();

        var result = await harness.Build().GetContentAsync(1, "en-GB", 10, null, [], CancellationToken.None);

        Assert.Multiple(() =>

        {

            Assert.That(result.Insights, Is.Empty);

            Assert.That(result.InsightsSearchURL, Is.Not.Null);

        });

    }

    [Test]

    public async Task GetContentAsync_WithNullFilters_TreatedAsNoFilters()

    {

        var harness = new Harness();

        harness.BlogPosts.Add(NewBlog(101, new DateTime(2025, 1, 2)));

        var result = await harness.Build().GetContentAsync(1, "en-GB", 10, null, null!, CancellationToken.None);

        Assert.That(result.Insights, Has.Count.EqualTo(1));

    }

    [Test]

    public async Task GetContentAsync_WithoutFilters_UsesCacheForPageQueries()

    {

        var harness = new Harness();

        harness.BlogPosts.Add(NewBlog(101, new DateTime(2025, 1, 2)));

        await harness.Build().GetContentAsync(1, "en-GB", 10, null, [], CancellationToken.None);

        Assert.That(harness.CacheProxy.LoadCount, Is.EqualTo(2));

    }

    [Test]

    public async Task GetContentAsync_WithFilters_BypassesCacheAndReturnsMatches()

    {

        var harness = new Harness();

        harness.BlogPosts.Add(NewBlog(101, new DateTime(2025, 1, 2)));

        harness.Insights.Add(NewInsight(102, new DateTime(2025, 1, 1)));

        ContentCategoryFilter[] filters =

        [

            new() { CategoryCode = "BusinessIssue", SubCategoryIds = Ids10 },

            new() { CategoryCode = "Industry", SubCategoryIds = Ids20 },

        ];

        var result = await harness.Build().GetContentAsync(1, "en-GB", 10, null, filters, CancellationToken.None);

        Assert.Multiple(() =>

        {

            Assert.That(harness.CacheProxy.LoadCount, Is.EqualTo(0));

            Assert.That(result.Insights.Select(x => x.PageType), Is.EqualTo(BlogThenInsight));

        });

    }

    [Test]

    public async Task GetContentAsync_WhenPageUrlCannotBeResolved_ReturnsEmptyUrl()

    {

        var harness = new Harness();

        harness.UrlRetriever

            .Setup(x => x.Retrieve(101, It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))

            .Returns(Task.FromResult<WebPageUrl>(null!));

        harness.BlogPosts.Add(NewBlog(101, new DateTime(2025, 1, 2)));

        var result = await harness.Build().GetContentAsync(1, "en-GB", 10, null, [], CancellationToken.None);

        Assert.That(result.Insights.Single().URL, Is.EqualTo(string.Empty));

    }

    [Test]

    public async Task GetContentAsync_WhenNoSearchPageExists_SearchUrlIsNull()

    {

        var harness = new Harness { SearchRowCount = 0 };

        var result = await harness.Build().GetContentAsync(1, "en-GB", 10, null, [], CancellationToken.None);

        Assert.That(result.InsightsSearchURL, Is.Null);

    }

    [Test]

    public async Task GetContentAsync_WhenSearchPageUrlCannotBeResolved_SearchUrlIsNull()

    {

        var harness = new Harness();

        harness.UrlRetriever

            .Setup(x => x.Retrieve(500, It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))

            .Returns(Task.FromResult<WebPageUrl>(null!));

        var result = await harness.Build().GetContentAsync(1, "en-GB", 10, null, [], CancellationToken.None);

        Assert.That(result.InsightsSearchURL, Is.Null);

    }

    [Test]

    public void GetContentAsync_WhenChannelNotFound_ThrowsNotFound()

    {

        var harness = new Harness { Channel = null };

        Assert.ThrowsAsync<NotFoundException>(async () =>

            await harness.Build().GetContentAsync(1, "en-GB", 10, null, [], CancellationToken.None));

    }

    [Test]

    public void GetContentAsync_WhenCultureInvalid_ThrowsValidationException()

    {

        var harness = new Harness { Tag = null };

        Assert.ThrowsAsync<ValidationException>(async () =>

            await harness.Build().GetContentAsync(1, "xx", 10, null, [], CancellationToken.None));

    }

    #endregion

    #region Filter validation

    [Test]

    public void GetContentAsync_WithInvalidCategoryCode_ThrowsUnprocessableEntity()

    {

        var harness = new Harness();

        ContentCategoryFilter[] filters = [new() { CategoryCode = "Invalid", SubCategoryIds = Ids1 }];

        Assert.ThrowsAsync<UnprocessableEntityException>(async () =>

            await harness.Build().GetContentAsync(1, "en-GB", 10, null, filters, CancellationToken.None));

        harness.Validation.Verify(

            x => x.AddUnprocessableEntityError(nameof(ContentCategoryFilter.CategoryCode), It.Is<string>(m => m.Contains("Not valid category code"))),

            Times.Once);

    }

    [Test]

    public void GetContentAsync_WithNullCategoryCode_ThrowsUnprocessableEntity()

    {

        var harness = new Harness();

        ContentCategoryFilter[] filters = [new() { CategoryCode = null, SubCategoryIds = Ids1 }];

        Assert.ThrowsAsync<UnprocessableEntityException>(async () =>

            await harness.Build().GetContentAsync(1, "en-GB", 10, null, filters, CancellationToken.None));

        harness.Validation.Verify(

            x => x.AddUnprocessableEntityError(nameof(ContentCategoryFilter.CategoryCode), It.Is<string>(m => m.Contains("null"))),

            Times.Once);

    }

    [Test]

    public void GetContentAsync_WithEmptySubCategoryIds_ThrowsUnprocessableEntity()

    {

        var harness = new Harness();

        ContentCategoryFilter[] filters = [new() { CategoryCode = "Industry", SubCategoryIds = Array.Empty<int>() }];

        Assert.ThrowsAsync<UnprocessableEntityException>(async () =>

            await harness.Build().GetContentAsync(1, "en-GB", 10, null, filters, CancellationToken.None));

        harness.Validation.Verify(

            x => x.AddUnprocessableEntityError(nameof(ContentCategoryFilter.SubCategoryIds), It.Is<string>(m => m.Contains("empty"))),

            Times.Once);

    }

    [Test]

    public void GetContentAsync_WithNullSubCategoryIds_ThrowsUnprocessableEntity()

    {

        var harness = new Harness();

        ContentCategoryFilter[] filters = [new() { CategoryCode = "Industry", SubCategoryIds = null }];

        Assert.ThrowsAsync<UnprocessableEntityException>(async () =>

            await harness.Build().GetContentAsync(1, "en-GB", 10, null, filters, CancellationToken.None));

    }

    [Test]

    public async Task GetContentAsync_WithManySubCategoryIds_AllowsFilter()

    {

        var harness = new Harness();

        harness.BlogPosts.Add(NewBlog(101, new DateTime(2025, 1, 2)));

        ContentCategoryFilter[] filters = [new() { CategoryCode = "Industry", SubCategoryIds = Enumerable.Range(1, 51).ToArray() }];

        var result = await harness.Build().GetContentAsync(1, "en-GB", 10, null, filters, CancellationToken.None);

        Assert.Multiple(() =>

        {

            Assert.That(result.Insights, Has.Count.EqualTo(1));

            harness.Validation.Verify(

                x => x.AddUnprocessableEntityError(nameof(ContentCategoryFilter.SubCategoryIds), It.IsAny<string>()),

                Times.Never);

        });

    }

    [Test]

    public void GetContentAsync_WithBothErrorTypes_PrefersValidationException()

    {

        var harness = new Harness();

        harness.Validation.Setup(x => x.HasErrors()).Returns(true);

        harness.Validation.Setup(x => x.HasUnprocessableEntityErrors()).Returns(true);

        Assert.ThrowsAsync<ValidationException>(async () =>

            await harness.Build().GetContentAsync(1, "en-GB", 10, null, [], CancellationToken.None));

    }

    #endregion

    #region Private static helpers

    [TestCase(null, 15)]

    [TestCase(0, 15)]

    [TestCase(-1, 15)]

    [TestCase(1, 1)]

    [TestCase(50, 50)]

    [TestCase(51, 51)]

    [TestCase(1000, 1000)]

    public void NormalizeInsightCount_AppliesDefaultOnly(int? input, int expected)

    {

        var result = InvokeStatic<int>("NormalizeInsightCount", input);

        Assert.That(result, Is.EqualTo(expected));

    }

    [Test]

    public void BuildSubCategoryIdsByCategoryCode_FillsMissingCodesWithEmptyArrays()

    {

        ContentCategoryFilter[] filters =

        [

            new() { CategoryCode = "BusinessIssue", SubCategoryIds = Ids8And9 },

            new() { CategoryCode = null, SubCategoryIds = Ids99 },

        ];

        var map = InvokeStatic<Dictionary<string, int[]>>("BuildSubCategoryIdsByCategoryCode", (object)filters);

        Assert.Multiple(() =>

        {

            Assert.That(map["BusinessIssue"], Is.EqualTo(Ids8And9));

            Assert.That(map["BusinessLine"], Is.Empty);

            Assert.That(map["ServiceArea"], Is.Empty);

            Assert.That(map["Industry"], Is.Empty);

        });

    }

    [Test]

    public void BuildSubCategoryIdsByCategoryCode_WithRepeatedCategory_UsesLastValue()

    {

        ContentCategoryFilter[] filters =

        [

            new() { CategoryCode = "Industry", SubCategoryIds = Ids1 },

            new() { CategoryCode = "Industry", SubCategoryIds = Ids2And3 },

        ];

        var map = InvokeStatic<Dictionary<string, int[]>>("BuildSubCategoryIdsByCategoryCode", (object)filters);

        Assert.That(map["Industry"], Is.EqualTo(Ids2And3));

    }

    [Test]

    public void BuildSubCategoryIdsByCategoryCode_IgnoresNullSubCategoryIds()

    {

        ContentCategoryFilter[] filters = [new() { CategoryCode = "ServiceArea", SubCategoryIds = null }];

        var map = InvokeStatic<Dictionary<string, int[]>>("BuildSubCategoryIdsByCategoryCode", (object)filters);

        Assert.That(map["ServiceArea"], Is.Empty);

    }

    [Test]

    public void GetLinkingFields_ReturnsOnlyCategoriesWithIds()

    {

        var map = new Dictionary<string, int[]>

        {

            ["BusinessIssue"] = Ids1,

            ["BusinessLine"] = Array.Empty<int>(),

            ["ServiceArea"] = Ids3And4,

            ["Industry"] = Array.Empty<int>(),

        };

        var fields = InvokeStatic<List<(string FieldName, int[] Ids)>>("GetLinkingFields", map);

        Assert.Multiple(() =>

        {

            Assert.That(fields, Has.Count.EqualTo(2));

            Assert.That(fields[0].FieldName, Is.EqualTo(nameof(BlogPost.MetadataBusinessIssues)));

            Assert.That(fields[0].Ids, Is.EqualTo(Ids1));

            Assert.That(fields[1].FieldName, Is.EqualTo(nameof(BlogPost.MetadataServiceAreas)));

            Assert.That(fields[1].Ids, Is.EqualTo(Ids3And4));

        });

    }

    [Test]

    public void GetLinkingFields_WhenAllCategoriesPresent_ReturnsAllFour()

    {

        var map = new Dictionary<string, int[]>

        {

            ["BusinessIssue"] = Ids1,

            ["BusinessLine"] = Ids2And3,

            ["ServiceArea"] = Ids3And4,

            ["Industry"] = Ids10,

        };

        var fields = InvokeStatic<List<(string FieldName, int[] Ids)>>("GetLinkingFields", map);

        Assert.That(fields.Select(x => x.FieldName), Is.EqualTo(ExpectedLinkingFieldNames));

    }

    [Test]

    public void GetCategoryOrderColumn_ForAllowedColumn_ReturnsAscendingColumn()

    {

        var column = InvokeStatic<OrderByColumn>("GetCategoryOrderColumn", "MetadataTitle", LanguageTag.GB);

        Assert.Multiple(() =>

        {

            Assert.That(column.Direction, Is.EqualTo(OrderDirection.Ascending));

            Assert.That(column.GetExpression(), Does.Contain("MetadataTitle"));

        });

    }

    [Test]

    public void GetCategoryOrderColumn_ForNameColumn_IsAllowed()

    {

        var column = InvokeStatic<OrderByColumn>("GetCategoryOrderColumn", nameof(Businessissue.Name), LanguageTag.GB);

        Assert.That(column.GetExpression(), Does.Contain(nameof(Businessissue.Name)));

    }

    [Test]

    public void GetCategoryOrderColumn_ForDisallowedColumn_ThrowsArgumentException()

    {

        Assert.Throws<ArgumentException>(() =>

            InvokeStatic<OrderByColumn>("GetCategoryOrderColumn", "Name; DROP TABLE x", LanguageTag.GB));

    }

    [TestCase(null)]

    [TestCase("")]

    [TestCase("   ")]

    [TestCase("javascript:alert(1)")]

    [TestCase("ftp://cdn.example/image.png")]

    public void GetAbsoluteImageUrl_WithUnusableImage_ReturnsEmpty(string? imageUrl)

    {

        var result = InvokeStatic<string>("GetAbsoluteImageUrl", imageUrl, PageBase + "/en/blog");

        Assert.That(result, Is.EqualTo(string.Empty));

    }

    [Test]

    public void GetAbsoluteImageUrl_WithRelativeImageAndNoPageUrl_ReturnsEmpty()

    {

        var result = InvokeStatic<string>("GetAbsoluteImageUrl", "/media/image.jpg", string.Empty);

        Assert.That(result, Is.EqualTo(string.Empty));

    }

    [TestCase("/media/image.jpg", PageBase + "/en/blog", PageBase + "/media/image.jpg")]

    [TestCase("~/media/image.jpg", PageBase + "/en/blog", PageBase + "/media/image.jpg")]

    [TestCase("media/image.jpg", PageBase + "/blog", PageBase + "/media/image.jpg")]

    [TestCase("https://cdn.example/image.jpg?width=640", PageBase + "/en/blog", "https://cdn.example/image.jpg")]

    [TestCase("//cdn.example/image.jpg", PageBase + "/en/blog", "https://cdn.example/image.jpg")]

    [TestCase("//cdn.example/image.jpg", "http://public.example/en/blog", "http://cdn.example/image.jpg")]

    [TestCase("//cdn.example/image.jpg", "", "https://cdn.example/image.jpg")]

    public void GetAbsoluteImageUrl_ResolvesUrls(string imageUrl, string pageUrl, string expectedPrefix)

    {

        var result = InvokeStatic<string>("GetAbsoluteImageUrl", imageUrl, pageUrl);

        Assert.That(result, Does.StartWith(expectedPrefix));

    }

    [Test]

    public void ToHtmlCompatibleUrl_WithNull_ReturnsEmpty()

    {

        var result = InvokeStatic<string>("ToHtmlCompatibleUrl", (object?)null);

        Assert.That(result, Is.EqualTo(string.Empty));

    }

    [Test]

    public void ToHtmlCompatibleUrl_WithAbsoluteUrl_ReturnsUrl()

    {

        var result = InvokeStatic<string>("ToHtmlCompatibleUrl", "https://example.com/page");

        Assert.That(result, Does.Contain("example.com/page"));

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

            SubCategories = new List<ContentSubCategoryDto> { new() { Id = 7, Name = "Healthcare" } },

        };

        Assert.Multiple(() =>

        {

            Assert.That(dto.Name, Is.EqualTo("Industry"));

            Assert.That(dto.Code, Is.EqualTo("Industry"));

            Assert.That(dto.SubCategories, Has.Count.EqualTo(1));

            Assert.That(dto.SubCategories[0].Id, Is.EqualTo(7));

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

    public void ContentDto_StoresValues()

    {

        var dto = new ContentDto { InsightsSearchURL = "https://example.com/search", Insights = new List<InsightContentDto>() };

        Assert.Multiple(() =>

        {

            Assert.That(dto.InsightsSearchURL, Is.EqualTo("https://example.com/search"));

            Assert.That(dto.Insights, Is.Empty);

        });

    }

    #endregion

    #region Test infrastructure

    private static BlogPost NewBlog(int id, DateTime? published) => new()

    {

        SystemFields = new WebPageFields { WebPageItemID = id, ContentItemCommonDataFirstPublishedWhen = published },

        MetadataTitle = "Blog title",

        MetadataDescription = "<p>Blog &amp; summary</p>",

    };

    private static Insight NewInsight(int id, DateTime? published) => new()

    {

        SystemFields = new WebPageFields { WebPageItemID = id, ContentItemCommonDataFirstPublishedWhen = published },

        MetadataTitle = "Insight title",

        MetadataDescription = "<p>Insight summary</p>",

    };

    private static T InvokeStatic<T>(string methodName, params object?[] args)

    {

        var method = typeof(ContentCategoryService).GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic)!;

        try

        {

            return (T)method.Invoke(null, args)!;

        }

        catch (TargetInvocationException ex) when (ex.InnerException is not null)

        {

            ExceptionDispatchInfo.Capture(ex.InnerException).Throw();

            throw;

        }

    }

    private static FakeChannelInfo CreateChannelInfo(string channelName)

    {

        var channel = (FakeChannelInfo)RuntimeHelpers.GetUninitializedObject(typeof(FakeChannelInfo));

        channel.ChannelName = channelName;

        return channel;

    }

    private static object? DefaultReturn(Type returnType)

    {

        if (returnType == typeof(Task))

        {

            return Task.CompletedTask;

        }

        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))

        {

            var payload = returnType.GetGenericArguments()[0];

            var value = payload.IsValueType ? Activator.CreateInstance(payload) : null;

            return typeof(Task).GetMethod(nameof(Task.FromResult))!.MakeGenericMethod(payload).Invoke(null, [value]);

        }

        return returnType != typeof(void) && returnType.IsValueType ? Activator.CreateInstance(returnType) : null;

    }

    private sealed class Harness

    {

        public Mock<IMappingHelper> Mapping { get; } = new();

        public Mock<IInfoProvider<ChannelInfo>> Channels { get; } = new();

        public Mock<IInfoProvider<WebsiteChannelInfo>> WebsiteChannels { get; } = new();

        public Mock<IInfoProvider<ContentLanguageInfo>> Languages { get; } = new();

        public Mock<IValidationState> Validation { get; } = new();

        public Mock<IWebPageUrlRetriever> UrlRetriever { get; } = new();

        public Dictionary<string, string?> Config { get; } = new();

        public List<BlogPost> BlogPosts { get; } = new();

        public List<Insight> Insights { get; } = new();

        public ChannelInfo? Channel { get; set; } = CreateChannelInfo("Global");

        public LanguageTag? Tag { get; set; } = LanguageTag.GB;

        public int SearchRowCount { get; set; } = 1;

        public CacheProxy CacheProxy { get; private set; } = null!;

        public Harness()

        {

            Mapping

                .Setup(x => x.ValidateAndNormalizeSiteCultureAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))

                .Returns(() => Task.FromResult<LanguageTag?>(Tag)!);

            // GetAsync(id) used by the service is an extension method (cannot be mocked directly);

            // it resolves through IInfoByIdProvider, so mock both the sync and async members.

            var channelById = Channels.As<IInfoByIdProvider<ChannelInfo>>();

            channelById.Setup(x => x.Get(It.IsAny<int>())).Returns(() => Channel!);

            channelById.Setup(x => x.GetAsync(It.IsAny<int>(), It.IsAny<CancellationToken?>()))

                .Returns(() => Task.FromResult(Channel!));

            UrlRetriever

                .Setup(x => x.Retrieve(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))

                .Returns((int id, string culture, bool includeDefault, CancellationToken token) =>

                    Task.FromResult(new WebPageUrl($"~/p/{id}", $"{PageBase}/p/{id}")));

            // Stateful validation state so that errors added by the service are observed by the service.

            var errors = new Dictionary<string, string>();

            var hasErrors = false;

            var hasUnprocessable = false;

            Validation.Setup(x => x.AddError(It.IsAny<string>(), It.IsAny<string>()))

                .Callback<string, string>((key, message) => { hasErrors = true; errors[key] = message; });

            Validation.Setup(x => x.AddUnprocessableEntityError(It.IsAny<string>(), It.IsAny<string>()))

                .Callback<string, string>((key, message) => { hasUnprocessable = true; errors[key] = message; });

            Validation.Setup(x => x.HasErrors()).Returns(() => hasErrors);

            Validation.Setup(x => x.HasUnprocessableEntityErrors()).Returns(() => hasUnprocessable);

            Validation.Setup(x => x.GetErrors()).Returns(() => errors);

        }

        public TestableContentCategoryService Build()

        {

            var executor = DispatchProxy.Create<IContentQueryExecutor, ExecutorProxy>();

            ((ExecutorProxy)(object)executor).Handler = HandleExecutor;

            var cache = DispatchProxy.Create<IProgressiveCache, CacheProxy>();

            CacheProxy = (CacheProxy)(object)cache;

            var configuration = new ConfigurationBuilder().AddInMemoryCollection(Config).Build();

            return new TestableContentCategoryService(

                GetPages,

                executor,

                Mapping.Object,

                Channels.Object,

                WebsiteChannels.Object,

                Languages.Object,

                Validation.Object,

                UrlRetriever.Object,

                cache,

                configuration,

                Mock.Of<ILogger<ContentCategoryService>>());

        }

        private IEnumerable<object> GetPages(Type pageType)

        {

            if (pageType == typeof(BlogPost))

            {

                return BlogPosts;

            }

            if (pageType == typeof(Insight))

            {

                return Insights;

            }

            return Enumerable.Empty<object>();

        }

        private object? HandleExecutor(MethodInfo method, object?[]? args)

        {

            if (method.Name != "GetResult"

                || !method.ReturnType.IsGenericType

                || !method.ReturnType.GetGenericArguments()[0].IsGenericType)

            {

                return DefaultReturn(method.ReturnType);

            }

            var payloadType = method.ReturnType.GetGenericArguments()[0];

            var elementType = payloadType.GetGenericArguments()[0];

            var selector = args!.OfType<Delegate>().First();

            var count = elementType == typeof(int) ? SearchRowCount : 1;

            var items = new List<object>();

            for (var i = 0; i < count; i++)

            {

                items.Add(selector.DynamicInvoke(CreateRow())!);

            }

            var array = Array.CreateInstance(elementType, items.Count);

            for (var i = 0; i < items.Count; i++)

            {

                array.SetValue(items[i], i);

            }

            return typeof(Task).GetMethod(nameof(Task.FromResult))!

                .MakeGenericMethod(payloadType)

                .Invoke(null, [array]);

        }

        private static IContentQueryDataContainer CreateRow()

        {

            var row = new Mock<IContentQueryDataContainer>();

            row.Setup(x => x.GetValue<int>(It.IsAny<string>()))

                .Returns((string key) => key == nameof(WebPageFields.WebPageItemID) ? 500 : 88);

            row.Setup(x => x.GetValue<string>(It.IsAny<string>())).Returns("Tax");

            return row.Object;

        }

    }

    /// <summary>

    /// Overrides the page-mapping seam, because the GetMappedResult extension requires the real

    /// Xperience executor (with its internal model mapper) and cannot run against a fake.

    /// </summary>

    private sealed class TestableContentCategoryService : ContentCategoryService

    {

        private readonly Func<Type, IEnumerable<object>> pagesByType;

        public TestableContentCategoryService(

            Func<Type, IEnumerable<object>> pagesByType,

            IContentQueryExecutor contentQueryExecutor,

            IMappingHelper mappingHelper,

            IInfoProvider<ChannelInfo> channelInfoProvider,

            IInfoProvider<WebsiteChannelInfo> websiteChannelInfoProvider,

            IInfoProvider<ContentLanguageInfo> contentLanguageInfoProvider,

            IValidationState validationState,

            IWebPageUrlRetriever webPageUrlRetriever,

            IProgressiveCache cache,

            IConfiguration configuration,

            ILogger<ContentCategoryService> logger)

            : base(contentQueryExecutor, mappingHelper, channelInfoProvider, websiteChannelInfoProvider,

                contentLanguageInfoProvider, validationState, webPageUrlRetriever, cache, configuration, logger)

        {

            this.pagesByType = pagesByType;

        }

        protected override Task<IEnumerable<TPage>> GetMappedPagesAsync<TPage>(

            ContentItemQueryBuilder builder, CancellationToken cancellationToken) =>

            Task.FromResult(pagesByType(typeof(TPage)).Cast<TPage>().ToList().AsEnumerable());

    }

    public class ExecutorProxy : DispatchProxy

    {

        public Func<MethodInfo, object?[]?, object?>? Handler { get; set; }

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args) =>

            targetMethod is null ? null : Handler?.Invoke(targetMethod, args) ?? DefaultReturn(targetMethod.ReturnType);

    }

    public class CacheProxy : DispatchProxy

    {

        public int LoadCount { get; private set; }

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)

        {

            if (targetMethod is null)

            {

                return null;

            }

            if (targetMethod.Name is "LoadAsync" or "Load")

            {

                LoadCount++;

                var loader = args!.OfType<Delegate>().First();

                var settings = args!.OfType<CacheSettings>().FirstOrDefault();

                return loader.DynamicInvoke(settings);

            }

            return DefaultReturn(targetMethod.ReturnType);

        }

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

    #endregion

}
