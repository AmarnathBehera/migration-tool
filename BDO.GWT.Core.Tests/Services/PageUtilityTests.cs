using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.services;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Core.Tests.Services
{
    [TestFixture]
    public class PageUtilityTests
    {
        private static readonly string[] ExpectedIndustryTitlesA = ["Industry A1", "Industry A2"];

        private Mock<IContentService> _contentServiceMock = null!;
        private Mock<IWebPageUrlResolver> _urlResolverMock = null!;
        private PageUtility _pageUtility = null!;

        [SetUp]
        public void Setup()
        {
            _contentServiceMock = new Mock<IContentService>();
            _urlResolverMock = new Mock<IWebPageUrlResolver>();
            _urlResolverMock
                .Setup(x => x.GetRelativePath(It.IsAny<IWebPageFieldsSource>()))
                .Returns((IWebPageFieldsSource p) => p.SystemFields.WebPageUrlPath);
            _pageUtility = new PageUtility(_contentServiceMock.Object, _urlResolverMock.Object);
        }

        #region Helpers

        private static WebPageFields CreateWebPageFields(int id, int parentId = 0, int order = 0, string urlPath = "/test") =>
            new WebPageFields
            {
                WebPageItemID = id,
                WebPageItemParentID = parentId,
                WebPageItemOrder = order,
                WebPageUrlPath = urlPath,
                ContentItemID = id,
                ContentItemGUID = Guid.NewGuid(),
                ContentItemName = $"Page-{id}",
                ContentItemContentTypeID = 1,
                ContentItemIsSecured = false,
                ContentItemCommonDataContentLanguageID = 1,
                WebPageItemGUID = Guid.NewGuid(),
                WebPageItemName = $"Page-{id}",
                WebPageItemWebsiteChannelId = 1,
                WebPageItemTreePath = urlPath,
                WebPageUrlPathContentLanguageID = 1,
            };

        private static IndustryCategory CreateIndustryCategory(int id, int parentId = 0, int order = 0, string title = "Category") =>
            new IndustryCategory
            {
                SystemFields = CreateWebPageFields(id, parentId, order, $"/category-{id}"),
                DescriptionTitle = title,
                DescriptionBody = $"Description for {title}",
            };

        private static Industry CreateIndustry(int id, int parentId = 0, int order = 0, string title = "Industry") =>
            new Industry
            {
                SystemFields = CreateWebPageFields(id, parentId, order, $"/industry-{id}"),
                DescriptionTitle = title,
            };

        private void SetupCategories(int parentId, params IndustryCategory[] categories) =>
            _contentServiceMock
                .Setup(x => x.GetChildPagesByParentGuidAsync<IndustryCategory>(parentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(categories.ToList());

        private void SetupPages<T>(params T[] pages) where T : class, IWebPageFieldsSource =>
            _contentServiceMock
                .Setup(x => x.GetPagesAsync<T>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(pages.ToList());

        #endregion

        #region GetSideboxDtoAsync

        [Test]
        public async Task GetSideboxDtoAsync_WhenChildPagesExist_ReturnsSideBoxWithChildLinks()
        {
            var parentId = 10;
            var siblingParentId = 20;

            var children = new List<IndustryCategory>
            {
                CreateIndustryCategory(1, parentId, 2, "Child B"),
                CreateIndustryCategory(2, parentId, 1, "Child A"),
            };

            _contentServiceMock
                .Setup(x => x.GetChildPagesByParentGuidAsync<IndustryCategory>(parentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(children);

            var result = await _pageUtility.GetSideboxDtoAsync<IndustryCategory, IndustryCategory>(
                "Nav Title",
                parentId,
                siblingParentId,
                c => c.DescriptionTitle,
                c => c.DescriptionTitle,
                CancellationToken.None);

            var links = result.Links.ToList();
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Title, Is.EqualTo("Nav Title"));
                Assert.That(links, Has.Count.EqualTo(2));
                // Should be ordered by WebPageItemOrder (1 first, then 2)
                Assert.That(links[0].Title, Is.EqualTo("Child A"));
                Assert.That(links[1].Title, Is.EqualTo("Child B"));
            });
        }

        [Test]
        public async Task GetSideboxDtoAsync_WhenNoChildPages_ReturnsSiblingLinks()
        {
            var parentId = 10;
            var siblingParentId = 20;

            var siblings = new List<IndustryCategory>
            {
                CreateIndustryCategory(5, siblingParentId, 1, "Sibling A"),
                CreateIndustryCategory(6, siblingParentId, 2, "Sibling B"),
            };

            _contentServiceMock
                .Setup(x => x.GetChildPagesByParentGuidAsync<IndustryCategory>(parentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<IndustryCategory>());

            _contentServiceMock
                .Setup(x => x.GetChildPagesByParentGuidAsync<IndustryCategory>(siblingParentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(siblings);

            var result = await _pageUtility.GetSideboxDtoAsync<IndustryCategory, IndustryCategory>(
                "Sibling Nav",
                parentId,
                siblingParentId,
                c => c.DescriptionTitle,
                c => c.DescriptionTitle,
                CancellationToken.None);

            var links = result.Links.ToList();
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Title, Is.EqualTo("Sibling Nav"));
                Assert.That(links, Has.Count.EqualTo(2));
                Assert.That(links[0].Title, Is.EqualTo("Sibling A"));
                Assert.That(links[1].Title, Is.EqualTo("Sibling B"));
            });
        }

        [Test]
        public async Task GetSideboxDtoAsync_WhenOnlyOneSiblingExists_ReturnsEmptyLinks()
        {
            var parentId = 10;
            var siblingParentId = 20;

            var siblings = new List<IndustryCategory>
            {
                CreateIndustryCategory(5, siblingParentId, 1, "Only Sibling"),
            };

            _contentServiceMock
                .Setup(x => x.GetChildPagesByParentGuidAsync<IndustryCategory>(parentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<IndustryCategory>());

            _contentServiceMock
                .Setup(x => x.GetChildPagesByParentGuidAsync<IndustryCategory>(siblingParentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(siblings);

            var result = await _pageUtility.GetSideboxDtoAsync<IndustryCategory, IndustryCategory>(
                "Nav",
                parentId,
                siblingParentId,
                c => c.DescriptionTitle,
                c => c.DescriptionTitle,
                CancellationToken.None);

            Assert.That(result.Links.ToList(), Is.Empty);
        }

        [Test]
        public async Task GetSideboxDtoAsync_WhenNeitherChildrenNorSiblingsExist_ReturnsEmptyLinks()
        {
            var parentId = 10;
            var siblingParentId = 20;

            _contentServiceMock
                .Setup(x => x.GetChildPagesByParentGuidAsync<IndustryCategory>(parentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<IndustryCategory>());

            _contentServiceMock
                .Setup(x => x.GetChildPagesByParentGuidAsync<IndustryCategory>(siblingParentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<IndustryCategory>());

            var result = await _pageUtility.GetSideboxDtoAsync<IndustryCategory, IndustryCategory>(
                "Nav",
                parentId,
                siblingParentId,
                c => c.DescriptionTitle,
                c => c.DescriptionTitle,
                CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Links.ToList(), Is.Empty);
            });
        }

        [Test]
        public async Task GetSideboxDtoAsync_ChildLinksHaveCorrectUrls()
        {
            var parentId = 10;
            var siblingParentId = 20;

            var children = new List<IndustryCategory>
            {
                CreateIndustryCategory(1, parentId, 1, "Child A"),
            };

            _contentServiceMock
                .Setup(x => x.GetChildPagesByParentGuidAsync<IndustryCategory>(parentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(children);

            var result = await _pageUtility.GetSideboxDtoAsync<IndustryCategory, IndustryCategory>(
                "Nav",
                parentId,
                siblingParentId,
                c => c.DescriptionTitle,
                c => c.DescriptionTitle,
                CancellationToken.None);

            var link = result.Links.First();
            Assert.That(link.URL, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public async Task GetSideboxDtoAsync_WhenChildrenExist_SiblingsAreNeverQueried()
        {
            SetupCategories(10, CreateIndustryCategory(1, 10, 1, "Child"));

            await _pageUtility.GetSideboxDtoAsync<IndustryCategory, IndustryCategory>(
                "Nav", 10, 20, c => c.DescriptionTitle, c => c.DescriptionTitle);

            _contentServiceMock.Verify(
                x => x.GetChildPagesByParentGuidAsync<IndustryCategory>(20, It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task GetSideboxDtoAsync_ChildLinkUrlComesFromUrlResolver()
        {
            SetupCategories(10, CreateIndustryCategory(1, 10, 1, "Child"));

            var result = await _pageUtility.GetSideboxDtoAsync<IndustryCategory, IndustryCategory>(
                "Nav", 10, 20, c => c.DescriptionTitle, c => c.DescriptionTitle);

            Assert.That(result.Links.Single().URL, Is.EqualTo("/category-1"));
        }

        [Test]
        public async Task GetSideboxDtoAsync_SiblingsAreOrderedByWebPageItemOrder()
        {
            SetupCategories(10);
            SetupCategories(20,
                CreateIndustryCategory(5, 20, 2, "Sibling B"),
                CreateIndustryCategory(6, 20, 1, "Sibling A"));

            var result = await _pageUtility.GetSideboxDtoAsync<IndustryCategory, IndustryCategory>(
                "Nav", 10, 20, c => c.DescriptionTitle, c => c.DescriptionTitle);

            Assert.That(result.Links.Select(l => l.Title), Is.EqualTo(new[] { "Sibling A", "Sibling B" }));
        }

        #endregion

        #region GetSideboxDtoAsync (single-type overload)

        [Test]
        public async Task GetSideboxDtoAsync_SingleType_WhenSiblingsExist_ReturnsOrderedSiblingLinks()
        {
            SetupCategories(20,
                CreateIndustryCategory(5, 20, 2, "Sibling B"),
                CreateIndustryCategory(6, 20, 1, "Sibling A"));

            var result = await _pageUtility.GetSideboxDtoAsync<IndustryCategory>(
                "Single Nav", 10, 20, c => c.DescriptionTitle, CancellationToken.None);

            var links = result.Links.ToList();
            Assert.Multiple(() =>
            {
                Assert.That(result.Title, Is.EqualTo("Single Nav"));
                Assert.That(links, Has.Count.EqualTo(2));
                Assert.That(links[0].Title, Is.EqualTo("Sibling A"));
                Assert.That(links[0].URL, Is.EqualTo("/category-6"));
                Assert.That(links[1].Title, Is.EqualTo("Sibling B"));
            });
        }

        [Test]
        public async Task GetSideboxDtoAsync_SingleType_WhenOnlyOneSibling_ReturnsEmptyLinks()
        {
            SetupCategories(20, CreateIndustryCategory(5, 20, 1, "Only"));

            var result = await _pageUtility.GetSideboxDtoAsync<IndustryCategory>(
                "Nav", 10, 20, c => c.DescriptionTitle);

            Assert.That(result.Links, Is.Empty);
        }

        [Test]
        public async Task GetSideboxDtoAsync_SingleType_WhenNoSiblings_ReturnsEmptyLinks()
        {
            SetupCategories(20);

            var result = await _pageUtility.GetSideboxDtoAsync<IndustryCategory>(
                "Nav", 10, 20, c => c.DescriptionTitle);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Title, Is.EqualTo("Nav"));
                Assert.That(result.Links, Is.Empty);
            });
        }

        [Test]
        public async Task GetSideboxDtoAsync_SingleType_DoesNotQueryParentChildren()
        {
            SetupCategories(20);

            await _pageUtility.GetSideboxDtoAsync<IndustryCategory>(
                "Nav", 10, 20, c => c.DescriptionTitle);

            _contentServiceMock.Verify(
                x => x.GetChildPagesByParentGuidAsync<IndustryCategory>(10, It.IsAny<CancellationToken>()),
                Times.Never);
        }

        #endregion

        #region GetMultiLinkCardsAsync

        [Test]
        public async Task GetMultiLinkCardsAsync_WhenNoCategoriesExist_ReturnsEmptyArray()
        {
            var parentId = 100;

            _contentServiceMock
                .Setup(x => x.GetChildPagesByParentGuidAsync<IndustryCategory>(parentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<IndustryCategory>());

            var result = await _pageUtility.GetMultiLinkCardsAsync<IndustryCategory, Industry>(
                parentId,
                c => c.DescriptionTitle,
                c => c.DescriptionBody,
                c => null,
                c => null,
                i => i.DescriptionTitle,
                CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result, Is.Empty);
            });
        }

        [Test]
        public async Task GetMultiLinkCardsAsync_WhenCategoriesExist_ReturnsMappedCards()
        {
            var parentId = 100;

            var categories = new List<IndustryCategory>
            {
                CreateIndustryCategory(1, parentId, 1, "Category A"),
                CreateIndustryCategory(2, parentId, 2, "Category B"),
            };

            _contentServiceMock
                .Setup(x => x.GetChildPagesByParentGuidAsync<IndustryCategory>(parentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(categories);

            _contentServiceMock
                .Setup(x => x.GetPagesAsync<Industry>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Industry>());

            var result = await _pageUtility.GetMultiLinkCardsAsync<IndustryCategory, Industry>(
                parentId,
                c => c.DescriptionTitle,
                c => c.DescriptionBody,
                c => null,
                c => null,
                i => i.DescriptionTitle,
                CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Has.Length.EqualTo(2));
                Assert.That(result[0].Title, Is.EqualTo("Category A"));
                Assert.That(result[1].Title, Is.EqualTo("Category B"));
            });
        }

        [Test]
        public async Task GetMultiLinkCardsAsync_WhenChildrenExist_AttachesReadMoreLinksToCorrectCard()
        {
            var parentId = 100;

            var categoryA = CreateIndustryCategory(1, parentId, 1, "Category A");
            var categoryB = CreateIndustryCategory(2, parentId, 2, "Category B");

            var categories = new List<IndustryCategory> { categoryA, categoryB };

            var industries = new List<Industry>
            {
                CreateIndustry(10, parentId: 1, order: 1, title: "Industry A1"),
                CreateIndustry(11, parentId: 1, order: 2, title: "Industry A2"),
                CreateIndustry(12, parentId: 2, order: 1, title: "Industry B1"),
            };

            _contentServiceMock
                .Setup(x => x.GetChildPagesByParentGuidAsync<IndustryCategory>(parentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(categories);

            _contentServiceMock
                .Setup(x => x.GetPagesAsync<Industry>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(industries);

            var result = await _pageUtility.GetMultiLinkCardsAsync<IndustryCategory, Industry>(
                parentId,
                c => c.DescriptionTitle,
                c => c.DescriptionBody,
                c => null,
                c => null,
                i => i.DescriptionTitle,
                CancellationToken.None);

            var cardA = result.First(c => c.Title == "Category A");
            var cardB = result.First(c => c.Title == "Category B");
            Assert.Multiple(() =>
            {
                Assert.That(result, Has.Length.EqualTo(2));
                Assert.That(cardA.ReadMoreLinks, Has.Length.EqualTo(2));
                Assert.That(cardA.ReadMoreLinks.Select(l => l.Title), Is.EquivalentTo(ExpectedIndustryTitlesA));
                Assert.That(cardB.ReadMoreLinks, Has.Length.EqualTo(1));
                Assert.That(cardB.ReadMoreLinks[0].Title, Is.EqualTo("Industry B1"));
            });
        }

        [Test]
        public async Task GetMultiLinkCardsAsync_WhenCategoriesExist_CardsAreOrderedByWebPageItemOrder()
        {
            var parentId = 100;

            var categories = new List<IndustryCategory>
            {
                CreateIndustryCategory(2, parentId, 3, "Category C"),
                CreateIndustryCategory(1, parentId, 1, "Category A"),
                CreateIndustryCategory(3, parentId, 2, "Category B"),
            };

            _contentServiceMock
                .Setup(x => x.GetChildPagesByParentGuidAsync<IndustryCategory>(parentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(categories);

            _contentServiceMock
                .Setup(x => x.GetPagesAsync<Industry>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Industry>());

            var result = await _pageUtility.GetMultiLinkCardsAsync<IndustryCategory, Industry>(
                parentId,
                c => c.DescriptionTitle,
                c => c.DescriptionBody,
                c => null,
                c => null,
                i => i.DescriptionTitle,
                CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result[0].Title, Is.EqualTo("Category A"));
                Assert.That(result[1].Title, Is.EqualTo("Category B"));
                Assert.That(result[2].Title, Is.EqualTo("Category C"));
            });
        }

        [Test]
        public async Task GetMultiLinkCardsAsync_CardDescriptionIsMappedCorrectly()
        {
            var parentId = 100;

            var categories = new List<IndustryCategory>
            {
                CreateIndustryCategory(1, parentId, 1, "Category A"),
            };

            _contentServiceMock
                .Setup(x => x.GetChildPagesByParentGuidAsync<IndustryCategory>(parentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(categories);

            _contentServiceMock
                .Setup(x => x.GetPagesAsync<Industry>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Industry>());

            var result = await _pageUtility.GetMultiLinkCardsAsync<IndustryCategory, Industry>(
                parentId,
                c => c.DescriptionTitle,
                c => c.DescriptionBody,
                c => "image.png",
                c => "alt text",
                i => i.DescriptionTitle,
                CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result[0].Description, Is.EqualTo("Description for Category A"));
                Assert.That(result[0].ImageURL, Is.EqualTo("image.png"));
                Assert.That(result[0].ImageAltText, Is.EqualTo("alt text"));
            });
        }

        [Test]
        public async Task GetMultiLinkCardsAsync_WhenNoCategoriesExist_GetPagesAsyncIsNotCalled()
        {
            var parentId = 100;

            _contentServiceMock
                .Setup(x => x.GetChildPagesByParentGuidAsync<IndustryCategory>(parentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<IndustryCategory>());

            await _pageUtility.GetMultiLinkCardsAsync<IndustryCategory, Industry>(
                parentId,
                c => c.DescriptionTitle,
                c => c.DescriptionBody,
                c => null,
                c => null,
                i => i.DescriptionTitle,
                CancellationToken.None);

            _contentServiceMock.Verify(
                x => x.GetPagesAsync<Industry>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task GetMultiLinkCardsAsync_ImageUrlLeadingTildeIsTrimmed()
        {
            SetupCategories(100, CreateIndustryCategory(1, 100, 1, "Category A"));
            SetupPages<Industry>();

            var result = await _pageUtility.GetMultiLinkCardsAsync<IndustryCategory, Industry>(
                100,
                c => c.DescriptionTitle,
                c => c.DescriptionBody,
                c => "~/images/a.png",
                c => "alt",
                i => i.DescriptionTitle);

            Assert.That(result[0].ImageURL, Is.EqualTo("/images/a.png"));
        }

        [Test]
        public async Task GetMultiLinkCardsAsync_NullImageUrl_YieldsNullImageUrl()
        {
            SetupCategories(100, CreateIndustryCategory(1, 100, 1, "Category A"));
            SetupPages<Industry>();

            var result = await _pageUtility.GetMultiLinkCardsAsync<IndustryCategory, Industry>(
                100,
                c => c.DescriptionTitle,
                c => c.DescriptionBody,
                c => null,
                c => null,
                i => i.DescriptionTitle);

            Assert.Multiple(() =>
            {
                Assert.That(result[0].ImageURL, Is.Null);
                Assert.That(result[0].ImageAltText, Is.Null);
            });
        }

        [Test]
        public async Task GetMultiLinkCardsAsync_CardUrlComesFromUrlResolver()
        {
            SetupCategories(100, CreateIndustryCategory(7, 100, 1, "Category A"));
            SetupPages<Industry>();

            var result = await _pageUtility.GetMultiLinkCardsAsync<IndustryCategory, Industry>(
                100, c => c.DescriptionTitle, c => c.DescriptionBody, c => null, c => null, i => i.DescriptionTitle);

            Assert.That(result[0].Url, Is.EqualTo("/category-7"));
        }

        [Test]
        public async Task GetMultiLinkCardsAsync_CategoryWithNoMatchingChildren_HasEmptyReadMoreLinks()
        {
            SetupCategories(100,
                CreateIndustryCategory(1, 100, 1, "Category A"),
                CreateIndustryCategory(2, 100, 2, "Category B"));
            SetupPages(CreateIndustry(10, parentId: 1, order: 1, title: "Industry A1"));

            var result = await _pageUtility.GetMultiLinkCardsAsync<IndustryCategory, Industry>(
                100, c => c.DescriptionTitle, c => c.DescriptionBody, c => null, c => null, i => i.DescriptionTitle);

            Assert.Multiple(() =>
            {
                Assert.That(result[0].ReadMoreLinks, Has.Length.EqualTo(1));
                Assert.That(result[1].ReadMoreLinks, Is.Empty);
            });
        }

        [Test]
        public async Task GetMultiLinkCardsAsync_ReadMoreLinksAreOrderedAndHaveUrls()
        {
            SetupCategories(100, CreateIndustryCategory(1, 100, 1, "Category A"));
            SetupPages(
                CreateIndustry(11, parentId: 1, order: 2, title: "Second"),
                CreateIndustry(10, parentId: 1, order: 1, title: "First"));

            var result = await _pageUtility.GetMultiLinkCardsAsync<IndustryCategory, Industry>(
                100, c => c.DescriptionTitle, c => c.DescriptionBody, c => null, c => null, i => i.DescriptionTitle);

            var links = result[0].ReadMoreLinks;
            Assert.Multiple(() =>
            {
                Assert.That(links.Select(l => l.Title), Is.EqualTo(new[] { "First", "Second" }));
                Assert.That(links[0].URL, Is.EqualTo("/industry-10"));
                Assert.That(links[1].URL, Is.EqualTo("/industry-11"));
            });
        }

        [Test]
        public async Task GetMultiLinkCardsAsync_WhenCategoriesExist_GetPagesAsyncIsCalledOnce()
        {
            SetupCategories(100,
                CreateIndustryCategory(1, 100, 1, "Category A"),
                CreateIndustryCategory(2, 100, 2, "Category B"));
            SetupPages<Industry>();

            await _pageUtility.GetMultiLinkCardsAsync<IndustryCategory, Industry>(
                100, c => c.DescriptionTitle, c => c.DescriptionBody, c => null, c => null, i => i.DescriptionTitle);

            _contentServiceMock.Verify(
                x => x.GetPagesAsync<Industry>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        #endregion

        #region GetMultiLinkCardsAsync2

        // Industry = TChild1, IndustryCategory (reused as a stand-in "report" page) = TChild2.

        [Test]
        public async Task GetMultiLinkCardsAsync2_WhenNoCategoriesExist_ReturnsEmptyAndSkipsChildQueries()
        {
            SetupCategories(100);

            var result = await _pageUtility.GetMultiLinkCardsAsync2<IndustryCategory, Industry, IndustryCategory>(
                100,
                c => c.DescriptionTitle,
                c => c.DescriptionBody,
                c => null,
                c => null,
                i => i.DescriptionTitle,
                r => r.DescriptionTitle);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Empty);
                _contentServiceMock.Verify(
                    x => x.GetPagesAsync<Industry>(
                        It.IsAny<RetrievePagesParameters>(),
                        It.IsAny<Action<RetrievePagesQueryParameters>>(),
                        It.IsAny<RetrievalCacheSettings>(),
                        It.IsAny<CancellationToken>()),
                    Times.Never);
                _contentServiceMock.Verify(
                    x => x.GetPagesAsync<IndustryCategory>(
                        It.IsAny<RetrievePagesParameters>(),
                        It.IsAny<Action<RetrievePagesQueryParameters>>(),
                        It.IsAny<RetrievalCacheSettings>(),
                        It.IsAny<CancellationToken>()),
                    Times.Never);
            });
        }

        [Test]
        public async Task GetMultiLinkCardsAsync2_MapsCardFieldsAndOrdersCards()
        {
            SetupCategories(100,
                CreateIndustryCategory(2, 100, 2, "Category B"),
                CreateIndustryCategory(1, 100, 1, "Category A"));
            SetupPages<Industry>();
            SetupPages<IndustryCategory>();

            var result = await _pageUtility.GetMultiLinkCardsAsync2<IndustryCategory, Industry, IndustryCategory>(
                100,
                c => c.DescriptionTitle,
                c => c.DescriptionBody,
                c => "~/img.png",
                c => "alt",
                i => i.DescriptionTitle,
                r => r.DescriptionTitle);

            Assert.Multiple(() =>
            {
                Assert.That(result, Has.Length.EqualTo(2));
                Assert.That(result[0].Title, Is.EqualTo("Category A"));
                Assert.That(result[0].Description, Is.EqualTo("Description for Category A"));
                Assert.That(result[0].ImageURL, Is.EqualTo("/img.png"));
                Assert.That(result[0].ImageAltText, Is.EqualTo("alt"));
                Assert.That(result[0].Url, Is.EqualTo("/category-1"));
                Assert.That(result[0].ReadMoreLinks, Is.Empty);
                Assert.That(result[1].Title, Is.EqualTo("Category B"));
            });
        }

        [Test]
        public async Task GetMultiLinkCardsAsync2_NullImageUrl_YieldsNullImageUrl()
        {
            SetupCategories(100, CreateIndustryCategory(1, 100, 1, "Category A"));
            SetupPages<Industry>();
            SetupPages<IndustryCategory>();

            var result = await _pageUtility.GetMultiLinkCardsAsync2<IndustryCategory, Industry, IndustryCategory>(
                100, c => c.DescriptionTitle, c => c.DescriptionBody, c => null, c => null,
                i => i.DescriptionTitle, r => r.DescriptionTitle);

            Assert.That(result[0].ImageURL, Is.Null);
        }

        [Test]
        public async Task GetMultiLinkCardsAsync2_AttachesBothChildTypesToCorrectCards()
        {
            SetupCategories(100,
                CreateIndustryCategory(1, 100, 1, "Category A"),
                CreateIndustryCategory(2, 100, 2, "Category B"));
            SetupPages(
                CreateIndustry(10, parentId: 1, order: 1, title: "Industry A1"),
                CreateIndustry(11, parentId: 2, order: 1, title: "Industry B1"));
            SetupPages(
                CreateIndustryCategory(20, parentId: 1, order: 2, title: "Report A1"));

            var result = await _pageUtility.GetMultiLinkCardsAsync2<IndustryCategory, Industry, IndustryCategory>(
                100, c => c.DescriptionTitle, c => c.DescriptionBody, c => null, c => null,
                i => i.DescriptionTitle, r => r.DescriptionTitle);

            Assert.Multiple(() =>
            {
                Assert.That(result[0].ReadMoreLinks.Select(l => l.Title),
                    Is.EqualTo(new[] { "Industry A1", "Report A1" }));
                Assert.That(result[1].ReadMoreLinks.Select(l => l.Title),
                    Is.EqualTo(new[] { "Industry B1" }));
                Assert.That(result[0].ReadMoreLinks[0].URL, Is.EqualTo("/industry-10"));
                Assert.That(result[0].ReadMoreLinks[1].URL, Is.EqualTo("/category-20"));
            });
        }

        [Test]
        public async Task GetMultiLinkCardsAsync2_CombinedChildrenAreOrderedByWebPageItemOrder()
        {
            SetupCategories(100, CreateIndustryCategory(1, 100, 1, "Category A"));
            SetupPages(
                CreateIndustry(10, parentId: 1, order: 3, title: "I-late"),
                CreateIndustry(11, parentId: 1, order: 2, title: "I-mid"));
            SetupPages(
                CreateIndustryCategory(20, parentId: 1, order: 1, title: "R-first"));

            var result = await _pageUtility.GetMultiLinkCardsAsync2<IndustryCategory, Industry, IndustryCategory>(
                100, c => c.DescriptionTitle, c => c.DescriptionBody, c => null, c => null,
                i => i.DescriptionTitle, r => r.DescriptionTitle);

            Assert.That(result[0].ReadMoreLinks.Select(l => l.Title),
                Is.EqualTo(new[] { "R-first", "I-mid", "I-late" }));
        }

        [Test]
        public async Task GetMultiLinkCardsAsync2_OnlyChild1Exists_StillReturnsLinks()
        {
            SetupCategories(100, CreateIndustryCategory(1, 100, 1, "Category A"));
            SetupPages(CreateIndustry(10, parentId: 1, order: 1, title: "Industry A1"));
            SetupPages<IndustryCategory>();

            var result = await _pageUtility.GetMultiLinkCardsAsync2<IndustryCategory, Industry, IndustryCategory>(
                100, c => c.DescriptionTitle, c => c.DescriptionBody, c => null, c => null,
                i => i.DescriptionTitle, r => r.DescriptionTitle);

            Assert.That(result[0].ReadMoreLinks.Select(l => l.Title), Is.EqualTo(new[] { "Industry A1" }));
        }

        [Test]
        public async Task GetMultiLinkCardsAsync2_ChildWithUnknownParent_IsNotAttachedToAnyCard()
        {
            SetupCategories(100, CreateIndustryCategory(1, 100, 1, "Category A"));
            SetupPages(CreateIndustry(10, parentId: 999, order: 1, title: "Orphan"));
            SetupPages<IndustryCategory>();

            var result = await _pageUtility.GetMultiLinkCardsAsync2<IndustryCategory, Industry, IndustryCategory>(
                100, c => c.DescriptionTitle, c => c.DescriptionBody, c => null, c => null,
                i => i.DescriptionTitle, r => r.DescriptionTitle);

            Assert.That(result[0].ReadMoreLinks, Is.Empty);
        }

        #endregion
    }
}