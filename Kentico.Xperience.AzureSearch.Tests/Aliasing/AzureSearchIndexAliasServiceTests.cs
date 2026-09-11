using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;

using Kentico.Xperience.AzureSearch.Aliasing;

using NSubstitute;

namespace Kentico.Xperience.AzureSearch.Tests.Aliasing;

[TestFixture]
[Category("Unit")]
internal class AzureSearchIndexAliasServiceTests
{
    private const string TEST_INDEX_NAME = "test-index";
    private const string ALIAS_NAME = "test-alias";
    private const string NEW_ALIAS_NAME = "new-test-alias";


    [Test]
    public async Task CreateAlias_WithValidParameters_CreatesAlias()
    {
        var mockIndexClient = Substitute.For<SearchIndexClient>();
        var service = new AzureSearchIndexAliasService(mockIndexClient);
        var alias = new SearchAlias(ALIAS_NAME, [TEST_INDEX_NAME]);
        var cancellationToken = CancellationToken.None;

        await service.CreateAlias(alias, cancellationToken);

        await mockIndexClient.Received(1).CreateOrUpdateAliasAsync(alias, cancellationToken: cancellationToken);
    }


    [Test]
    public void CreateAlias_WithNullAlias_ThrowsArgumentNullException()
    {
        var mockIndexClient = Substitute.For<SearchIndexClient>();
        var service = new AzureSearchIndexAliasService(mockIndexClient);

        Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await service.CreateAlias(null!, CancellationToken.None));
    }


    [Test]
    public async Task EditAlias_WithValidParameters_DeletesOldAndCreatesNew()
    {
        var mockIndexClient = Substitute.For<SearchIndexClient>();
        var service = new AzureSearchIndexAliasService(mockIndexClient);
        var newAlias = new SearchAlias(NEW_ALIAS_NAME, [TEST_INDEX_NAME]);
        var cancellationToken = CancellationToken.None;

        await service.EditAlias(ALIAS_NAME, newAlias, cancellationToken);

        await mockIndexClient.Received(1).DeleteAliasAsync(ALIAS_NAME);
        await mockIndexClient.Received(1).CreateOrUpdateAliasAsync(newAlias, cancellationToken: cancellationToken);
    }


    [Test]
    public void EditAlias_WithNullOldAliasName_ThrowsArgumentNullException()
    {
        var mockIndexClient = Substitute.For<SearchIndexClient>();
        var service = new AzureSearchIndexAliasService(mockIndexClient);
        var newAlias = new SearchAlias(NEW_ALIAS_NAME, [TEST_INDEX_NAME]);

        Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await service.EditAlias(null!, newAlias, CancellationToken.None));
    }


    [Test]
    public void EditAlias_WithEmptyOldAliasName_ThrowsArgumentNullException()
    {
        var mockIndexClient = Substitute.For<SearchIndexClient>();
        var service = new AzureSearchIndexAliasService(mockIndexClient);
        var newAlias = new SearchAlias(NEW_ALIAS_NAME, [TEST_INDEX_NAME]);

        Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await service.EditAlias(string.Empty, newAlias, CancellationToken.None));
    }


    [Test]
    public async Task DeleteAlias_WithValidParameters_DeletesAlias()
    {
        var mockIndexClient = Substitute.For<SearchIndexClient>();
        var service = new AzureSearchIndexAliasService(mockIndexClient);
        var cancellationToken = CancellationToken.None;

        await service.DeleteAlias(ALIAS_NAME, cancellationToken);

        await mockIndexClient.Received(1).DeleteAliasAsync(ALIAS_NAME);
    }


    [Test]
    public void DeleteAlias_WithNullAliasName_ThrowsArgumentNullException()
    {
        var mockIndexClient = Substitute.For<SearchIndexClient>();
        var service = new AzureSearchIndexAliasService(mockIndexClient);

        Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await service.DeleteAlias(null!, CancellationToken.None));
    }


    [Test]
    public void DeleteAlias_WithEmptyAliasName_ThrowsArgumentNullException()
    {
        var mockIndexClient = Substitute.For<SearchIndexClient>();
        var service = new AzureSearchIndexAliasService(mockIndexClient);

        Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await service.DeleteAlias(string.Empty, CancellationToken.None));
    }
}
