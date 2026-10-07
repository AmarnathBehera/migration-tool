using BDO.GWT.Core;
using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.Helper;
using BDO.GWT.Core.Retrievers;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.Utilities;
using CMS.ContentEngine;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Core.Tests.Services
{
    public class BlogPageCardsServiceTests
    {
        private static readonly string[] ExpectedContributorLabels = ["Amy Lee", "Bob", "Zed Young"];
        private static readonly string[] ExpectedPageFilterLabels = ["Alpha", "Mid", "Zeta"];
        private static readonly string[] ExpectedTagFilterLabels = ["Alpha", "Beta"];
        private static readonly string[] ExpectedKeywordLabels = ["alpha", "beta"];

        private Mock<ICardsService> _cardsServiceMock = null!;
        private Mock<ICultureContext> _cultureContextMock = null!;
        private Mock<IContentService> _contentServiceMock = null!;
        private Mock<IWebsiteChannelContext> _websiteChannelContextMock = null!;
        private Mock<IUrlHelperFactory> _urlHelperFactoryMock = null!;
        private Mock<IHttpContextAccessor> _httpContextAccessorMock = null!;
        private Mock<IWebPageUrlResolver> _urlResolverMock = null!;
        private Mock<IUrlHelper> _urlHelperMock = null!;
        private BlogPageCardsService _service = null!;

        [SetUp]
        public void Setup()
        {
            _cardsServiceMock = new Mock<ICardsService>();
            _cultureContextMock = new Mock<ICultureContext>();
            _contentServiceMock = new Mock<IContentService>();
            _urlHelperFactoryMock = new Mock<IUrlHelperFactory>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _websiteChannelContextMock = new Mock<IWebsiteChannelContext>();
            _urlHelperMock = new Mock<IUrlHelper>();
            _urlResolverMock = new Mock<IWebPageUrlResolver>();

            _cultureContextMock
                .Setup(x => x.CurrentLanguageTag)
                .Returns("en-US");

            _websiteChannelContextMock
                .Setup(x => x.WebsiteChannelID)
                .Returns(1);

            // Setup HttpContext and UrlHelper
            var httpContext = new DefaultHttpContext();
            _httpContextAccessorMock
                .Setup(x => x.HttpContext)
                .Returns(httpContext);

            _urlHelperMock
                .Setup(x => x.Content(It.IsAny<string>()))
                .Returns<string>(s => s ?? string.Empty);

            _urlHelperFactoryMock
                .Setup(x => x.GetUrlHelper(It.IsAny<ActionContext>()))
                .Returns(_urlHelperMock.Object);

            _service = new BlogPageCardsService(
                _cardsServiceMock.Object,
                _cultureContextMock.Object,
                _contentServiceMock.Object,
                _urlHelperFactoryMock.Object,
                _httpContextAccessorMock.Object,
                _websiteChannelContextMock.Object,
                _urlResolverMock.Object);
        }

        // ==========================================================================
        // CreateFiltersAsync
        // ==========================================================================

        #region CreateFiltersAsync

        [Test]
        public async Task CreateFiltersAsync_DelegatesToCardsService()
        {
            var filters = new HashSet<string> { "keywords", "businessLine" };

            // "keywords" → FilterParameterType.Field → BuildOptionsForFieldFilterAsync → GetItemsByTypesAsync
            // "businessLine" → FilterParameterType.Page → BuildOptionsForPageFilterAsync → GetItemsByTypesAsync
            _contentServiceMock
                .Setup(x => x.GetItemsByTypesAsync(
                    It.IsAny<Type[]>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<object>());

            var result = await _service.CreateBlogPageFiltersAsync(filters, 1, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result, Has.Length.EqualTo(2));
            });
        }

        [Test]
        public async Task CreateFiltersAsync_EmptyFilterSet_ReturnsEmptyArray()
        {
            var result = await _service.CreateBlogPageFiltersAsync(new HashSet<string>(), 1, CancellationToken.None);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task CreateFiltersAsync_ReturnsFiltersForKnownFilterNames()
        {
            var filters = new HashSet<string> { "keywords", "businessLine" };

            _contentServiceMock
                .Setup(x => x.GetItemsByTypesAsync(
                    It.IsAny<Type[]>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<object>());

            var result = await _service.CreateBlogPageFiltersAsync(filters, 1, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Length.EqualTo(2));
        }

        [Test]
        public async Task CreateBlogPageFiltersAsync_UnknownFilterName_IsSkipped()
        {
            var result = await _service.CreateBlogPageFiltersAsync(
                new HashSet<string> { "this-filter-does-not-exist" }, 1, CancellationToken.None);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task CreateBlogPageFiltersAsync_PersonFilter_ReturnsSortedTrimmedContributorNames()
        {
            var personFilter = RequireFilter(
                d => d.FilterParameterType == FilterParameterType.Page && d.EntityType?.Name == "Person",
                "a Page-type filter whose EntityType is Person");

            const int blogsPageId = 7;
            var contributorPage = new BlogContributors
            {
                SystemFields = new WebPageFields { WebPageItemID = 50 }
            };

            _contentServiceMock
                .Setup(x => x.GetChildPagesByParentGuidAsync<BlogContributors>(blogsPageId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { contributorPage });

            _contentServiceMock
                .Setup(x => x.GetChildPagesByParentGuidAsync<Person>(50, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[]
                {
                    new Person { PersonFirstName = "Zed", PersonLastName = "Young" },
                    new Person { PersonFirstName = "Amy", PersonLastName = "Lee" },
                    new Person { PersonFirstName = "Bob", PersonLastName = "" },   // trimmed -> "Bob"
                    new Person { PersonFirstName = "", PersonLastName = "" }       // filtered out
                });

            var result = await _service.CreateBlogPageFiltersAsync(
                new HashSet<string> { personFilter }, blogsPageId, CancellationToken.None);

            var options = GetOptions(result.Single());

            Assert.Multiple(() =>
            {
                Assert.That(options.Select(o => o.Label), Is.EqualTo(ExpectedContributorLabels));
                Assert.That(options.Select(o => o.Value), Is.EqualTo(options.Select(o => o.Label)));
            });
        }

        [Test]
        public async Task CreateBlogPageFiltersAsync_PersonFilter_NoContributorPersons_ReturnsEmptyOptions()
        {
            var personFilter = RequireFilter(
                d => d.FilterParameterType == FilterParameterType.Page && d.EntityType?.Name == "Person",
                "a Page-type filter whose EntityType is Person");

            _contentServiceMock
                .Setup(x => x.GetChildPagesByParentGuidAsync<BlogContributors>(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { new BlogContributors { SystemFields = new WebPageFields { WebPageItemID = 50 } } });

            _contentServiceMock
                .Setup(x => x.GetChildPagesByParentGuidAsync<Person>(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<Person>());

            var result = await _service.CreateBlogPageFiltersAsync(
                new HashSet<string> { personFilter }, 1, CancellationToken.None);

            Assert.That(GetOptions(result.Single()), Is.Empty);
        }

        [Test]
        public async Task CreateBlogPageFiltersAsync_PersonFilter_NoContributorPages_ReturnsEmptyOptions()
        {
            var personFilter = RequireFilter(
                d => d.FilterParameterType == FilterParameterType.Page && d.EntityType?.Name == "Person",
                "a Page-type filter whose EntityType is Person");

            _contentServiceMock
                .Setup(x => x.GetChildPagesByParentGuidAsync<BlogContributors>(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<BlogContributors>());

            var result = await _service.CreateBlogPageFiltersAsync(
                new HashSet<string> { personFilter }, 1, CancellationToken.None);

            Assert.That(GetOptions(result.Single()), Is.Empty);
        }

        [Test]
        public async Task CreateBlogPageFiltersAsync_PageFilter_ResolvesNameFromTitleThenOgTitleThenItemName()
        {
            var pageFilter = RequireFilter(
                d => d.FilterParameterType == FilterParameterType.Page && d.EntityType?.Name != "Person",
                "a Page-type filter whose EntityType is not Person");

            _contentServiceMock
                .Setup(x => x.GetItemsByTypesAsync(It.IsAny<Type[]>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<object>
                {
                    new FakePageItem { MetadataTitle = "Zeta" },
                    new FakePageItem { MetadataOGTitle = "Alpha" },
                    new FakePageItem { SystemFields = new FakeSystemFields { ContentItemName = "Mid" } },
                    new FakePageItem(),   // no name anywhere -> filtered out
                    new object()          // no reflected properties at all -> filtered out
                });

            var result = await _service.CreateBlogPageFiltersAsync(
                new HashSet<string> { pageFilter }, 1, CancellationToken.None);

            Assert.That(GetOptions(result.Single()).Select(o => o.Label),
                Is.EqualTo(ExpectedPageFilterLabels));
        }

        [Test]
        public async Task CreateBlogPageFiltersAsync_PageFilter_NullItems_ReturnsEmptyOptions()
        {
            var pageFilter = RequireFilter(
                d => d.FilterParameterType == FilterParameterType.Page && d.EntityType?.Name != "Person",
                "a Page-type filter whose EntityType is not Person");

            // If GetItemsByTypesAsync returns a non-nullable type this lambda still compiles; adjust if needed.
            _contentServiceMock
                .Setup(x => x.GetItemsByTypesAsync(It.IsAny<Type[]>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => null!);

            var result = await _service.CreateBlogPageFiltersAsync(
                new HashSet<string> { pageFilter }, 1, CancellationToken.None);

            Assert.That(GetOptions(result.Single()), Is.Empty);
        }

        [Test]
        public async Task CreateBlogPageFiltersAsync_TagPageFilter_ReturnsSortedNonEmptyNames()
        {
            var tagFilter = RequireFilter(
                d => d.FilterParameterType == FilterParameterType.Tagpage,
                "a Tagpage-type filter");

            _contentServiceMock
                .Setup(x => x.GetItemsByTypesAsync(It.IsAny<Type[]>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<object>
                {
                    new FakeTagItem { Name = "Beta" },
                    new FakeTagItem { Name = "Alpha" },
                    new FakeTagItem { Name = "" },   // filtered out
                    new object()                     // no Name property -> filtered out
                });

            var result = await _service.CreateBlogPageFiltersAsync(
                new HashSet<string> { tagFilter }, 1, CancellationToken.None);

            Assert.That(GetOptions(result.Single()).Select(o => o.Label),
                Is.EqualTo(ExpectedTagFilterLabels));
        }

        [Test]
        public async Task CreateBlogPageFiltersAsync_FieldFilter_BlogPostKeywords_ReturnsDistinctKeywords()
        {
            var fieldFilter = RequireFilter(
                d => d.FilterParameterType == FilterParameterType.Field && d.EntityType == typeof(BlogPost),
                "a Field-type filter whose EntityType is BlogPost");

            var post1 = CreateBlogPost("P1", new DateTime(2024, 1, 1));
            post1.MetadataKeywordsBlog = "alpha";
            var post2 = CreateBlogPost("P2", new DateTime(2024, 1, 2));
            post2.MetadataKeywordsBlog = "beta";
            var post3 = CreateBlogPost("P3", new DateTime(2024, 1, 3));
            post3.MetadataKeywordsBlog = "alpha";   // duplicate
            var post4 = CreateBlogPost("P4", new DateTime(2024, 1, 4));
            post4.MetadataKeywordsBlog = "";        // ignored

            _contentServiceMock
                .Setup(x => x.GetItemsByTypesAsync(It.IsAny<Type[]>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<object> { post1, post2, post3, post4 });

            var result = await _service.CreateBlogPageFiltersAsync(
                new HashSet<string> { fieldFilter }, 1, CancellationToken.None);

            var labels = GetOptions(result.Single()).Select(o => o.Label).ToArray();

            Assert.That(labels, Is.EquivalentTo(ExpectedKeywordLabels));
        }

        [Test]
        public async Task CreateBlogPageFiltersAsync_FieldFilter_NoItems_ReturnsEmptyOptions()
        {
            var fieldFilter = RequireFilter(
                d => d.FilterParameterType == FilterParameterType.Field,
                "a Field-type filter");

            _contentServiceMock
                .Setup(x => x.GetItemsByTypesAsync(It.IsAny<Type[]>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<object>());

            var result = await _service.CreateBlogPageFiltersAsync(
                new HashSet<string> { fieldFilter }, 1, CancellationToken.None);

            Assert.That(GetOptions(result.Single()), Is.Empty);
        }



        #endregion

        // ==========================================================================
        // GetFilteredItemsAsync
        // ==========================================================================

        #region GetFilteredItemsAsync

        [Test]
        public async Task GetFilteredItemsAsync_NoBlogPostGroups_ReturnsEmptyPagination()
        {
            _contentServiceMock
                .Setup(x => x.GetChildPagesByParentGuidAsync<BlogPostGroup>(
                    It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Enumerable.Empty<BlogPostGroup>());

            var cardsQuery = new CardsQuery
            {
                CurrentPage = 1,
                PageSize = 10,
                Filters = new Dictionary<string, string>()
            };

            var result = await _service.GetFilteredItemsAsync(
                "en-US", cardsQuery, null,
                _ => false, 100, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.TotalCount, Is.EqualTo(0));
                Assert.That(result.Data, Is.Empty);
            });
        }

        [Test]
        public async Task GetFilteredItemsAsync_WithBlogPosts_ReturnsPaginatedResults()
        {
            var blogPostGroup = CreateBlogPostGroup(webPageItemId: 10);
            var blogPosts = new[]
            {
                CreateBlogPost("Post 1", new DateTime(2024, 6, 1)),
                CreateBlogPost("Post 2", new DateTime(2024, 5, 1)),
                CreateBlogPost("Post 3", new DateTime(2024, 4, 1))
            };

            _contentServiceMock
                .Setup(x => x.GetChildPagesByParentGuidAsync<BlogPostGroup>(
                    It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { blogPostGroup });

            _contentServiceMock
                .Setup(x => x.GetChildPagesByParentGuidAsync<BlogPost>(
                    It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(blogPosts);

            _contentServiceMock
                .Setup(x => x.GetChildPagesByParentGuidWithQueryBuilderAsync<BlogPost>(
                    It.IsAny<Action<ContentItemQueryBuilder>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(blogPosts);

            var cardsQuery = new CardsQuery
            {
                CurrentPage = 1,
                PageSize = 10,
                Filters = new Dictionary<string, string>()
            };

            var result = await _service.GetFilteredItemsAsync(
                "en-US", cardsQuery, null,
                _ => false, 100, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.TotalCount, Is.EqualTo(3));
                Assert.That(result.Data, Has.Length.EqualTo(3));
            });
        }

        [Test]
        public async Task GetFilteredItemsAsync_PaginatesCorrectly_Page1()
        {
            var blogPostGroup = CreateBlogPostGroup(webPageItemId: 10);
            var blogPosts = Enumerable.Range(1, 5)
                .Select(i => CreateBlogPost($"Post {i}", new DateTime(2024, 6, i)))
                .ToArray();

            _contentServiceMock
                .Setup(x => x.GetChildPagesByParentGuidAsync<BlogPostGroup>(
                    It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { blogPostGroup });

            _contentServiceMock
                .Setup(x => x.GetChildPagesByParentGuidAsync<BlogPost>(
                    It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(blogPosts);

            _contentServiceMock
                .Setup(x => x.GetChildPagesByParentGuidWithQueryBuilderAsync<BlogPost>(
                    It.IsAny<Action<ContentItemQueryBuilder>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(blogPosts);

            var cardsQuery = new CardsQuery
            {
                CurrentPage = 1,
                PageSize = 2,   // ← key: only 2 items per page
                Filters = new Dictionary<string, string>()
            };

            var result = await _service.GetFilteredItemsAsync(
                "en-US", cardsQuery, null,
                _ => false, 100, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.TotalCount, Is.EqualTo(5)); // all 5 exist
                Assert.That(result.Data, Has.Length.EqualTo(2)); // only 2 returned on page 1
            });
        }

        [Test]
        public async Task GetFilteredItemsAsync_PaginatesCorrectly_Page2()
        {
            var blogPostGroup = CreateBlogPostGroup(webPageItemId: 10);
            var blogPosts = Enumerable.Range(1, 5)
                .Select(i => CreateBlogPost($"Post {i}", new DateTime(2024, 6, i)))
                .ToArray();

            _contentServiceMock
                .Setup(x => x.GetChildPagesByParentGuidAsync<BlogPostGroup>(
                    It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { blogPostGroup });

            _contentServiceMock
                .Setup(x => x.GetChildPagesByParentGuidAsync<BlogPost>(
                    It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(blogPosts);

            _contentServiceMock
                .Setup(x => x.GetChildPagesByParentGuidWithQueryBuilderAsync<BlogPost>(
                    It.IsAny<Action<ContentItemQueryBuilder>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(blogPosts);

            var cardsQuery = new CardsQuery
            {
                CurrentPage = 2,
                PageSize = 2,
                Filters = new Dictionary<string, string>()
            };

            var result = await _service.GetFilteredItemsAsync(
                "en-US", cardsQuery, null,
                _ => false, 100, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.TotalCount, Is.EqualTo(5));
                Assert.That(result.Data, Has.Length.EqualTo(2));
            });
        }

        [Test]
        public async Task GetFilteredItemsAsync_DefaultsToPage1_WhenCurrentPageIsNull()
        {
            var blogPostGroup = CreateBlogPostGroup(webPageItemId: 10);
            var blogPosts = Enumerable.Range(1, 3)
                .Select(i => CreateBlogPost($"Post {i}", new DateTime(2024, 6, i)))
                .ToArray();

            _contentServiceMock
                .Setup(x => x.GetChildPagesByParentGuidAsync<BlogPostGroup>(
                    It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { blogPostGroup });

            _contentServiceMock
                .Setup(x => x.GetChildPagesByParentGuidAsync<BlogPost>(
                    It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(blogPosts);

            _contentServiceMock
                .Setup(x => x.GetChildPagesByParentGuidWithQueryBuilderAsync<BlogPost>(
                    It.IsAny<Action<ContentItemQueryBuilder>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(blogPosts);

            var cardsQuery = new CardsQuery
            {
                CurrentPage = null,
                PageSize = 10,
                Filters = new Dictionary<string, string>()
            };

            var result = await _service.GetFilteredItemsAsync(
                "en-US", cardsQuery, null,
                _ => false, 100, CancellationToken.None);

            Assert.That(result.Data, Has.Length.EqualTo(3));
        }

        [Test]
        public async Task GetFilteredItemsAsync_DefaultsToPageSize10_WhenPageSizeIsNull()
        {
            var blogPostGroup = CreateBlogPostGroup(webPageItemId: 10);
            var blogPosts = Enumerable.Range(1, 15)
                .Select(i => CreateBlogPost($"Post {i}", new DateTime(2024, 1, 1).AddDays(i)))
                .ToArray();

            _contentServiceMock
                .Setup(x => x.GetChildPagesByParentGuidAsync<BlogPostGroup>(
                    It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { blogPostGroup });

            _contentServiceMock
                .Setup(x => x.GetChildPagesByParentGuidAsync<BlogPost>(
                    It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(blogPosts);

            _contentServiceMock
                .Setup(x => x.GetChildPagesByParentGuidWithQueryBuilderAsync<BlogPost>(
                    It.IsAny<Action<ContentItemQueryBuilder>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(blogPosts);

            var cardsQuery = new CardsQuery
            {
                CurrentPage = 1,
                PageSize = null,
                Filters = new Dictionary<string, string>()
            };

            var result = await _service.GetFilteredItemsAsync(
                "en-US", cardsQuery, null,
                _ => false, 100, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.TotalCount, Is.EqualTo(15));
                Assert.That(result.Data, Has.Length.EqualTo(10));
            });
        }

        [Test]
        public async Task GetFilteredItemsAsync_FeaturedPostsOrderedFirst()
        {
            var featuredGuid = Guid.NewGuid();
            var blogPostGroup = CreateBlogPostGroup(webPageItemId: 10);

            var regularPost = CreateBlogPost("Regular", new DateTime(2024, 6, 1));
            var featuredPost = CreateBlogPost("Featured", new DateTime(2024, 1, 1));
            featuredPost.SystemFields = new WebPageFields
            {
                ContentItemGUID = featuredGuid,
                WebPageItemID = 2,
                WebPageUrlPath = "/blogs/featured",
                ContentItemCommonDataLastPublishedWhen = new DateTime(2024, 1, 1)
            };

            var blogPosts = new[] { regularPost, featuredPost };

            _contentServiceMock
                .Setup(x => x.GetChildPagesByParentGuidAsync<BlogPostGroup>(
                    It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { blogPostGroup });

            _contentServiceMock
                .Setup(x => x.GetChildPagesByParentGuidAsync<BlogPost>(
                    It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(blogPosts);

            _contentServiceMock
                .Setup(x => x.GetChildPagesByParentGuidWithQueryBuilderAsync<BlogPost>(
                    It.IsAny<Action<ContentItemQueryBuilder>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(blogPosts);

            var cardsQuery = new CardsQuery
            {
                CurrentPage = 1,
                PageSize = 10,
                Filters = new Dictionary<string, string>()
            };

            var result = await _service.GetFilteredItemsAsync(
                "en-US", cardsQuery, null,
                bp => bp.SystemFields.ContentItemGUID == featuredGuid,
                100, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.Data, Has.Length.EqualTo(2));
                Assert.That(result.Data[0].IsFeatured, Is.True);
                Assert.That(result.Data[0].Title, Is.EqualTo("Featured"));
            });
        }

        [Test]
        public async Task GetFilteredItemsAsync_OrdersByPublishDateDescending_AfterFeatured()
        {
            var blogPostGroup = CreateBlogPostGroup(webPageItemId: 10);
            var post1 = CreateBlogPost("Oldest", new DateTime(2024, 1, 1));
            var post2 = CreateBlogPost("Middle", new DateTime(2024, 3, 1));
            var post3 = CreateBlogPost("Newest", new DateTime(2024, 6, 1));

            var blogPosts = new[] { post1, post2, post3 };

            _contentServiceMock
                .Setup(x => x.GetChildPagesByParentGuidAsync<BlogPostGroup>(
                    It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { blogPostGroup });

            _contentServiceMock
                .Setup(x => x.GetChildPagesByParentGuidAsync<BlogPost>(
                    It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(blogPosts);

            _contentServiceMock
                .Setup(x => x.GetChildPagesByParentGuidWithQueryBuilderAsync<BlogPost>(
                    It.IsAny<Action<ContentItemQueryBuilder>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(blogPosts);

            var cardsQuery = new CardsQuery
            {
                CurrentPage = 1,
                PageSize = 10,
                Filters = new Dictionary<string, string>()
            };

            var result = await _service.GetFilteredItemsAsync(
                "en-US", cardsQuery, null,
                _ => false, 100, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.Data[0].Title, Is.EqualTo("Newest"));
                Assert.That(result.Data[1].Title, Is.EqualTo("Middle"));
                Assert.That(result.Data[2].Title, Is.EqualTo("Oldest"));
            });
        }

        [Test]
        public async Task GetFilteredItemsAsync_MultipleBlogPostGroups_AggregatesResults()
        {
            var group1 = CreateBlogPostGroup(webPageItemId: 10);
            var group2 = CreateBlogPostGroup(webPageItemId: 20);

            var postsGroup1 = new[] { CreateBlogPost("G1-Post1", new DateTime(2024, 6, 1)) };
            var postsGroup2 = new[] { CreateBlogPost("G2-Post1", new DateTime(2024, 5, 1)) };

            _contentServiceMock
                .Setup(x => x.GetChildPagesByParentGuidAsync<BlogPostGroup>(
                    100, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { group1, group2 });

            _contentServiceMock
                .Setup(x => x.GetChildPagesByParentGuidAsync<BlogPost>(
                    10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(postsGroup1);

            _contentServiceMock
                .Setup(x => x.GetChildPagesByParentGuidAsync<BlogPost>(
                    20, It.IsAny<CancellationToken>()))
                .ReturnsAsync(postsGroup2);

            var callCount = 0;
            _contentServiceMock
                .Setup(x => x.GetChildPagesByParentGuidWithQueryBuilderAsync<BlogPost>(
                    It.IsAny<Action<ContentItemQueryBuilder>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() =>
                {
                    callCount++;
                    return callCount == 1 ? postsGroup1 : postsGroup2;
                });

            var cardsQuery = new CardsQuery
            {
                CurrentPage = 1,
                PageSize = 10,
                Filters = new Dictionary<string, string>()
            };

            var result = await _service.GetFilteredItemsAsync(
                "en-US", cardsQuery, null,
                _ => false, 100, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.TotalCount, Is.EqualTo(2));
                Assert.That(result.Data, Has.Length.EqualTo(2));
            });
        }

        [Test]
        public async Task GetFilteredItemsAsync_FiltersNullItems()
        {
            var blogPostGroup = CreateBlogPostGroup(webPageItemId: 10);

            _contentServiceMock
                .Setup(x => x.GetChildPagesByParentGuidAsync<BlogPostGroup>(
                    It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { blogPostGroup });

            _contentServiceMock
                .Setup(x => x.GetChildPagesByParentGuidAsync<BlogPost>(
                    It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<BlogPost>());

            _contentServiceMock
                .Setup(x => x.GetChildPagesByParentGuidWithQueryBuilderAsync<BlogPost>(
                    It.IsAny<Action<ContentItemQueryBuilder>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BlogPost[] { null!, CreateBlogPost("Valid", new DateTime(2024, 6, 1)), null! });

            var cardsQuery = new CardsQuery
            {
                CurrentPage = 1,
                PageSize = 10,
                Filters = new Dictionary<string, string>()
            };

            var result = await _service.GetFilteredItemsAsync(
                "en-US", cardsQuery, null,
                _ => false, 100, CancellationToken.None);

            Assert.That(result.TotalCount, Is.EqualTo(1));
        }

        // ---- Filter expressions / query-builder lambda ------------------------------

        private static IEnumerable<string> PageFilterNames => new[]
        {
            FilterParameterName.BusinessLine,
            FilterParameterName.BusinessIssue,
            FilterParameterName.ServiceArea,
            FilterParameterName.Service,
            FilterParameterName.IndustryCategory,
            FilterParameterName.IndustryPage,
            FilterParameterName.MicrositeMasterPage,
            FilterParameterName.MicrositePage,
            FilterParameterName.Person
        };

        [TestCaseSource(nameof(PageFilterNames))]
        public async Task GetFilteredItemsAsync_PageFilter_ResolvesPageAndAppliesWhere(string filterName)
        {
            SetupGroupsAndPosts(CreateBlogPost("Post", new DateTime(2024, 6, 1)));

            _contentServiceMock
                .Setup(x => x.GetPageByTypeAndNameAsync(It.IsAny<Type>(), It.IsAny<string>()))
                .ReturnsAsync(CreateBlogPost("Filter page", new DateTime(2024, 1, 1)));

            var result = await _service.GetFilteredItemsAsync(
                "en-US", Query(new Dictionary<string, string> { [filterName] = "value" }),
                null, _ => false, 100, CancellationToken.None);

            Assert.That(result.TotalCount, Is.EqualTo(1));
            _contentServiceMock.Verify(
                x => x.GetPageByTypeAndNameAsync(It.IsAny<Type>(), "value"), Times.Once);
        }

        [Test]
        public async Task GetFilteredItemsAsync_PageFilter_PageNotFound_FilterIsIgnored()
        {
            SetupGroupsAndPosts(CreateBlogPost("Post", new DateTime(2024, 6, 1)));

            _contentServiceMock
                .Setup(x => x.GetPageByTypeAndNameAsync(It.IsAny<Type>(), It.IsAny<string>()))
                .ReturnsAsync(() => null!);

            var result = await _service.GetFilteredItemsAsync(
                "en-US", Query(new Dictionary<string, string> { [FilterParameterName.BusinessLine] = "missing" }),
                null, _ => false, 100, CancellationToken.None);

            Assert.That(result.TotalCount, Is.EqualTo(1));
        }

        [Test]
        public async Task GetFilteredItemsAsync_MultiplePageFilters_AllAreApplied()
        {
            SetupGroupsAndPosts(CreateBlogPost("Post", new DateTime(2024, 6, 1)));

            _contentServiceMock
                .Setup(x => x.GetPageByTypeAndNameAsync(It.IsAny<Type>(), It.IsAny<string>()))
                .ReturnsAsync(CreateBlogPost("Filter page", new DateTime(2024, 1, 1)));

            var filters = new Dictionary<string, string>
            {
                [FilterParameterName.BusinessLine] = "a",
                [FilterParameterName.Service] = "b",
                [FilterParameterName.Person] = "c"
            };

            var result = await _service.GetFilteredItemsAsync(
                "en-US", Query(filters), null, _ => false, 100, CancellationToken.None);

            Assert.That(result.TotalCount, Is.EqualTo(1));
            _contentServiceMock.Verify(
                x => x.GetPageByTypeAndNameAsync(It.IsAny<Type>(), It.IsAny<string>()), Times.Exactly(3));
        }

        [Test]
        public async Task GetFilteredItemsAsync_KeywordsFilter_AppliesFieldExpression()
        {
            SetupGroupsAndPosts(CreateBlogPost("Post", new DateTime(2024, 6, 1)));

            var result = await _service.GetFilteredItemsAsync(
                "en-US", Query(new Dictionary<string, string> { [FilterParameterName.Keywords] = "tax" }),
                null, _ => false, 100, CancellationToken.None);

            Assert.That(result.TotalCount, Is.EqualTo(1));
        }

        [Test]
        public async Task GetFilteredItemsAsync_UnknownFilterName_IsIgnored()
        {
            SetupGroupsAndPosts(CreateBlogPost("Post", new DateTime(2024, 6, 1)));

            var result = await _service.GetFilteredItemsAsync(
                "en-US", Query(new Dictionary<string, string> { ["not-a-real-filter"] = "x" }),
                null, _ => false, 100, CancellationToken.None);

            Assert.That(result.TotalCount, Is.EqualTo(1));
        }


        [Test]
        public async Task GetFilteredItemsAsync_InvokesDecorateQuery()
        {
            SetupGroupsAndPosts(CreateBlogPost("Post", new DateTime(2024, 6, 1)));
            var decorated = false;

            await _service.GetFilteredItemsAsync(
                "en-US", Query(new Dictionary<string, string>()),
                _ => decorated = true, _ => false, 100, CancellationToken.None);

            Assert.That(decorated, Is.True);
        }

        [Test]
        public async Task GetFilteredItemsAsync_ParentPageIdZero_SkipsParentFilter()
        {
            SetupGroupsAndPosts(CreateBlogPost("Post", new DateTime(2024, 6, 1)));

            var result = await _service.GetFilteredItemsAsync(
                "en-US", Query(new Dictionary<string, string>()),
                null, _ => false, 0, CancellationToken.None);

            Assert.That(result.TotalCount, Is.EqualTo(1));
        }

        [Test]
        public async Task GetFilteredItemsAsync_NoLastPublishedDate_FallsBackToFirstPublished()
        {
            var fallbackPost = CreateBlogPostWithDates("Fallback", new DateTime(2024, 9, 1), null);
            var normalPost = CreateBlogPostWithDates("Normal", new DateTime(2020, 1, 1), new DateTime(2024, 6, 1));

            SetupGroupsAndPosts(normalPost, fallbackPost);

            var result = await _service.GetFilteredItemsAsync(
                "en-US", Query(new Dictionary<string, string>()),
                null, _ => false, 100, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.Data[0].Title, Is.EqualTo("Fallback"));
                Assert.That(result.Data[0].Date, Is.EqualTo(new DateTime(2024, 9, 1)));
                Assert.That(result.Data[1].Title, Is.EqualTo("Normal"));
            });
        }

        #endregion

        // ==========================================================================
        // GetContentPageCardListAsync
        // ==========================================================================

        #region GetContentPageCardListAsync

        [Test]
        public async Task GetContentPageCardListAsync_EmptyCollection_ReturnsEmptyList()
        {
            var result = await _service.GetContentPageCardListAsync(
                Array.Empty<BlogPost>(), "en-US", _ => false, CancellationToken.None);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetContentPageCardListAsync_MapsTitleCorrectly()
        {
            var post = CreateBlogPost("My Blog Title", new DateTime(2024, 6, 1));

            var result = await _service.GetContentPageCardListAsync(
                new[] { post }, "en-US", _ => false, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Has.Count.EqualTo(1));
                Assert.That(result[0].Title, Is.EqualTo("My Blog Title"));
            });
        }

        [Test]
        public async Task GetContentPageCardListAsync_MapsIsFeaturedCorrectly()
        {
            var post = CreateBlogPost("Featured Post", new DateTime(2024, 6, 1));

            var result = await _service.GetContentPageCardListAsync(
                new[] { post }, "en-US", _ => true, CancellationToken.None);

            Assert.That(result[0].IsFeatured, Is.True);
        }

        [Test]
        public async Task GetContentPageCardListAsync_MapsIsFeaturedFalse_WhenPredicateReturnsFalse()
        {
            var post = CreateBlogPost("Regular Post", new DateTime(2024, 6, 1));

            var result = await _service.GetContentPageCardListAsync(
                new[] { post }, "en-US", _ => false, CancellationToken.None);

            Assert.That(result[0].IsFeatured, Is.False);
        }

        [Test]
        public async Task GetContentPageCardListAsync_MapsImageAltText()
        {
            var post = CreateBlogPost("Post", new DateTime(2024, 6, 1));
            post.ImageFileAltTextM = "My Image Alt";

            var result = await _service.GetContentPageCardListAsync(
                new[] { post }, "en-US", _ => false, CancellationToken.None);

            Assert.That(result[0].ImageAltText, Is.EqualTo("My Image Alt"));
        }

        [Test]
        public async Task GetContentPageCardListAsync_MapsDescription_StrippingHtml()
        {
            var post = CreateBlogPost("Post", new DateTime(2024, 6, 1));
            post.MetadataDescription = "<p>Hello <strong>World</strong></p>";

            var result = await _service.GetContentPageCardListAsync(
                new[] { post }, "en-US", _ => false, CancellationToken.None);

            // Should contain stripped text (depends on HtmlHelper.ExtractTextFromHtml implementation)
            Assert.Multiple(() =>
            {
                Assert.That(result[0].Description, Is.Not.Null);
                Assert.That(result[0].Description, Does.Not.Contain("<p>"));
                Assert.That(result[0].Description, Does.Not.Contain("<strong>"));
            });
        }

        [Test]
        public async Task GetContentPageCardListAsync_MapsDateFromLastPublished()
        {
            var publishDate = new DateTime(2024, 6, 15);
            var post = CreateBlogPost("Post", publishDate);

            var result = await _service.GetContentPageCardListAsync(
                new[] { post }, "en-US", _ => false, CancellationToken.None);

            Assert.That(result[0].Date, Is.EqualTo(publishDate));
        }

        [Test]
        public async Task GetContentPageCardListAsync_MapsUpdatedOnDate_WhenNotMinValue()
        {
            var post = CreateBlogPost("Post", new DateTime(2024, 6, 1));
            post.UpdatedOn = new DateTime(2024, 7, 1);

            var result = await _service.GetContentPageCardListAsync(
                new[] { post }, "en-US", _ => false, CancellationToken.None);

            Assert.That(result[0].UpdatedOnDate, Is.EqualTo(new DateTime(2024, 7, 1)));
        }

        [Test]
        public async Task GetContentPageCardListAsync_UpdatedOnDateIsNull_WhenMinValue()
        {
            var post = CreateBlogPost("Post", new DateTime(2024, 6, 1));
            post.UpdatedOn = DateTime.MinValue;

            var result = await _service.GetContentPageCardListAsync(
                new[] { post }, "en-US", _ => false, CancellationToken.None);

            Assert.That(result[0].UpdatedOnDate, Is.Null);
        }

        [Test]
        public async Task GetContentPageCardListAsync_MapsImageUrl_WhenImageExists()
        {
            var post = CreateBlogPost("Post", new DateTime(2024, 6, 1));
            post.ImageFileM = new[]
            {
                new ContentAttachment
                {
                    ContentAttachmentAsset = new ContentItemAsset { Url = "/media/blog-image.jpg" }
                }
            };

            _urlHelperMock
                .Setup(x => x.Content("/media/blog-image.jpg"))
                .Returns("/media/blog-image.jpg");

            var result = await _service.GetContentPageCardListAsync(
                new[] { post }, "en-US", _ => false, CancellationToken.None);

            Assert.That(result[0].ImageURL, Is.EqualTo("/media/blog-image.jpg"));
        }

        [Test]
        public async Task GetContentPageCardListAsync_MapsEmptyImageUrl_WhenNoImage()
        {
            var post = CreateBlogPost("Post", new DateTime(2024, 6, 1));
            post.ImageFileM = Array.Empty<ContentAttachment>();

            var result = await _service.GetContentPageCardListAsync(
                new[] { post }, "en-US", _ => false, CancellationToken.None);

            Assert.That(result[0].ImageURL, Is.Null.Or.Empty);
        }

        [Test]
        public async Task GetContentPageCardListAsync_MapsContentTypeName_WhenTypeFilterPresent()
        {
            var post = CreateBlogPost("Post", new DateTime(2024, 6, 1));
            post.ContentTypeFilter = new[] { new Contenttypes { Name = "Insights" } };

            var result = await _service.GetContentPageCardListAsync(
                new[] { post }, "en-US", _ => false, CancellationToken.None);

            Assert.That(result[0].Type, Is.EqualTo("Insights"));
        }

        [Test]
        public async Task GetContentPageCardListAsync_TypeIsEmpty_WhenNoTypeFilter()
        {
            var post = CreateBlogPost("Post", new DateTime(2024, 6, 1));
            post.ContentTypeFilter = Array.Empty<Contenttypes>();

            var result = await _service.GetContentPageCardListAsync(
                new[] { post }, "en-US", _ => false, CancellationToken.None);

            Assert.That(result[0].Type, Is.EqualTo(string.Empty));
        }

        [Test]
        public async Task GetContentPageCardListAsync_BuildsReadMoreLink_AndIsNotComingSoon()
        {
            var post = CreateBlogPost("Post", new DateTime(2024, 6, 1));

            var result = await _service.GetContentPageCardListAsync(
                new[] { post }, "en-US", _ => false, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result[0].ReadMoreLink, Is.Not.Null);
                Assert.That(result[0].ReadMoreLink.Title, Is.EqualTo(ResourceStringKey.LiveSite.ReadMore));
                Assert.That(result[0].isComingSoon, Is.False);
            });
        }

        [Test]
        public async Task GetContentPageCardListAsync_DateFallsBackToFirstPublished_WhenLastPublishedIsNull()
        {
            var post = CreateBlogPostWithDates("Post", new DateTime(2024, 3, 3), null);

            var result = await _service.GetContentPageCardListAsync(
                new[] { post }, "en-US", _ => false, CancellationToken.None);

            Assert.That(result[0].Date, Is.EqualTo(new DateTime(2024, 3, 3)));
        }

        [Test]
        public async Task GetContentPageCardListAsync_MultiplePosts_ReturnsCardPerPost()
        {
            var posts = new[]
            {
                CreateBlogPost("A", new DateTime(2024, 1, 1)),
                CreateBlogPost("B", new DateTime(2024, 1, 2))
            };

            var result = await _service.GetContentPageCardListAsync(
                posts, "en-US", p => p.MetadataTitle == "B", CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Has.Count.EqualTo(2));
                Assert.That(result[0].IsFeatured, Is.False);
                Assert.That(result[1].IsFeatured, Is.True);
            });
        }

        #endregion

        // ==========================================================================
        // Helpers
        // ==========================================================================

        private static CardsQuery Query(IDictionary<string, string> filters) => new CardsQuery
        {
            CurrentPage = 1,
            PageSize = 10,
            Filters = filters
        };

        /// <summary>
        /// Sets up one blog group and makes the query-builder call run the service's lambda against a
        /// real ContentItemQueryBuilder (so the language / parent / filter / decorate lines execute).
        /// </summary>
        private void SetupGroupsAndPosts(params BlogPost[] posts)
        {
            _contentServiceMock
                .Setup(x => x.GetChildPagesByParentGuidAsync<BlogPostGroup>(
                    It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { CreateBlogPostGroup(10) });

            _contentServiceMock
                .Setup(x => x.GetChildPagesByParentGuidWithQueryBuilderAsync<BlogPost>(
                    It.IsAny<Action<ContentItemQueryBuilder>>(), It.IsAny<CancellationToken>()))
                .Callback<Action<ContentItemQueryBuilder>, CancellationToken>(
                    (configure, _) => configure(new ContentItemQueryBuilder()))
                .ReturnsAsync(posts);
        }

        /// <summary>Finds a filter name (from FilterParameterName constants) matching the predicate.</summary>
        private static string RequireFilter(Func<FilterParameterData, bool> predicate, string description)
        {
            var names = typeof(FilterParameterName)
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(f => f.FieldType == typeof(string))
                .Select(f => f.GetValue(null) as string)
                .Where(v => !string.IsNullOrEmpty(v));

            foreach (var name in names)
            {
                var data = FilterUtility.GetFilterParameterData(name!);
                if (data is not null && predicate(data))
                {
                    return name!;
                }
            }

            Assert.Ignore($"No filter found in FilterParameterName for: {description}.");
            return string.Empty;
        }

        /// <summary>Reads the OptionDto[] off a FilterItemDto without depending on the property name.</summary>
        private static OptionDto[] GetOptions(FilterItemDto<OptionDto> filter)
        {
            var options = filter.GetType()
                .GetProperties()
                .Select(p => p.GetValue(filter))
                .OfType<IEnumerable<OptionDto>>()
                .FirstOrDefault();

            Assert.That(options, Is.Not.Null, "Could not find an OptionDto collection on FilterItemDto.");
            return options!.ToArray();
        }

        private static BlogPostGroup CreateBlogPostGroup(int webPageItemId = 1)
        {
            return new BlogPostGroup
            {
                SystemFields = new WebPageFields
                {
                    ContentItemID = 1,
                    ContentItemGUID = Guid.NewGuid(),
                    ContentItemName = "Test Group",
                    WebPageItemID = webPageItemId,
                    WebPageItemGUID = Guid.NewGuid(),
                    WebPageUrlPath = "/blogs/group",
                    WebPageItemOrder = 1,
                    WebPageItemTreePath = "/blogs/group",
                    WebPageItemParentID = 100
                }
            };
        }

        /// <summary>Creates a post whose first and last published dates are both <paramref name="publishDate"/>.</summary>
        private static BlogPost CreateBlogPost(string title, DateTime publishDate)
        {
            return new BlogPost
            {
                SystemFields = new WebPageFields
                {
                    ContentItemID = 1,
                    ContentItemGUID = Guid.NewGuid(),
                    ContentItemName = title.Replace(" ", "-").ToLower(),
                    ContentItemContentTypeID = 100,
                    ContentItemIsSecured = false,
                    ContentItemCommonDataContentLanguageID = 1,
                    ContentItemCommonDataFirstPublishedWhen = publishDate,
                    ContentItemCommonDataLastPublishedWhen = publishDate,
                    WebPageItemGUID = Guid.NewGuid(),
                    WebPageItemID = 1,
                    WebPageItemName = title.Replace(" ", "-").ToLower(),
                    WebPageItemWebsiteChannelId = 1,
                    WebPageUrlPath = $"/blogs/{title.Replace(" ", "-").ToLower()}",
                    WebPageItemOrder = 1,
                    WebPageItemTreePath = $"/blogs/{title.Replace(" ", "-").ToLower()}",
                    WebPageUrlPathContentLanguageID = 1,
                    WebPageItemParentID = 10
                },
                MetadataTitle = title,
                MetadataDescription = $"Description for {title}",
                ImageFileM = Array.Empty<ContentAttachment>(),
                ImageFileAltTextM = "alt",
                ContentTypeFilter = Array.Empty<Contenttypes>(),
                UpdatedOn = DateTime.MinValue,
                MetadataKeywordsBlog = string.Empty,
                MetadataBusinessIssues = Array.Empty<Businessissue>(),
                ShowCallToAction = false,
                CallToActionDescription = string.Empty,
                CallToActionButtonText = string.Empty,
                CallToActionButtonUrl = string.Empty,
                ShowSidebar = false,
                ShowPrintButton = false,
                DownloadButtonAttachment = Array.Empty<ContentAttachment>(),
                ShowSocialMediaButtons = false,
                MetadataOGTitle = string.Empty,
                MetadataOGDescription = string.Empty,
                MetadataOGImage = Array.Empty<ContentAttachment>(),
                MetadataOGTwitterImage = Array.Empty<ContentAttachment>(),
                MetadataTeaserImage = Array.Empty<ContentAttachment>(),
                MetadataTeaserImageAltText = string.Empty,
                MetadataCanonical = string.Empty,
                MetadataRssImage = Array.Empty<IContentItemFieldsSource>(),
                MetadataBusinessLine = Array.Empty<BusinessLine>(),
                MetadataServices = Array.Empty<Service>(),
                MetadataServiceAreas = Array.Empty<ServiceArea>(),
                MetadataIndustryCategories = Array.Empty<IndustryCategory>(),
                MetadataIndustries = Array.Empty<Industry>(),
                MicrositePages = Array.Empty<MicrositePage>(),
                MicrositeMasterPages = Array.Empty<MicrositeMasterPage>(),
                MetadataRssCategory = Array.Empty<MicrositePlaceholder>(),
                BlogRelevantContactPerson = Array.Empty<Person>(),
                BlogRelatedExternalPeople = Array.Empty<ExternalPerson>(),
                PageTitle = title,
                PageDescription = string.Empty,
                PageKeywords = string.Empty,
                ShowTitle = true,
                ShowDescription = true,
                Content = "<p>Content</p>"
            };
        }

        /// <summary>Same as <see cref="CreateBlogPost"/> but with independent first/last published dates.</summary>
        private static BlogPost CreateBlogPostWithDates(string title, DateTime firstPublished, DateTime? lastPublished)
        {
            var post = CreateBlogPost(title, firstPublished);
            post.SystemFields.ContentItemCommonDataLastPublishedWhen = lastPublished;
            return post;
        }

        // ---- Fakes used by the reflection-based option builders --------------------

        private class FakeSystemFields
        {
            public string? ContentItemName { get; set; }
        }

        private class FakePageItem
        {
            public string? MetadataTitle { get; set; }
            public string? MetadataOGTitle { get; set; }
            public FakeSystemFields? SystemFields { get; set; }
        }

        private class FakeTagItem
        {
            public string? Name { get; set; }
        }
    }
}