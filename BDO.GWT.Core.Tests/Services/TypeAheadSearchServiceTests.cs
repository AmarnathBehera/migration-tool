using BDO.GWT.AzureCognitiveSearch.Services;
using BDO.GWT.Core.Entities.Search;
using BDO.GWT.Core.Helpers;
using BDO.GWT.Core.Retrievers.Search;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.Xperience.Core.Services;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SearchDocument = BDO.GWT.Core.Entities.Search.SearchDocument;

namespace BDO.GWT.Core.Tests.Services
{
    public class TypeAheadSearchServiceTests
    {
        private const string LANGUAGE_TAG = "en-US";

        private Mock<ICultureContext> _cultureContextMock = null!;
        private Mock<IDateTimeProviderSearch> _dateTimeProviderMock = null!;
        private Mock<IPageUrlService> _pageUrlServiceMock = null!;
        private Mock<ISearchService> _searchServiceMock = null!;
        private TypeAheadSearchService _service = null!;

        [SetUp]
        public void Setup()
        {
            _cultureContextMock = new Mock<ICultureContext>();
            _dateTimeProviderMock = new Mock<IDateTimeProviderSearch>();
            _pageUrlServiceMock = new Mock<IPageUrlService>();
            _searchServiceMock = new Mock<ISearchService>();

            _cultureContextMock.SetupGet(x => x.CurrentLanguageTag).Returns(LANGUAGE_TAG);

            _service = new TypeAheadSearchService(
                _cultureContextMock.Object,
                _dateTimeProviderMock.Object,
                _pageUrlServiceMock.Object,
                _searchServiceMock.Object);
        }

        private void SetupSearchResults(List<SearchDocument> documents)
        {
            _searchServiceMock
                .Setup(x => x.SearchAsync(
                    It.IsAny<string>(),
                    It.IsAny<Action<SearchQuery<SearchDocument>>>(),
                    It.IsAny<CancellationToken>()))
                .Callback<string, Action<SearchQuery<SearchDocument>>?, CancellationToken>((_, decorateQuery, _) =>
                {
                    // Exercise the query-building delegate to increase coverage.
                    decorateQuery?.Invoke(new SearchQuery<SearchDocument>());
                })
                .ReturnsAsync(new SearchResponse<SearchDocument> { Documents = documents });
        }

        [Test]
        public async Task GetSuggestions_ReturnsTitleAndUrl_ForDocumentsWithMatchingPageUrl()
        {
            var documents = new List<SearchDocument>
            {
                new() { NodeId = 1, MetadataTitle = "Audit Services" },
                new() { NodeId = 2, MetadataTitle = "Tax Services" },
            };

            SetupSearchResults(documents);

            _pageUrlServiceMock
                .Setup(x => x.GetPageRelativeUrlByNodeIdAsync(
                    It.IsAny<IEnumerable<int>>(), LANGUAGE_TAG, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<int, RelativeLink>
                {
                    [1] = Link.Parse("/audit-services") as RelativeLink ?? new RelativeLink(),
                    [2] = Link.Parse("/tax-services") as RelativeLink ?? new RelativeLink(),
                });

            var results = (await _service.GetSuggestions("services", CancellationToken.None)).ToList();

            Assert.That(results, Has.Count.EqualTo(2));
            Assert.Multiple(() =>
            {
                Assert.That(results[0].Title, Is.EqualTo("Audit Services"));
                Assert.That(results[0].Url, Is.EqualTo("/audit-services"));
                Assert.That(results[1].Title, Is.EqualTo("Tax Services"));
                Assert.That(results[1].Url, Is.EqualTo("/tax-services"));
            });
        }

        [Test]
        public async Task GetSuggestions_WhenNoDocumentsMatch_ReturnsEmptyCollection()
        {
            SetupSearchResults(new List<SearchDocument>());

            _pageUrlServiceMock
                .Setup(x => x.GetPageRelativeUrlByNodeIdAsync(
                    It.IsAny<IEnumerable<int>>(), LANGUAGE_TAG, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<int, RelativeLink>());

            var results = await _service.GetSuggestions("no-match", CancellationToken.None);

            Assert.That(results, Is.Empty);
        }

        [Test]
        public async Task GetSuggestions_ExcludesDocumentsWithoutResolvedUrl()
        {
            var documents = new List<SearchDocument>
            {
                new() { NodeId = 1, MetadataTitle = "Has URL" },
                new() { NodeId = 2, MetadataTitle = "No URL" },
            };

            SetupSearchResults(documents);

            _pageUrlServiceMock
                .Setup(x => x.GetPageRelativeUrlByNodeIdAsync(
                    It.IsAny<IEnumerable<int>>(), LANGUAGE_TAG, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<int, RelativeLink>
                {
                    [1] = Link.Parse("/has-url") as RelativeLink ?? new RelativeLink(),
                });

            var results = (await _service.GetSuggestions("text", CancellationToken.None)).ToList();

            Assert.That(results, Has.Count.EqualTo(1));
            Assert.That(results[0].Title, Is.EqualTo("Has URL"));
        }

        [Test]
        public async Task GetSuggestions_WhenMetadataTitleIsNull_ReturnsEmptyStringTitle()
        {
            var documents = new List<SearchDocument>
            {
                new() { NodeId = 1, MetadataTitle = null! },
            };

            SetupSearchResults(documents);

            _pageUrlServiceMock
                .Setup(x => x.GetPageRelativeUrlByNodeIdAsync(
                    It.IsAny<IEnumerable<int>>(), LANGUAGE_TAG, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<int, RelativeLink>
                {
                    [1] = Link.Parse("/no-title") as RelativeLink ?? new RelativeLink(),
                });

            var results = (await _service.GetSuggestions("text", CancellationToken.None)).ToList();

            Assert.That(results, Has.Count.EqualTo(1));
            Assert.That(results[0].Title, Is.EqualTo(string.Empty));
        }

        [Test]
        public async Task GetSuggestions_PassesSearchTextToSearchService()
        {
            SetupSearchResults(new List<SearchDocument>());
            _pageUrlServiceMock
                .Setup(x => x.GetPageRelativeUrlByNodeIdAsync(
                    It.IsAny<IEnumerable<int>>(), LANGUAGE_TAG, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<int, RelativeLink>());

            await _service.GetSuggestions("audit", CancellationToken.None);

            _searchServiceMock.Verify(x => x.SearchAsync(
                "audit",
                It.IsAny<Action<SearchQuery<SearchDocument>>>(),
                CancellationToken.None), Times.Once);
        }

        [Test]
        public async Task GetSuggestions_UsesCurrentLanguageTagWhenResolvingPageUrls()
        {
            SetupSearchResults(new List<SearchDocument>());
            _pageUrlServiceMock
                .Setup(x => x.GetPageRelativeUrlByNodeIdAsync(
                    It.IsAny<IEnumerable<int>>(), LANGUAGE_TAG, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<int, RelativeLink>());

            await _service.GetSuggestions("audit", CancellationToken.None);

            _pageUrlServiceMock.Verify(x => x.GetPageRelativeUrlByNodeIdAsync(
                It.IsAny<IEnumerable<int>>(), LANGUAGE_TAG, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void GetSuggestions_WhenSearchServiceThrows_PropagatesException()
        {
            _searchServiceMock
                .Setup(x => x.SearchAsync(
                    It.IsAny<string>(),
                    It.IsAny<Action<SearchQuery<SearchDocument>>>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("search failure"));

            Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _service.GetSuggestions("text", CancellationToken.None));
        }

        [Test]
        public void GetSuggestions_WhenCancelled_PropagatesOperationCanceledException()
        {
            _searchServiceMock
                .Setup(x => x.SearchAsync(
                    It.IsAny<string>(),
                    It.IsAny<Action<SearchQuery<SearchDocument>>>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OperationCanceledException());

            Assert.ThrowsAsync<OperationCanceledException>(
                async () => await _service.GetSuggestions("text", new CancellationToken(true)));
        }
    }
}