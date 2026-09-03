using Azure;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Kentico.Xperience.AzureSearch.Indexing;

using NSubstitute.ExceptionExtensions;

namespace Kentico.Xperience.AzureSearch.Tests.Indexing;

[TestFixture]
[Category("Unit")]
internal class AzureSearchIndexClientServiceTests
{
    private const string TEST_FIELD_NAME = "field1";

    private SearchIndexClient mockIndexClient = null!;
    private IServiceProvider mockServiceProvider = null!;
    private AzureSearchIndexClientService service = null!;


    [SetUp]
    public void SetUp()
    {
        AzureSearchIndexStore.Instance.SetIndices([]);
        mockIndexClient = Substitute.For<SearchIndexClient>();
        mockServiceProvider = Substitute.For<IServiceProvider>();
        service = new AzureSearchIndexClientService(mockIndexClient, mockServiceProvider);
    }


    [Test]
    public void InitializeIndexClient_WithNonExistentIndex_ThrowsInvalidOperationException()
    {
        AzureSearchIndexStore.Instance.SetIndices([]);

        var exception = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await service.InitializeIndexClient("NonExistentIndex", CancellationToken.None));

        Assert.That(exception!.Message, Is.EqualTo("The index 'NonExistentIndex' is not registered."));
    }


    [Test]
    public async Task TryDeleteIndexIfExists_WithExistingIndex_ReturnsTrue()
    {
        var cancellationToken = CancellationToken.None;
        var indexName = "test-index";

        var mockIndex = new SearchIndex(indexName);
        var mockResponse = Response.FromValue(mockIndex, Substitute.For<Response>());
        mockIndexClient.GetIndexAsync(indexName, cancellationToken).Returns(mockResponse);

        var result = await service.TryDeleteIndexIfExists(indexName, onlyIfUnchanged: false, cancellationToken);

        Assert.That(result, Is.True);
        await mockIndexClient.Received(1).GetIndexAsync(indexName, cancellationToken);
        await mockIndexClient.Received(1).DeleteIndexAsync(Arg.Any<SearchIndex>(), false, cancellationToken);
    }


    [Test]
    public async Task TryDeleteIndexIfExists_WithNonExistentIndex_ReturnsFalse()
    {
        var cancellationToken = CancellationToken.None;
        var indexName = "non-existent-index";

        mockIndexClient.GetIndexAsync(indexName, cancellationToken)
            .Throws(new RequestFailedException(404, "Not Found"));

        var result = await service.TryDeleteIndexIfExists(indexName, onlyIfUnchanged: false, cancellationToken);

        Assert.That(result, Is.False);
        await mockIndexClient.Received(1).GetIndexAsync(indexName, cancellationToken);
        await mockIndexClient.DidNotReceive().DeleteIndexAsync(Arg.Any<SearchIndex>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }


    [TestCase(" ")]
    [TestCase("")]
    [TestCase(null)]
    public void TryDeleteIndexIfExists_WithNullIndexName_ThrowsArgumentException(string? invalidInput)
    {
        var exception = Assert.ThrowsAsync<ArgumentException>(async () =>
            await service.TryDeleteIndexIfExists(invalidInput!, onlyIfUnchanged: false, CancellationToken.None));

        Assert.That(exception!.ParamName, Is.EqualTo("indexName"));
    }


    [Test]
    public async Task TryDeleteIndexIfExists_WithOnlyIfUnchangedTrue_PassesCorrectParameter()
    {
        var cancellationToken = CancellationToken.None;
        var indexName = "test-index";

        var mockIndex = new SearchIndex(indexName);
        var mockResponse = Response.FromValue(mockIndex, Substitute.For<Response>());
        mockIndexClient.GetIndexAsync(indexName, cancellationToken).Returns(mockResponse);

        await service.TryDeleteIndexIfExists(indexName, onlyIfUnchanged: true, cancellationToken);

        await mockIndexClient.Received(1).DeleteIndexAsync(Arg.Any<SearchIndex>(), true, cancellationToken);
    }
}
