using BDO.GWT.Core;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Web.Core.Services;
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
    public class LegalPageServiceTests
    {
        private Mock<IContentService> _contentServiceMock = null!;
        private Mock<IWebPageUrlRetriever> _webPageUrlRetrieverMock = null!;
        private LegalPageService _service = null!;

        [SetUp]
        public void Setup()
        {
            _contentServiceMock = new Mock<IContentService>();
            _webPageUrlRetrieverMock = new Mock<IWebPageUrlRetriever>();

            _contentServiceMock
                .Setup(x => x.GetCurrentLanguage())
                .Returns("en");

            _service = new LegalPageService(
                _contentServiceMock.Object,
                _webPageUrlRetrieverMock.Object);
        }


        // ── Title / BodyContent mapping ──────────────────────────────────────

        [Test]
        public async Task GetLegalPageDtoAsync_MapsTitle_FromMetadataTitle()
        {
            var page = CreateLegalPage(webPageItemId: 1, parentId: 0, title: "Privacy Policy");

            SetupCurrentPage(page);
            SetupNoSiblings(parentId: 0);

            var result = await _service.GetLegalPageDtoAsync(CancellationToken.None);

            Assert.That(result.Title, Is.EqualTo("Privacy Policy"));
        }

        [Test]
        public async Task GetLegalPageDtoAsync_MapsBodyContent_FromMetadataDescription()
        {
            var page = CreateLegalPage(webPageItemId: 1, parentId: 0, description: "<p>Legal text.</p>");

            SetupCurrentPage(page);
            SetupNoSiblings(parentId: 0);

            var result = await _service.GetLegalPageDtoAsync(CancellationToken.None);

            Assert.That(result.BodyContent, Is.EqualTo("<p>Legal text.</p>"));
        }

        [Test]
        public async Task GetLegalPageDtoAsync_BodyContent_IsEmptyString_WhenDescriptionIsNull()
        {
            var page = CreateLegalPage(webPageItemId: 1, parentId: 0, description: null);

            SetupCurrentPage(page);
            SetupNoSiblings(parentId: 0);

            var result = await _service.GetLegalPageDtoAsync(CancellationToken.None);

            Assert.That(result.BodyContent, Is.EqualTo(string.Empty));
        }

        // ── Links – no parent / no siblings ─────────────────────────────────

        [Test]
        public async Task GetLegalPageDtoAsync_ReturnsEmptyLinks_WhenParentIdIsZero()
        {
            var page = CreateLegalPage(webPageItemId: 1, parentId: 0);

            SetupCurrentPage(page);

            var result = await _service.GetLegalPageDtoAsync(CancellationToken.None);

            Assert.That(result.Links, Is.Empty);
        }

        [Test]
        public async Task GetLegalPageDtoAsync_ReturnsEmptyLinks_WhenParentIdIsNegative()
        {
            var page = CreateLegalPage(webPageItemId: 1, parentId: -5);

            SetupCurrentPage(page);

            var result = await _service.GetLegalPageDtoAsync(CancellationToken.None);

            Assert.That(result.Links, Is.Empty);
        }

        [Test]
        public async Task GetLegalPageDtoAsync_ReturnsEmptyLinks_WhenNoSiblingsFound()
        {
            var page = CreateLegalPage(webPageItemId: 1, parentId: 10);

            SetupCurrentPage(page);
            SetupNoSiblings(parentId: 10);

            var result = await _service.GetLegalPageDtoAsync(CancellationToken.None);

            Assert.That(result.Links, Is.Empty);
        }

        // ── Links – happy path ───────────────────────────────────────────────

        [Test]
        public async Task GetLegalPageDtoAsync_ReturnsMappedLinks_WhenSiblingsExist()
        {
            var page = CreateLegalPage(webPageItemId: 1, parentId: 10);

            var sibling1 = CreateLegalPage(webPageItemId: 2, parentId: 10, title: "Privacy");
            var sibling2 = CreateLegalPage(webPageItemId: 3, parentId: 10, title: "Terms");

            SetupCurrentPage(page);
            SetupSiblings(parentId: 10, new[] { sibling1, sibling2 });
            SetupUrl(webPageItemId: 2, relativeUrl: "~/legal/privacy");
            SetupUrl(webPageItemId: 3, relativeUrl: "~/legal/terms");

            var result = await _service.GetLegalPageDtoAsync(CancellationToken.None);

            Assert.That(result.Links.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task GetLegalPageDtoAsync_LinkTitles_MappedFromMetadataTitle()
        {
            var page = CreateLegalPage(webPageItemId: 1, parentId: 10);
            var sibling = CreateLegalPage(webPageItemId: 2, parentId: 10, title: "Cookie Policy");

            SetupCurrentPage(page);
            SetupSiblings(parentId: 10, new[] { sibling });
            SetupUrl(webPageItemId: 2, relativeUrl: "~/legal/cookies");

            var result = await _service.GetLegalPageDtoAsync(CancellationToken.None);

            Assert.That(result.Links.First().Title, Is.EqualTo("Cookie Policy"));
        }

        [Test]
        public async Task GetLegalPageDtoAsync_LinkUrls_UsesRelativePathFromRetriever()
        {
            var page = CreateLegalPage(webPageItemId: 1, parentId: 10);
            var sibling = CreateLegalPage(webPageItemId: 2, parentId: 10, title: "Terms");

            SetupCurrentPage(page);
            SetupSiblings(parentId: 10, new[] { sibling });
            SetupUrl(webPageItemId: 2, relativeUrl: "~/legal/terms");

            var result = await _service.GetLegalPageDtoAsync(CancellationToken.None);

            Assert.That(result.Links.First().URL, Is.EqualTo("~/legal/terms"));
        }

        [Test]
        public async Task GetLegalPageDtoAsync_LinkUrls_MappedFromRetriever()
        {
            var page = CreateLegalPage(webPageItemId: 1, parentId: 10);
            var sibling = CreateLegalPage(webPageItemId: 2, parentId: 10, title: "Terms");

            SetupCurrentPage(page);
            SetupSiblings(parentId: 10, new[] { sibling });
            SetupUrl(webPageItemId: 2, relativeUrl: "~/legal/terms");

            var result = await _service.GetLegalPageDtoAsync(CancellationToken.None);

            Assert.That(result.Links.First().URL, Is.EqualTo("~/legal/terms"));
        }

        // ── Url retriever – siblings without resolvable URL excluded ────────

        [Test]
        public async Task GetLegalPageDtoAsync_PropagatesException_WhenUrlRetrieverThrows()
        {
            var page = CreateLegalPage(webPageItemId: 1, parentId: 10);
            var sibling = CreateLegalPage(webPageItemId: 2, parentId: 10, title: "Terms");

            SetupCurrentPage(page);
            SetupSiblings(parentId: 10, new[] { sibling });

            _webPageUrlRetrieverMock
                .Setup(x => x.Retrieve(2, It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Retriever error"));

            Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.GetLegalPageDtoAsync(CancellationToken.None));
        }

        // ── Url retriever called once per unique sibling ─────────────────────

        [Test]
        public async Task GetLegalPageDtoAsync_CallsUrlRetriever_ForEachSibling()
        {
            var page = CreateLegalPage(webPageItemId: 1, parentId: 10);
            var sibling1 = CreateLegalPage(webPageItemId: 2, parentId: 10, title: "Page A");
            var sibling2 = CreateLegalPage(webPageItemId: 3, parentId: 10, title: "Page B");

            SetupCurrentPage(page);
            SetupSiblings(parentId: 10, new[] { sibling1, sibling2 });
            SetupUrl(webPageItemId: 2, relativeUrl: "~/legal/a");
            SetupUrl(webPageItemId: 3, relativeUrl: "~/legal/b");

            await _service.GetLegalPageDtoAsync(CancellationToken.None);

            _webPageUrlRetrieverMock.Verify(
                x => x.Retrieve(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()),
                Times.Exactly(2));
        }

        [Test]
        public async Task GetLegalPageDtoAsync_DoesNotCallUrlRetriever_WhenNoSiblings()
        {
            var page = CreateLegalPage(webPageItemId: 1, parentId: 10);

            SetupCurrentPage(page);
            SetupNoSiblings(parentId: 10);

            await _service.GetLegalPageDtoAsync(CancellationToken.None);

            _webPageUrlRetrieverMock.Verify(
                x => x.Retrieve(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        // ── CancellationToken propagation ────────────────────────────────────

        [Test]
        public async Task GetLegalPageDtoAsync_PropagatesCancellationToken_ToRetrieveCurrentPage()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            var page = CreateLegalPage(webPageItemId: 1, parentId: 0);
            SetupCurrentPage(page);

            await _service.GetLegalPageDtoAsync(token);

            _contentServiceMock.Verify(
                x => x.RetrieveCurrentPageAsync<LegalPage>(
                    It.IsAny<RetrieveCurrentPageParameters>(), token),
                Times.Once);
        }

        [Test]
        public async Task GetLegalPageDtoAsync_ThrowsOperationCancelled_WhenTokenCancelled()
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            var page = CreateLegalPage(webPageItemId: 1, parentId: 10);
            var sibling = CreateLegalPage(webPageItemId: 2, parentId: 10, title: "Terms");

            SetupCurrentPage(page);
            SetupSiblings(parentId: 10, new[] { sibling });

            _webPageUrlRetrieverMock
                .Setup(x => x.Retrieve(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), cts.Token))
                .ThrowsAsync(new OperationCanceledException());

            Assert.ThrowsAsync<OperationCanceledException>(
                () => _service.GetLegalPageDtoAsync(cts.Token));
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static LegalPage CreateLegalPage(
            int webPageItemId,
            int parentId,
            string? title = "Default Title",
            string? description = null)
        {
            return new LegalPage
            {
                SystemFields = new WebPageFields
                {
                    WebPageItemID = webPageItemId,
                    WebPageItemParentID = parentId
                },
                MetadataTitle = title!,
                MetadataDescription = description!
            };
        }

        private void SetupCurrentPage(LegalPage page)
        {
            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<LegalPage>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(page);
        }

        private void SetupNoSiblings(int parentId)
        {
            _contentServiceMock
                .Setup(x => x.GetChildPagesByParentGuidAsync<LegalPage>(
                    parentId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<LegalPage>());
        }

        private void SetupSiblings(int parentId, IEnumerable<LegalPage> siblings)
        {
            _contentServiceMock
                .Setup(x => x.GetChildPagesByParentGuidAsync<LegalPage>(
                    parentId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(siblings.ToList());
        }

        private void SetupUrl(int webPageItemId, string relativeUrl)
        {
            _webPageUrlRetrieverMock
                .Setup(x => x.Retrieve(webPageItemId, It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new WebPageUrl(relativeUrl, $"https://example.com{relativeUrl.TrimStart('~')}"));
        }
    }
}
