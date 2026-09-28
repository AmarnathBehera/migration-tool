using BDO.GWT.Core;
using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
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
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Core.Tests.Services
{
    public class BlogPageCardsServiceTests
    {
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

            var result = await _service.CreateBlogPageFiltersAsync(filters, 1,CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Length.EqualTo(2));
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
            Assert.That(result[0].Description, Is.Not.Null);
            Assert.That(result[0].Description, Does.Not.Contain("<p>"));
            Assert.That(result[0].Description, Does.Not.Contain("<strong>"));
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

        #endregion

        // ==========================================================================
        // Helpers
        // ==========================================================================

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
                ContentType = Array.Empty<Contenttypes>(),
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
    }
}