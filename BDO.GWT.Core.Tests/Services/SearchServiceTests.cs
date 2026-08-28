using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;

using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;

using BDO.GWT.AzureCognitiveSearch.Services;
using BDO.GWT.Core.DTOs.AzureCognitiveSearch.Models;
using BDO.GWT.Core.Helpers;
using BDO.GWT.Core.Retrievers.Search;
using BDO.GWT.Xperience.Core.Services;

using Moq;

using SearchDocument = BDO.GWT.Core.Entities.Search.SearchDocument;
using CMS.Core;

namespace BDO.GWT.Core.Tests.Services
{
    public class SearchServiceTests
    {
        private const string SITE_NAME = "GlobalChannel";
        private const string SUGGESTER_NAME = "sg";

        private IMapper _mapper = null!;
        private Mock<ISearchClientBuilder> _searchClientBuilderMock = null!;
        private Mock<ISearchQueryHelper> _searchQueryHelperMock = null!;
        private Mock<ISiteContext> _siteContextMock = null!;
        private Mock<SearchClient> _searchClientMock = null!;
        private Mock<IEventLogService> _eventLogServiceMock = null!;
        private SearchService _service = null!;


        [SetUp]
        public void Setup()
        {
            var configuration = new MapperConfiguration(
                cfg => cfg.CreateMap<AzureSearchDocument, SearchDocument>(),
                NullLoggerFactory.Instance);
            _mapper = configuration.CreateMapper();

            _searchClientMock = new Mock<SearchClient>();
            _searchClientBuilderMock = new Mock<ISearchClientBuilder>();
            _searchQueryHelperMock = new Mock<ISearchQueryHelper>();
            _siteContextMock = new Mock<ISiteContext>();
            _eventLogServiceMock = new Mock<IEventLogService>();

            _siteContextMock.SetupGet(x => x.CurrentSiteName).Returns(SITE_NAME);
            _searchClientBuilderMock
                .Setup(x => x.CreateSearchClient(It.IsAny<string>()))
                .Returns(_searchClientMock.Object);
            _searchQueryHelperMock
                .Setup(x => x.MapSearchQueryToSearchOptions(It.IsAny<ISearchQuery<SearchDocument>>()))
                .Returns(new SearchOptions());

            SetupSearchResults([]);

            _service = new SearchService(
                _mapper,
                _searchClientBuilderMock.Object,
                _searchQueryHelperMock.Object,
                _siteContextMock.Object,
                _eventLogServiceMock.Object);
        }


        #region Helpers

        private static Response<SearchResults<AzureSearchDocument>> BuildResults(
            IEnumerable<AzureSearchDocument> documents,
            long? totalCount = null,
            IDictionary<string, IList<FacetResult>>? facets = null)
        {
            var values = documents
                .Select(document => SearchModelFactory.SearchResult(document, 1.0, null))
                .ToList();

            var results = SearchModelFactory.SearchResults(values, totalCount, facets, null, Mock.Of<Response>());

            return Response.FromValue(results, Mock.Of<Response>());
        }


        private void SetupSearchResults(
            IEnumerable<AzureSearchDocument> documents,
            long? totalCount = null,
            IDictionary<string, IList<FacetResult>>? facets = null)
        {
            _searchClientMock
                .Setup(x => x.SearchAsync<AzureSearchDocument>(
                    It.IsAny<string>(), It.IsAny<SearchOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(BuildResults(documents, totalCount, facets));
        }


        private static Response<SuggestResults<AzureSearchDocument>> BuildSuggestResults(
            IEnumerable<SearchSuggestion<AzureSearchDocument>>? suggestions = null)
        {
            var rawResponse = Mock.Of<Response>();

            IReadOnlyList<SearchSuggestion<AzureSearchDocument>> results =
                suggestions?.ToList() ?? new List<SearchSuggestion<AzureSearchDocument>>();

            var suggestResults = SearchModelFactory.SuggestResults<AzureSearchDocument>(
                results,
                coverage: null);

            return Response.FromValue(suggestResults, rawResponse);
        }


        private void SetupSuggestResults(IEnumerable<SearchSuggestion<AzureSearchDocument>>? suggestions = null)
        {
            _searchQueryHelperMock
                .Setup(x => x.MapSuggestSearchQueryToSuggestSearchOptions(
                    It.IsAny<ISuggestSearchQuery<SearchDocument>>()))
                .Returns(new SuggestOptions());

            _searchClientMock
                .Setup(x => x.SuggestAsync<AzureSearchDocument>(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<SuggestOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(BuildSuggestResults(suggestions));
        }

        #endregion

        #region Search text

        [Test]
        public async Task SearchAsync_PassesSearchTextToClientUnchanged()
        {
            await _service.SearchAsync<SearchDocument>("audit services");

            _searchClientMock.Verify(x => x.SearchAsync<AzureSearchDocument>(
                "audit services", It.IsAny<SearchOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }


        [TestCase("")]
        [TestCase("   ")]
        [TestCase("\t")]
        public async Task SearchAsync_WithEmptyOrWhitespaceSearchText_StillQueriesAzure(string searchText)
        {
            var response = await _service.SearchAsync<SearchDocument>(searchText);

            Assert.That(response, Is.Not.Null);
            _searchClientMock.Verify(x => x.SearchAsync<AzureSearchDocument>(
                searchText, It.IsAny<SearchOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }


        [Test]
        public async Task SearchAsync_WithNullSearchText_DoesNotThrow()
        {
            var response = await _service.SearchAsync<SearchDocument>(null!);

            Assert.That(response.Documents, Is.Empty);
        }

        #endregion

        #region Client construction and parameter pass-through

        [Test]
        public async Task SearchAsync_CreatesClientForCurrentSiteName()
        {
            await _service.SearchAsync<SearchDocument>("text");

            _searchClientBuilderMock.Verify(x => x.CreateSearchClient(SITE_NAME), Times.Once);
        }


        [Test]
        public async Task SearchAsync_PassesOptionsFromQueryHelperToClient()
        {
            var expectedOptions = new SearchOptions
            {
                Size = 25,
                Skip = 50,
                Filter = "ClassName eq 'BDO.Insight'"
            };
            _searchQueryHelperMock
                .Setup(x => x.MapSearchQueryToSearchOptions(It.IsAny<ISearchQuery<SearchDocument>>()))
                .Returns(expectedOptions);

            await _service.SearchAsync<SearchDocument>("text");

            _searchClientMock.Verify(x => x.SearchAsync<AzureSearchDocument>(
                It.IsAny<string>(),
                It.Is<SearchOptions>(o => o.Size == 25 && o.Skip == 50 && o.Filter == expectedOptions.Filter),
                It.IsAny<CancellationToken>()), Times.Once);
        }


        [Test]
        public async Task SearchAsync_InvokesDecorateQueryBeforeMappingOptions()
        {
            var wasInvoked = false;
            SearchQuery<SearchDocument>? decorated = null;

            await _service.SearchAsync<SearchDocument>("text", query =>
            {
                wasInvoked = true;
                decorated = query;
            });

            Assert.Multiple(() =>
            {
                Assert.That(wasInvoked, Is.True);
                Assert.That(decorated, Is.Not.Null);
            });
            _searchQueryHelperMock.Verify(x => x.MapSearchQueryToSearchOptions(decorated!), Times.Once);
        }


        [Test]
        public async Task SearchAsync_WithoutDecorateQuery_UsesDefaultQuery()
        {
            await _service.SearchAsync<SearchDocument>("text");

            _searchQueryHelperMock.Verify(x => x.MapSearchQueryToSearchOptions(
                It.IsAny<ISearchQuery<SearchDocument>>()), Times.Once);
        }


        [Test]
        public async Task SearchAsync_ForwardsCancellationTokenToClient()
        {
            using var cts = new CancellationTokenSource();

            await _service.SearchAsync<SearchDocument>("text", null, cts.Token);

            _searchClientMock.Verify(x => x.SearchAsync<AzureSearchDocument>(
                It.IsAny<string>(), It.IsAny<SearchOptions>(), cts.Token), Times.Once);
        }

        #endregion

        #region Result mapping

        [Test]
        public async Task SearchAsync_WithNoResults_ReturnsEmptyDocuments()
        {
            SetupSearchResults([]);

            var response = await _service.SearchAsync<SearchDocument>("no matches");

            Assert.Multiple(() =>
            {
                Assert.That(response.Documents, Is.Empty);
                Assert.That(response.Facets, Is.Empty);
            });
        }


        [Test]
        public async Task SearchAsync_WithNoResults_ReturnsNullOrZeroCount()
        {
            SetupSearchResults([], totalCount: 0);

            var response = await _service.SearchAsync<SearchDocument>("no matches");

            Assert.That(response.Count, Is.EqualTo(0));
        }


        [Test]
        public async Task SearchAsync_WithResults_MapsEveryDocument()
        {
            SetupSearchResults(
                [new AzureSearchDocument(), new AzureSearchDocument(), new AzureSearchDocument()],
                totalCount: 3);

            var response = await _service.SearchAsync<SearchDocument>("text");

            Assert.Multiple(() =>
            {
                Assert.That(response.Documents.Count, Is.EqualTo(3));
                Assert.That(response.Count, Is.EqualTo(3));
            });
        }


        [Test]
        public async Task SearchAsync_WhenTotalCountExceedsPageSize_ReturnsFullTotalCount()
        {
            SetupSearchResults([new AzureSearchDocument()], totalCount: 137);

            var response = await _service.SearchAsync<SearchDocument>("text");

            Assert.Multiple(() =>
            {
                Assert.That(response.Documents.Count(), Is.EqualTo(1));
                Assert.That(response.Count, Is.EqualTo(137));
            });
        }


        [Test]
        public async Task SearchAsync_WithNullFacets_ReturnsEmptyFacetDictionary()
        {
            SetupSearchResults([new AzureSearchDocument()], facets: null);

            var response = await _service.SearchAsync<SearchDocument>("text");

            Assert.That(response.Facets, Is.Not.Null.And.Empty);
        }

        #endregion

        #region Azure failures


        


        [Test]
        public void SearchAsync_WhenCancelled_PropagatesOperationCanceledException()
        {
            _searchClientMock
                .Setup(x => x.SearchAsync<AzureSearchDocument>(
                    It.IsAny<string>(), It.IsAny<SearchOptions>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OperationCanceledException());

            Assert.ThrowsAsync<OperationCanceledException>(async () =>
                await _service.SearchAsync<SearchDocument>("text", null, new CancellationToken(true)));
        }


        [Test]
        public void SearchAsync_WhenClientBuilderFails_PropagatesException()
        {
            _searchClientBuilderMock
                .Setup(x => x.CreateSearchClient(It.IsAny<string>()))
                .Throws(new InvalidOperationException("Missing query key"));

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _service.SearchAsync<SearchDocument>("text"));
        }

        #endregion

        #region Suggest

        [Test]
        public async Task SuggestAsync_PassesSearchTextAndSuggesterNameToClient()
        {
            SetupSuggestResults();

            var response = await _service.SuggestAsync<SearchDocument>("aud", SUGGESTER_NAME);

            Assert.That(response.Results, Is.Empty);
            _searchClientMock.Verify(x => x.SuggestAsync<AzureSearchDocument>(
                "aud", SUGGESTER_NAME, It.IsAny<SuggestOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion
    }
}