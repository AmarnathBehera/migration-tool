using Azure.Search.Documents;

using Kentico.Xperience.AzureSearch.Search;

namespace Kentico.Xperience.AzureSearch.Tests.Search;

/// <summary>
/// Tests for the <see cref="AzureSearchQueryClientService"/> class.
/// </summary>
[TestFixture]
[Category("Unit")]
internal class AzureSearchQueryClientServiceTests
{
    private const string SERVICE_ENDPOINT = "https://test-search.search.windows.net";
    private const string QUERY_API_KEY = "test-query-api-key";
    private const string DEFAULT_INDEX = "SimpleIndex";


    private static AzureSearchQueryClientOptions CreateOptions() =>
        new(SERVICE_ENDPOINT, QUERY_API_KEY);


    [Test]
    public void CreateSearchClientForQueries_WithValidParameters_ReturnsSearchClient()
    {
        var service = new AzureSearchQueryClientService(CreateOptions());

        var result = service.CreateSearchClientForQueries(DEFAULT_INDEX);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<SearchClient>());
    }


    [Test]
    public void CreateSearchClientForQueries_InitializesWithCorrectEndpoint()
    {
        var service = new AzureSearchQueryClientService(CreateOptions());

        var result = service.CreateSearchClientForQueries(DEFAULT_INDEX);

        Assert.That(result.Endpoint.ToString(), Is.EqualTo(SERVICE_ENDPOINT + "/"));
    }


    [Test]
    public void CreateSearchClientForQueries_InitializesWithCorrectIndexName()
    {
        var service = new AzureSearchQueryClientService(CreateOptions());

        var result = service.CreateSearchClientForQueries(DEFAULT_INDEX);

        Assert.That(result.IndexName, Is.EqualTo(DEFAULT_INDEX));
    }


    [Test]
    public void Constructor_WithClientOptions_DoesNotThrow()
    {
        var clientOptions = new SearchClientOptions();

        Assert.That(() => new AzureSearchQueryClientService(CreateOptions(), clientOptions), Throws.Nothing);
    }


    [Test]
    public void CreateSearchClientForQueries_WithCustomClientOptions_ReturnsSearchClient()
    {
        var clientOptions = new SearchClientOptions
        {
            Retry = { MaxRetries = 7 }
        };
        var service = new AzureSearchQueryClientService(CreateOptions(), clientOptions);

        var result = service.CreateSearchClientForQueries(DEFAULT_INDEX);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<SearchClient>());
    }


    [Test]
    public void CreateSearchClientForQueries_WithCustomClientOptions_InitializesWithCorrectEndpoint()
    {
        var service = new AzureSearchQueryClientService(CreateOptions(), new SearchClientOptions());

        var result = service.CreateSearchClientForQueries(DEFAULT_INDEX);

        Assert.That(result.Endpoint.ToString(), Is.EqualTo(SERVICE_ENDPOINT + "/"));
    }


    [Test]
    public void CreateSearchClientForQueries_WithCustomClientOptions_InitializesWithCorrectIndexName()
    {
        var service = new AzureSearchQueryClientService(CreateOptions(), new SearchClientOptions());

        var result = service.CreateSearchClientForQueries(DEFAULT_INDEX);

        Assert.That(result.IndexName, Is.EqualTo(DEFAULT_INDEX));
    }
}
